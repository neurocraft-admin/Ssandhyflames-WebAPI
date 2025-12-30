# Delivery Mapping API - Quick Test Guide

## ?? Swagger Testing Commands

### 1?? Get Commercial Items for Delivery

**Endpoint:** `GET /api/delivery-mapping/commercial-items/5`

**Expected Result:**
```json
[
  {
    "deliveryId": 5,
    "productId": 3,
    "productName": "19kg Commercial Cylinder",
    "categoryName": "Commercial Cylinder",
    "noOfCylinders": 20,
    "noOfInvoices": 15,
  "noOfDeliveries": 15,
    "mappedQuantity": 0,
    "remainingQuantity": 20,
    "sellingPrice": 1200.00
  }
]
```

---

### 2?? Create Cash Sale Mapping

**Endpoint:** `POST /api/delivery-mapping`

```json
{
  "deliveryId": 5,
  "productId": 3,
  "customerId": 1,
  "quantity": 3,
  "isCreditSale": false,
  "paymentMode": "Cash",
  "invoiceNumber": "CASH-001",
  "remarks": "Cash payment received"
}
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Customer mapping created successfully"
}
```

**Verify:**
- No changes in CustomerCredit table
- Mapping appears in GET /api/delivery-mapping/delivery/5

---

### 3?? Create Credit Sale Mapping

**Endpoint:** `POST /api/delivery-mapping`

```json
{
  "deliveryId": 5,
  "productId": 3,
  "customerId": 1,
  "quantity": 5,
  "isCreditSale": true,
  "paymentMode": "Credit",
  "invoiceNumber": null,
  "remarks": "Credit sale - 30 days"
}
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Customer mapping created successfully"
}
```

**Verify:**
? Check `GET /api/customer-credit/1` - CreditUsed increased  
? Check `GET /api/customer-credit/1` - CreditAvailable decreased  
? Check `GET /api/customer-credit/transactions/1` - New debit transaction  

---

### 4?? Get Mappings for Delivery

**Endpoint:** `GET /api/delivery-mapping/delivery/5`

**Expected Result:**
```json
[
  {
    "mappingId": 1,
    "deliveryId": 5,
    "productId": 3,
    "productName": "19kg Commercial Cylinder",
    "customerId": 1,
    "customerName": "ABC Restaurant",
    "quantity": 3,
    "sellingPrice": 1200.00,
    "totalAmount": 3600.00,
    "isCreditSale": false,
    "paymentMode": "Cash",
    "invoiceNumber": "CASH-001",
    "remarks": "Cash payment received",
    "createdAt": "2025-01-21T..."
  },
  {
 "mappingId": 2,
    "deliveryId": 5,
    "productId": 3,
    "productName": "19kg Commercial Cylinder",
    "customerId": 1,
    "customerName": "ABC Restaurant",
    "quantity": 5,
    "sellingPrice": 1200.00,
    "totalAmount": 6000.00,
    "isCreditSale": true,
    "paymentMode": "Credit",
    "invoiceNumber": null,
    "remarks": "Credit sale - 30 days",
    "createdAt": "2025-01-21T..."
  }
]
```

---

### 5?? Get Delivery Summary

**Endpoint:** `GET /api/delivery-mapping/summary/5`

**Expected Result:**
```json
{
  "deliveryId": 5,
  "deliveryDate": "2025-01-21T00:00:00",
  "driverName": "Ramesh Kumar",
  "vehicleNo": "KA-01-AB-1234",
  "totalCommercialCylinders": 20,
  "mappedCylinders": 8,
  "unmappedCylinders": 12
}
```

**Calculation Check:**
- `mappedCylinders` = sum of all quantities (3 + 5 = 8)
- `unmappedCylinders` = totalCommercialCylinders - mappedCylinders (20 - 8 = 12)

---

### 6?? Delete Credit Sale Mapping

**Endpoint:** `DELETE /api/delivery-mapping/2`

**Expected Response:**
```json
{
  "success": true,
  "message": "Mapping deleted successfully"
}
```

**Verify:**
? Check `GET /api/customer-credit/1` - CreditUsed decreased by 6000  
? Check `GET /api/customer-credit/1` - CreditAvailable increased by 6000  
? Check `GET /api/delivery-mapping/delivery/5` - Mapping removed  
? Check `GET /api/delivery-mapping/summary/5` - MappedCylinders updated  

---

## ? Error Scenarios to Test

### 1. Quantity Exceeds Available

```json
{
  "deliveryId": 5,
  "productId": 3,
  "customerId": 1,
  "quantity": 25,
  "isCreditSale": false,
  "paymentMode": "Cash"
}
```

**Expected Response (400):**
```json
{
  "success": false,
  "message": "Quantity exceeds available cylinders"
}
```

---

### 2. Insufficient Credit

Assuming customer has only ?5,000 credit available:

```json
{
  "deliveryId": 5,
  "productId": 3,
  "customerId": 1,
  "quantity": 10,
  "isCreditSale": true,
  "paymentMode": "Credit"
}
```

**Expected Response (400):**
```json
{
  "success": false,
  "message": "Insufficient credit available"
}
```

---

### 3. Invalid Delivery ID

**Endpoint:** `GET /api/delivery-mapping/commercial-items/99999`

**Expected:** Empty array `[]` or 500 error

---

### 4. Invalid Mapping ID for Delete

**Endpoint:** `DELETE /api/delivery-mapping/99999`

**Expected Response (400):**
```json
{
  "success": false,
  "message": "Mapping not found"
}
```

---

## ?? Complete Test Flow

### Step-by-Step Testing:

1. **Setup:**
   - Ensure delivery ID 5 exists with commercial items
   - Ensure customer ID 1 has sufficient credit (e.g., ?50,000)

2. **Get Initial State:**
   ```
   GET /api/delivery-mapping/commercial-items/5
   GET /api/customer-credit/1
   ```

3. **Create Cash Sale:**
   ```
   POST /api/delivery-mapping (3 cylinders, cash)
   GET /api/delivery-mapping/summary/5
   ```

4. **Create Credit Sale:**
 ```
   POST /api/delivery-mapping (5 cylinders, credit)
   GET /api/customer-credit/1 (verify credit decreased)
   GET /api/customer-credit/transactions/1 (verify transaction)
   ```

5. **View All Mappings:**
   ```
   GET /api/delivery-mapping/delivery/5
   ```

6. **Delete Credit Sale:**
   ```
   DELETE /api/delivery-mapping/{mappingId}
   GET /api/customer-credit/1 (verify credit restored)
   ```

7. **Final Verification:**
   ```
   GET /api/delivery-mapping/summary/5 (verify counts updated)
   ```

---

## ?? Key Points to Verify

### Credit Integration:
- ? Credit sale updates CustomerCredit table
- ? Cash sale does NOT update CustomerCredit table
- ? Delete reverses credit for credit sales only
- ? Transaction created in CreditTransactions for credit sales

### Quantity Validation:
- ? Cannot map more than RemainingQuantity
- ? RemainingQuantity = NoOfCylinders - MappedQuantity
- ? Summary shows correct mapped/unmapped counts

### Data Integrity:
- ? TotalAmount = SellingPrice × Quantity
- ? SellingPrice frozen from delivery time
- ? All nullable fields handled correctly
- ? Proper error messages for validation failures

---

## ?? Related Endpoints to Cross-Check

After delivery mapping operations, verify:

1. **Customer Credit:**
   - `GET /api/customer-credit/{customerId}`
   - `GET /api/customer-credit/transactions/{customerId}`

2. **Daily Delivery:**
   - `GET /api/dailydelivery/{deliveryId}`

3. **Products:**
   - `GET /api/products` (verify selling prices)

---

## ?? Sample Test Data

### Test Customer Setup:
```sql
-- Customer with good credit
CustomerId: 1
CustomerName: "ABC Restaurant"
CreditLimit: 50000
CreditUsed: 0
CreditAvailable: 50000
```

### Test Delivery Setup:
```sql
-- Delivery with commercial items
DeliveryId: 5
DeliveryDate: 2025-01-21
ProductId: 3 (19kg Commercial Cylinder)
NoOfCylinders: 20
SellingPrice: 1200.00
```

---

## ? Success Criteria

All tests pass when:
- ? Build successful
- ? All 5 endpoints accessible in Swagger
- ? Cash sales work without credit updates
- ? Credit sales update CustomerCredit correctly
- ? Delete reverses credit for credit sales
- ? Validation prevents over-mapping
- ? Validation prevents insufficient credit sales
- ? Summary shows accurate counts
- ? No errors in console/logs
