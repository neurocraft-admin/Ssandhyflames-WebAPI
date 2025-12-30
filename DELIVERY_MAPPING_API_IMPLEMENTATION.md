# Delivery Mapping API - Implementation Summary

## ? Implementation Complete

### Files Created

1. **Models/DeliveryMappingModel.cs**
   - `CommercialItemModel` - Commercial items with remaining quantities
   - `CustomerMappingModel` - Customer mapping details
   - `DeliveryMappingSummaryModel` - Summary with mapped/unmapped counts
   - `CreateCustomerMappingRequest` - Request model for creating mappings

2. **Helpers/DeliveryMappingSqlHelper.cs**
   - `GetCommercialItemsByDeliveryAsync()` - Calls sp_GetCommercialItemsByDelivery
   - `GetMappingsByDeliveryAsync()` - Calls sp_GetMappingsByDelivery
   - `GetDeliveryMappingSummaryAsync()` - Calls sp_GetDeliveryMappingSummary
   - `CreateCustomerMappingAsync()` - Calls sp_CreateCustomerMapping (with credit integration)
   - `DeleteCustomerMappingAsync()` - Calls sp_DeleteCustomerMapping (reverses credit)

3. **Routes/DeliveryMappingRoutes.cs**
   - Implements all 5 RESTful endpoints
   - Follows existing architectural pattern
   - Full validation and error handling

4. **Program.cs** (Updated)
   - Added `app.MapDeliveryMappingRoutes();` registration

---

## ?? API Endpoints

### 1. GET /api/delivery-mapping/commercial-items/{deliveryId}
**Description:** Get commercial items for a delivery with remaining quantities  
**Parameters:** deliveryId (int) - route parameter  
**Swagger Tag:** Delivery Mapping

**Response Example:**
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
    "mappedQuantity": 8,
    "remainingQuantity": 12,
    "sellingPrice": 1200.00
  }
]
```

**Business Logic:**
- Returns only items from "Commercial" category
- `MappedQuantity` = sum of all customer mappings for this item
- `RemainingQuantity` = `NoOfCylinders` - `MappedQuantity`

---

### 2. GET /api/delivery-mapping/delivery/{deliveryId}
**Description:** Get all customer mappings for a delivery  
**Parameters:** deliveryId (int) - route parameter  
**Swagger Tag:** Delivery Mapping

**Response Example:**
```json
[
  {
    "mappingId": 25,
    "deliveryId": 5,
    "productId": 3,
    "productName": "19kg Commercial Cylinder",
    "customerId": 8,
    "customerName": "Raj Restaurant",
    "quantity": 5,
    "sellingPrice": 1200.00,
    "totalAmount": 6000.00,
    "isCreditSale": true,
    "paymentMode": "Credit",
    "invoiceNumber": "INV-2025-123",
    "remarks": "Monthly supply",
    "createdAt": "2025-12-21T10:30:00"
  }
]
```

---

### 3. GET /api/delivery-mapping/summary/{deliveryId}
**Description:** Get delivery mapping summary with mapped/unmapped counts  
**Parameters:** deliveryId (int) - route parameter  
**Swagger Tag:** Delivery Mapping

**Response Example:**
```json
{
  "deliveryId": 5,
  "deliveryDate": "2025-12-21T00:00:00",
  "driverName": "Ramesh Kumar",
  "vehicleNo": "KA-01-AB-1234",
  "totalCommercialCylinders": 20,
  "mappedCylinders": 8,
  "unmappedCylinders": 12
}
```

**Returns 404 if delivery not found**

---

### 4. POST /api/delivery-mapping
**Description:** Create customer mapping (with automatic credit integration)  
**Swagger Tag:** Delivery Mapping

**Request Body:**
```json
{
  "deliveryId": 5,
  "productId": 3,
  "customerId": 8,
  "quantity": 5,
  "isCreditSale": true,
  "paymentMode": "Credit",
  "invoiceNumber": "INV-2025-123",
  "remarks": "Monthly supply for restaurant"
}
```

**Validations:**
- `deliveryId` must be > 0
- `productId` must be > 0
- `customerId` must be > 0
- `quantity` must be > 0
- `paymentMode` is required (Cash/Credit/Card/UPI)

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "Customer mapping created successfully"
}
```

**Error Responses (400 Bad Request):**
```json
{
  "success": false,
  "message": "Quantity exceeds available cylinders"
}
```
```json
{
  "success": false,
  "message": "Insufficient credit available"
}
```

**Business Logic - What Happens:**
1. Validates delivery, item, and customer exist
2. Gets selling price from DailyDeliveryItems (frozen at delivery time)
3. Checks if quantity exceeds remaining unmapped cylinders
4. Calculates `TotalAmount` = `SellingPrice` × `Quantity`
5. Inserts record in `DailyDeliveryCustomerMapping`
6. **IF `IsCreditSale` = true:**
   - Calls `sp_AddCreditUsage` to update `CustomerCredit`
   - Inserts transaction in `CreditTransactions` (type: Debit)
   - Increases customer's `CreditUsed` and `OutstandingAmount`
   - Decreases `CreditAvailable`

---

### 5. DELETE /api/delivery-mapping/{mappingId}
**Description:** Delete customer mapping (reverses credit if it was a credit sale)  
**Parameters:** mappingId (int) - route parameter  
**Swagger Tag:** Delivery Mapping

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "Mapping deleted successfully"
}
```

**Error Response (400 Bad Request):**
```json
{
  "success": false,
  "message": "Mapping not found"
}
```

**Business Logic - What Happens:**
1. Gets mapping details (CustomerId, TotalAmount, IsCreditSale)
2. **IF `IsCreditSale` = true:**
   - Reverses credit usage in `CustomerCredit`
   - Decreases `CreditUsed`
   - Increases `CreditAvailable`
   - Decreases `OutstandingAmount`
3. Deletes record from `DailyDeliveryCustomerMapping`

---

## ?? Credit Integration

### Automatic Credit Management

When `IsCreditSale = true` in **Create Mapping**, the stored procedure:
1. ? Calls `sp_AddCreditUsage` automatically
2. ? Validates customer has sufficient credit available
3. ? Updates `CustomerCredit` table:
   - `CreditUsed` += TotalAmount
   - `CreditAvailable` -= TotalAmount
   - `OutstandingAmount` += TotalAmount
4. ? Creates transaction in `CreditTransactions` (TransactionType: "Debit")

### Credit Reversal on Delete

When **Deleting a Credit Sale Mapping**, the stored procedure:
1. ? Gets original mapping details
2. ? Updates `CustomerCredit` table:
   - `CreditUsed` -= TotalAmount
   - `CreditAvailable` += TotalAmount
   - `OutstandingAmount` -= TotalAmount
3. ? Deletes the mapping record

### Example Credit Flow:

**Initial State:**
```
Customer: Raj Restaurant
Credit Limit:        ?50,000
Credit Used:  ?20,000
Credit Available:    ?30,000
Outstanding Amount:  ?20,000
```

**After Creating Credit Sale (5 cylinders @ ?1,200 = ?6,000):**
```
Credit Limit:        ?50,000
Credit Used:         ?26,000  (+6,000)
Credit Available:    ?24,000  (-6,000)
Outstanding Amount:  ?26,000  (+6,000)
```

**After Deleting the Mapping:**
```
Credit Limit:  ?50,000
Credit Used: ?20,000  (-6,000)
Credit Available:    ?30,000  (+6,000)
Outstanding Amount:  ?20,000  (-6,000)
```

---

## ?? Key Features Implemented

? **Follows Existing Architecture**
- Uses Routes pattern (not Controllers)
- Implements SqlHelper pattern for database operations
- Uses async/await throughout
- Proper error handling and logging
- Consistent response formats

? **Data Types**
- All amounts use `decimal(18,2)` precision
- Nullable fields: InvoiceNumber, Remarks
- DateTime for all date fields
- int for all ID fields

? **Business Logic**
- Commercial items filtering (only "Commercial" category)
- Remaining quantity calculation
- Selling price frozen at delivery time
- Credit validation before mapping
- Automatic credit integration

? **Error Handling**
- Try-catch blocks in all endpoints
- Console logging for debugging
- Proper HTTP status codes (200, 400, 404, 500)
- Descriptive error messages
- SQL exception handling

? **Swagger Integration**
- All endpoints tagged "Delivery Mapping"
- Named endpoints for easy identification
- Response type annotations
- Status code documentation

---

## ?? Testing Checklist

### Test the following endpoints:

1. **GET /api/delivery-mapping/commercial-items/{deliveryId}**
   - [ ] Returns only commercial category items
   - [ ] Shows correct RemainingQuantity
   - [ ] Returns empty array if no commercial items
   - [ ] Returns 500 on database error

2. **GET /api/delivery-mapping/delivery/{deliveryId}**
   - [ ] Returns all mappings for the delivery
- [ ] Shows correct TotalAmount calculation
   - [ ] Displays IsCreditSale flag correctly
   - [ ] Returns empty array if no mappings

3. **GET /api/delivery-mapping/summary/{deliveryId}**
   - [ ] Returns correct summary counts
   - [ ] MappedCylinders + UnmappedCylinders = TotalCommercialCylinders
   - [ ] Returns 404 for non-existent delivery
   - [ ] Shows driver and vehicle information

4. **POST /api/delivery-mapping**
   - [ ] Creates mapping successfully
   - [ ] Validates all required fields
   - [ ] Rejects quantity > RemainingQuantity
   - [ ] For Credit Sales:
     - [ ] Updates CustomerCredit table
     - [ ] Decreases CreditAvailable
     - [ ] Creates transaction in CreditTransactions
     - [ ] Rejects if insufficient credit
   - [ ] For Cash Sales:
     - [ ] Does not update credit tables
   - [ ] Accepts PaymentMode: Cash/Card/UPI

5. **DELETE /api/delivery-mapping/{mappingId}**
   - [ ] Deletes mapping successfully
   - [ ] For Credit Sales:
  - [ ] Reverses credit usage
     - [ ] Increases CreditAvailable
     - [ ] Decreases OutstandingAmount
   - [ ] Returns error for non-existent mapping
   - [ ] Returns 400 for invalid mappingId

---

## ?? Database Tables Involved

### DailyDeliveryCustomerMapping (Primary)
- MappingId (PK)
- DeliveryId (FK)
- ProductId (FK)
- CustomerId (FK)
- Quantity
- SellingPrice
- TotalAmount
- IsCreditSale
- PaymentMode
- InvoiceNumber (nullable)
- Remarks (nullable)
- CreatedAt

### CustomerCredit (Updated by sp_CreateCustomerMapping)
- CustomerId
- CreditLimit
- CreditUsed
- CreditAvailable
- OutstandingAmount
- TotalPaid

### CreditTransactions (Created by sp_CreateCustomerMapping)
- TransactionId
- CustomerId
- TransactionType ("Debit" for mapping)
- Amount
- ReferenceNumber
- Description
- TransactionDate

---

## ?? Stored Procedures Used

| Stored Procedure | Purpose | Returns |
|-----------------|---------|---------|
| `sp_GetCommercialItemsByDelivery` | Get commercial items with remaining qty | List of commercial items |
| `sp_GetMappingsByDelivery` | Get customer mappings for delivery | List of mappings |
| `sp_GetDeliveryMappingSummary` | Get summary with counts | Single summary row |
| `sp_CreateCustomerMapping` | Create mapping + credit integration | Success/message |
| `sp_DeleteCustomerMapping` | Delete mapping + reverse credit | Success/message |

---

## ?? Integration Points

### Frontend Integration
The Angular frontend already has:
- DeliveryMappingModel defined
- DeliveryMappingService with all API methods
- Mapping components
- Routes configured
- Link from Daily Delivery list

### Database Integration
All stored procedures are ready:
- Commercial items filtering logic
- Credit integration in sp_CreateCustomerMapping
- Credit reversal in sp_DeleteCustomerMapping
- Validation logic

### Customer Credit Integration
- Seamless integration with CustomerCredit module
- Automatic credit updates
- Transaction tracking
- Credit availability validation

---

## ?? Example Usage Scenarios

### Scenario 1: Cash Sale Mapping
```bash
POST /api/delivery-mapping
{
  "deliveryId": 5,
  "productId": 3,
  "customerId": 10,
  "quantity": 3,
  "isCreditSale": false,
  "paymentMode": "Cash",
  "invoiceNumber": "CASH-123",
  "remarks": "Immediate payment"
}
```
**Result:** Mapping created, no credit tables updated

---

### Scenario 2: Credit Sale Mapping
```bash
POST /api/delivery-mapping
{
  "deliveryId": 5,
  "productId": 3,
  "customerId": 8,
  "quantity": 5,
  "isCreditSale": true,
  "paymentMode": "Credit",
  "invoiceNumber": null,
"remarks": "30 days credit"
}
```
**Result:** 
- Mapping created
- CreditUsed increased
- CreditAvailable decreased
- Transaction created

---

### Scenario 3: Delete Credit Sale
```bash
DELETE /api/delivery-mapping/25
```
**Result:**
- Mapping deleted
- Credit usage reversed
- CreditAvailable restored

---

## ? Status: READY FOR TESTING

The Delivery Mapping API is fully implemented and ready for testing with the Angular frontend.

**Build Status:** ? **SUCCESSFUL**

All endpoints follow the existing architecture pattern and integrate seamlessly with the Customer Credit system.
