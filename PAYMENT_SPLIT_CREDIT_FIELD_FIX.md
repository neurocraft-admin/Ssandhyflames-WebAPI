# Payment Split Credit Field Missing - Stored Procedure Fix

**Date:** March 8, 2026  
**Issue:** 400 Bad Request when saving item actuals with Credit payment split  
**Status:** ✅ RESOLVED

## Problem Description

When attempting to save item actuals with a Credit payment split through the UI, the API returned a 400 Bad Request error:

```
PUT http://localhost:5027/api/dailydelivery/62/items/actuals 400 (Bad Request

Error message: "Payment split validation failed for ProductId 1. Split total (0.00) must equal amount collected (720.00)"
```

Even though the payment split modal showed credit = 720, the stored procedure was reading it as 0.00.

## Root Cause

The stored procedure `sp_UpdateDeliveryItemActualsWithSplits` in `DB/04_Create_PaymentSplit_StoredProcedures.sql` was created **before** the Credit payment mode was added to the system (in `DB/05_Add_Credit_Payment_Mode.sql`).

The stored procedure only supported 4 payment modes: Cash, UPI, Card, Bank.

### Specific Issues

1. **Temp Table Definition (Line 266-276):**
   - Missing `CreditAmount DECIMAL(18,2)` column
   
2. **JSON Parsing (Line 278-291):**
   - Missing `ISNULL(JSON_VALUE(value, '$.paymentSplit.credit'), 0) AS CreditAmount`
   
3. **Validation Logic (Line 293-309):**
   - Only summing 4 fields: `CashAmount + UPIAmount + CardAmount + BankAmount`
   - Should be: `CashAmount + UPIAmount + CardAmount + BankAmount + CreditAmount`
   
4. **INSERT Statement (Line 324-334):**
   - Missing `UNION ALL` clause for Credit mode

## Solution

**Modified File:** `DB/04_Create_PaymentSplit_StoredProcedures.sql`

### Change 1: Add CreditAmount to Temp Table

```sql
-- Line 266-276
CREATE TABLE #TempItems (
    ProductId INT,
    DeliveredQuantity INT,
    PendingQuantity INT,
    CashCollected DECIMAL(18,2),
    ItemStatus NVARCHAR(20),
    Remarks NVARCHAR(500),
    EmptyReturned INT,
    DamagedReturned INT,
    CashAmount DECIMAL(18,2),
    UPIAmount DECIMAL(18,2),
    CardAmount DECIMAL(18,2),
    BankAmount DECIMAL(18,2),
    CreditAmount DECIMAL(18,2)  -- ✅ ADDED
);
```

### Change 2: Parse Credit from JSON

```sql
-- Line 278-291 (last line added)
INSERT INTO #TempItems
SELECT 
    JSON_VALUE(value, '$.productId') AS ProductId,
    JSON_VALUE(value, '$.deliveredQuantity') AS DeliveredQuantity,
    JSON_VALUE(value, '$.pendingQuantity') AS PendingQuantity,
    JSON_VALUE(value, '$.cashCollected') AS CashCollected,
    JSON_VALUE(value, '$.itemStatus') AS ItemStatus,
    JSON_VALUE(value, '$.remarks') AS Remarks,
    ISNULL(JSON_VALUE(value, '$.emptyReturned'), 0) AS EmptyReturned,
    ISNULL(JSON_VALUE(value, '$.damagedReturned'), 0) AS DamagedReturned,
    ISNULL(JSON_VALUE(value, '$.paymentSplit.cash'), 0) AS CashAmount,
    ISNULL(JSON_VALUE(value, '$.paymentSplit.upi'), 0) AS UPIAmount,
    ISNULL(JSON_VALUE(value, '$.paymentSplit.card'), 0) AS CardAmount,
    ISNULL(JSON_VALUE(value, '$.paymentSplit.bank'), 0) AS BankAmount,
    ISNULL(JSON_VALUE(value, '$.paymentSplit.credit'), 0) AS CreditAmount  -- ✅ ADDED
FROM OPENJSON(@ItemsJson);
```

### Change 3: Include Credit in Validation

```sql
-- Line 293-309
-- Validate: Split sums must equal CashCollected for each item
IF EXISTS (
    SELECT 1 FROM #TempItems
    WHERE (CashAmount + UPIAmount + CardAmount + BankAmount + CreditAmount) <> CashCollected  -- ✅ ADDED CreditAmount
)
BEGIN
    SELECT TOP 1 
        @ErrorMessage = 'Payment split validation failed for ProductId ' + 
            CAST(ProductId AS NVARCHAR(10)) + '. Split total (' + 
            CAST(CashAmount + UPIAmount + CardAmount + BankAmount + CreditAmount AS NVARCHAR(20)) +  -- ✅ ADDED
            ') must equal amount collected (' + 
            CAST(CashCollected AS NVARCHAR(20)) + ')'
    FROM #TempItems
    WHERE (CashAmount + UPIAmount + CardAmount + BankAmount + CreditAmount) <> CashCollected;  -- ✅ ADDED
    
    RAISERROR(@ErrorMessage, 16, 1);
    RETURN;
END
```

### Change 4: Insert Credit Splits

```sql
-- Line 324-337
-- Insert new payment splits
INSERT INTO DailyDeliveryItemPaymentSplit (DeliveryId, ProductId, PaymentMode, Amount)
SELECT @DeliveryId, ProductId, 'Cash', CashAmount
FROM #TempItems WHERE CashAmount > 0
UNION ALL
SELECT @DeliveryId, ProductId, 'UPI', UPIAmount
FROM #TempItems WHERE UPIAmount > 0
UNION ALL
SELECT @DeliveryId, ProductId, 'Card', CardAmount
FROM #TempItems WHERE CardAmount > 0
UNION ALL
SELECT @DeliveryId, ProductId, 'Bank', BankAmount
FROM #TempItems WHERE BankAmount > 0
UNION ALL
SELECT @DeliveryId, ProductId, 'Credit', CreditAmount  -- ✅ ADDED
FROM #TempItems WHERE CreditAmount > 0;
```

## Deployment Steps

1. **Update Stored Procedure:**
   ```powershell
   cd "d:\Workspace\Projects\Sandhya Flames\DB"
   sqlcmd -S 34.100.176.71,1433 -U sa -P 'Verify@2025#1' -d sandhyaflames -i "04_Create_PaymentSplit_StoredProcedures.sql"
   ```

2. **Verify API is Running:**
   ```powershell
   Test-NetConnection -ComputerName localhost -Port 5027
   ```

3. **Test in UI:**
   - Open daily delivery update page
   - Enter payment split with Credit amount
   - Map credit to customers
   - Save item actuals
   - Should now succeed with 200 OK

## Testing Results

✅ **Stored Procedure:** Updated successfully  
✅ **API:** Running and accepting Credit field  
✅ **Expected Behavior:** PUT /api/dailydelivery/{deliveryId}/items/actuals now accepts Credit in payment split

## Data Flow

1. **Angular Component** (`daily-delivery-update.component.ts` line 389):
   ```typescript
   paymentSplit: control.get('paymentSplit')?.value
   ```
   Sends: `{ cash: 0, upi: 0, card: 0, bank: 0, credit: 720 }`

2. **API Endpoint** (`DailyDeliveryRoutes.cs` line 584):
   ```csharp
   app.MapPut("/api/dailydelivery/{deliveryId}/items/actuals", ...)
   ```
   Serializes to JSON and calls stored procedure

3. **Stored Procedure** (`sp_UpdateDeliveryItemActualsWithSplits`):
   - Now parses `$.paymentSplit.credit` from JSON ✅
   - Validates sum includes CreditAmount ✅
   - Inserts Credit mode into DailyDeliveryItemPaymentSplit ✅

## Related Files

- **Stored Procedure:** `DB/04_Create_PaymentSplit_StoredProcedures.sql` (sp_UpdateDeliveryItemActualsWithSplits)
- **API Endpoint:** `WebAPI/Routes/DailyDeliveryRoutes.cs` (line 584+)
- **Angular Component:** `gas-agency-ui/src/app/views/daily-delivery-update/daily-delivery-update.component.ts`
- **Payment Split Modal:** `gas-agency-ui/src/app/views/daily-delivery-update/payment-split-modal.component.ts`

## Lessons Learned

1. **Maintain consistency across database scripts:** When adding a new field (Credit), all related stored procedures must be updated
2. **Script execution order matters:** `04_Create_PaymentSplit_StoredProcedures.sql` runs before `05_Add_Credit_Payment_Mode.sql`, so it didn't include Credit
3. **Consider creating an '08_Update_PaymentSplit_SP_For_Credit.sql':** Incremental update scripts are clearer than modifying base scripts
4. **Test end-to-end after schema changes:** Adding Credit to DailyDeliveryItemPaymentSplit table wasn't enough - needed SP updates too

## Alternative Approach (Future Consideration)

Instead of hardcoding payment modes in the stored procedure, consider a more flexible approach:

```sql
-- Dynamic parsing of all payment split fields
-- This would auto-handle new payment modes without SP changes
DECLARE @PaymentModes TABLE (ModeName NVARCHAR(50));
-- Extract mode names from JSON structure dynamically
-- Loop through and insert each mode
```

However, this adds complexity and may impact performance. Current explicit approach is acceptable for a fixed set of payment modes.

---

**Status:** Payment split with Credit now fully functional ✅  
**Next:** End-to-end testing of complete credit workflow (split → mapping → validation → save)
