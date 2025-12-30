# ?? Item-Level Actuals - Quick Reference

## ?? 5 New Endpoints

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/dailydelivery/{id}/items/initialize` | POST | Create actuals for all items |
| `/api/dailydelivery/{id}/items/actuals` | GET | Get item actuals |
| `/api/dailydelivery/{id}/items/actuals` | PUT | Update item actuals |
| `/api/dailydelivery/{id}/with-items` | GET | Get delivery + items |
| `/api/dailydelivery/{id}/close-with-items` | PUT | Close with verification |

---

## ?? Quick Test

### 1. Initialize
```bash
POST /api/dailydelivery/5/items/initialize
```
**Expected:** `{ "success": true, "message": "..." }`

### 2. Get Actuals
```bash
GET /api/dailydelivery/5/items/actuals
```
**Expected:** Array of `ItemActualDto`

### 3. Update Actuals
```bash
PUT /api/dailydelivery/5/items/actuals
Content-Type: application/json

{
  "items": [
 {
      "productId": 3,
   "delivered": 18,
      "pending": 2,
      "cashCollected": 21600.00,
      "remarks": "2 pending"
    }
  ]
}
```
**Expected:** `{ "success": true, "message": "..." }`

### 4. Get With Items
```bash
GET /api/dailydelivery/5/with-items
```
**Expected:** `{ "delivery": {...}, "items": [...] }`

### 5. Close With Items
```bash
PUT /api/dailydelivery/5/close-with-items
Content-Type: application/json

{
  "returnTime": "17:30",
  "emptyCylindersReturned": 18,
  "remarks": "Completed"
}
```
**Expected:** `{ "success": true, "message": "..." }`

---

## ?? Models

### ItemActualDto
```csharp
{
  actualId: int,
  deliveryId: int,
  productId: int,
  productName: string,
  categoryName: string,
  plannedQuantity: int,
  deliveredQuantity: int,
  pendingQuantity: int,
  cashCollected: decimal,
  itemStatus: string,
  remarks: string?,
  updatedAt: DateTime,
  unitPrice: decimal,
  totalAmount: decimal
}
```

### UpdateItemActualsRequest
```csharp
{
  items: [
    {
      productId: int,
      delivered: int,
      pending: int,
      cashCollected: decimal,
      remarks: string?
    }
  ]
}
```

### CloseDeliveryWithItemsRequest
```csharp
{
  returnTime: string,
  emptyCylindersReturned: int,
  remarks: string?
}
```

---

## ?? Workflow

```
CREATE DELIVERY
     ?
INITIALIZE ITEMS (optional/automatic)
     ?
UPDATE ACTUALS (during delivery)
     ?
CLOSE WITH ITEMS (verification)
```

---

## ? Checklist

- [x] Models created (`DailyDeliveryItemActualsModel.cs`)
- [x] 5 endpoints added to `DailyDeliveryRoutes.cs`
- [x] Error handling implemented
- [x] Async/await used throughout
- [x] Swagger tags added
- [x] Build successful
- [ ] Database SPs created
- [ ] Test in Swagger
- [ ] Integration test

---

## ?? Swagger Tags

- **"Daily Delivery"** - Original endpoints
- **"Daily Delivery - Item Actuals"** - 5 new endpoints

---

**Status:** ? Ready for testing in Swagger!
