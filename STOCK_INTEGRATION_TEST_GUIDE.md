# ?? Stock Integration - Quick Test Guide

## ? Prerequisites

1. Database SPs exist:
   - `sp_UpdateStockFromPurchase`
   - `sp_UpdateStockFromDeliveryAssignment`
   - `sp_UpdateStockFromDeliveryReturn`

2. Stock initialized:
```bash
POST /api/stockregister/initialize
```

---

## ?? 3-Step Test Sequence

### Step 1: Purchase (Stock IN)

**Request:**
```json
POST /api/purchases

{
  "vendorId": 1,
  "invoiceNo": "TEST-001",
  "purchaseDate": "2025-12-26",
  "items": [
    {
      "productId": 3,
      "categoryId": 1,
      "subCategoryId": 1,
 "qty": 100,
   "unitPrice": 500
    }
  ]
}
```

**Expected Console:**
```
? Stock updated for Product 3: Stock increased by 100 units
```

**Verify:**
```sql
SELECT FilledStock FROM StockRegister WHERE ProductId = 3;
-- Should show: 100
```

---

### Step 2: Delivery (Stock OUT)

**Request:**
```json
POST /api/dailydelivery

{
  "deliveryDate": "2025-12-26",
  "driverId": 2,
  "vehicleId": 3,
  "startTime": "09:00",
  "items": [
    { "productId": 3, "noOfCylinders": 20 }
  ]
}
```

**Expected Console:**
```
? Stock deducted for Delivery 1: Stock decreased by 20 units
```

**Verify:**
```sql
SELECT FilledStock FROM StockRegister WHERE ProductId = 3;
-- Should show: 80 (100 - 20)
```

---

### Step 3: Return (Empty IN)

**Request:**
```json
PUT /api/dailydelivery/1/actuals

{
  "returnTime": "17:30",
  "completedInvoices": 15,
  "pendingInvoices": 0,
  "cashCollected": 21600,
  "emptyCylindersReturned": 18,
  "remarks": "Completed"
}
```

**Expected Console:**
```
? Stock return updated for Delivery 1: Stock updated with 18 empty cylinders
```

**Verify:**
```sql
SELECT FilledStock, EmptyStock, TotalStock 
FROM StockRegister WHERE ProductId = 3;
-- Should show: 80 filled, 18 empty, 98 total
```

---

## ?? Complete Verification

```sql
-- Stock Summary
SELECT 
    p.ProductName,
    sr.FilledStock,
    sr.EmptyStock,
    (sr.FilledStock + sr.EmptyStock) AS TotalStock
FROM StockRegister sr
INNER JOIN Products p ON sr.ProductId = p.ProductId
WHERE p.ProductId = 3;

-- Transaction History
SELECT 
    TransactionType,
    FilledChange,
    EmptyChange,
    ReferenceType,
    ReferenceId,
    TransactionDate
FROM StockTransactions
WHERE ProductId = 3
ORDER BY TransactionDate;
```

**Expected Transactions:**
```
TransactionType    | FilledChange | EmptyChange | ReferenceType | ReferenceId
-------------------|--------------|-------------|---------------|------------
Purchase           | +100  | 0           | Purchase      | 1
DeliveryAssigned   | -20          | 0 | Delivery      | 1
DeliveryCompleted  | 0    | +18  | Delivery   | 1
```

---

## ? Success Criteria

- [x] Console shows all 3 success messages
- [x] FilledStock = 80
- [x] EmptyStock = 18
- [x] TotalStock = 98
- [x] 3 transactions logged

---

## ?? If Something Fails

### Console shows error:
1. Check error message
2. Verify SP exists in database
3. Test SP manually in SSMS
4. Use manual stock adjustment as fallback

### Stock not updating:
1. Check StockRegister table has record for ProductId
2. Run: `POST /api/stockregister/initialize`
3. Check database connection string

### Negative stock:
1. Initialize stock first
2. Or manually adjust:
```json
POST /api/stockregister/adjust
{
  "productId": 3,
  "filledChange": 100,
  "emptyChange": 0,
  "damagedChange": 0,
  "remarks": "Initial stock"
}
```

---

**Time to complete:** 5 minutes  
**Expected result:** ? All stock updates automatic
