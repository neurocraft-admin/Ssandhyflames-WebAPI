# ?? Stock Register Integration Guide

## ?? CRITICAL INTEGRATIONS REQUIRED

The Stock Register endpoints are created, but you MUST integrate them with existing modules for automatic stock updates.

---

## 1?? Purchase Entry Integration

### Location: Routes/PurchaseRoutes.cs (or wherever purchases are created)

### Find your Purchase POST endpoint and add this code:

**AFTER** the purchase is successfully saved:

```csharp
// ? ADD THIS CODE after purchase is saved
try
{
    // Create stock update request
    var stockUpdateRequest = new
    {
        PurchaseId = savedPurchaseId,  // Your saved purchase ID
    ProductId = purchase.ProductId,
        Quantity = purchase.Quantity,
        Remarks = $"Purchase #{savedPurchaseId}"
    };

    // Call stock update endpoint
    using var httpClient = new HttpClient();
    httpClient.BaseAddress = new Uri("http://localhost:7183");  // Your API base URL
    
  var stockResponse = await httpClient.PostAsJsonAsync(
      "/api/stockregister/update-from-purchase",
     stockUpdateRequest
    );
    
    if (!stockResponse.IsSuccessStatusCode)
    {
   var errorContent = await stockResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"?? Stock update failed for Purchase #{savedPurchaseId}: {errorContent}");
        // Don't block purchase - just log the error
    }
    else
    {
        Console.WriteLine($"? Stock updated for Purchase #{savedPurchaseId}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"? Stock update error: {ex.Message}");
    // Don't throw - purchase should succeed even if stock update fails
}
```

### Example Full Purchase Endpoint:

```csharp
app.MapPost("/api/purchases", async (
    [FromBody] PurchaseRequest request,
    IConfiguration config) =>
{
    try
    {
      // 1. Save purchase to database
        using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
   using var cmd = new SqlCommand("sp_CreatePurchase", conn)
        {
    CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.AddWithValue("@ProductId", request.ProductId);
        cmd.Parameters.AddWithValue("@Quantity", request.Quantity);
        // ... other parameters

        await conn.OpenAsync();
        var savedPurchaseId = (int)await cmd.ExecuteScalarAsync();

        // 2. ? UPDATE STOCK (ADD THIS)
        try
        {
            var stockUpdateRequest = new
    {
    PurchaseId = savedPurchaseId,
        ProductId = request.ProductId,
 Quantity = request.Quantity,
    Remarks = $"Purchase #{savedPurchaseId}"
            };

      using var httpClient = new HttpClient();
     httpClient.BaseAddress = new Uri("http://localhost:7183");
          
       await httpClient.PostAsJsonAsync(
                "/api/stockregister/update-from-purchase",
        stockUpdateRequest
         );
 }
     catch (Exception ex)
        {
            Console.WriteLine($"Stock update failed: {ex.Message}");
        }

        return Results.Ok(new { success = true, purchaseId = savedPurchaseId });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { success = false, message = ex.Message });
    }
});
```

---

## 2?? Daily Delivery Integration

### Location: Routes/DailyDeliveryRoutes.cs

### A. When Delivery is CREATED (Stock OUT)

Find your **POST /api/dailydelivery** endpoint and add this **AFTER** delivery is created:

```csharp
// ? ADD THIS CODE after delivery is created
try
{
    using var httpClient = new HttpClient();
    httpClient.BaseAddress = new Uri("http://localhost:7183");
    
    var stockResponse = await httpClient.PostAsync(
        $"/api/stockregister/update-from-delivery/{deliveryId}",
null  // No body needed
    );
    
    if (!stockResponse.IsSuccessStatusCode)
    {
        Console.WriteLine($"?? Stock not deducted for Delivery #{deliveryId}");
    }
    else
    {
        Console.WriteLine($"? Stock deducted for Delivery #{deliveryId}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"? Stock deduction error: {ex.Message}");
}
```

### Example:

```csharp
app.MapPost("/api/dailydelivery", async (
    [FromBody] DailyDeliveryModel delivery,
    IConfiguration config) =>
{
    try
 {
        // 1. Create delivery
        using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
  // ... save delivery logic
        var deliveryId = await cmd.ExecuteScalarAsync();

        // 2. ? DEDUCT STOCK (ADD THIS)
        try
        {
         using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://localhost:7183");
     
            await httpClient.PostAsync(
      $"/api/stockregister/update-from-delivery/{deliveryId}",
          null
  );
        }
        catch (Exception ex)
        {
    Console.WriteLine($"Stock deduction failed: {ex.Message}");
     }

        return Results.Ok(new { deliveryId });
  }
    catch (Exception ex)
    {
   return Results.BadRequest(new { message = ex.Message });
    }
});
```

---

### B. When Delivery is CLOSED (Empty/Damaged Return)

Find your delivery **close** or **actuals update** endpoint and add this:

```csharp
// ? ADD THIS CODE when delivery is closed/updated with returns
try
{
    var returnRequest = new
    {
        EmptyCylindersReturned = actuals.EmptyCylindersReturned,
        DamagedCylinders = actuals.DamagedCylinders ?? 0  // Add this field if not present
    };

    using var httpClient = new HttpClient();
    httpClient.BaseAddress = new Uri("http://localhost:7183");
    
    var stockResponse = await httpClient.PostAsJsonAsync(
        $"/api/stockregister/update-from-return/{deliveryId}",
returnRequest
    );
    
 if (!stockResponse.IsSuccessStatusCode)
    {
  Console.WriteLine($"?? Empty stock not updated for Delivery #{deliveryId}");
    }
    else
    {
    Console.WriteLine($"? Empty stock updated for Delivery #{deliveryId}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"? Return stock update error: {ex.Message}");
}
```

### Example:

```csharp
app.MapPut("/api/dailydelivery/{id}/actuals", async (
    int id,
    [FromBody] DailyDeliveryActualsModel actuals,
    IConfiguration config) =>
{
    try
    {
      // 1. Update delivery actuals
        var dt = DailyDeliverySqlHelper.ExecuteDataTableSync(
   config, 
        "sp_UpdateDailyDeliveryActuals",
      new SqlParameter("@DeliveryId", id),
            // ... other parameters
        );

     // 2. ? UPDATE RETURN STOCK (ADD THIS)
 try
        {
        var returnRequest = new
            {
   EmptyCylindersReturned = actuals.EmptyCylindersReturned,
            DamagedCylinders = 0  // Add field to DailyDeliveryActualsModel if tracking damaged
         };

  using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://localhost:7183");
      
            await httpClient.PostAsJsonAsync(
    $"/api/stockregister/update-from-return/{id}",
          returnRequest
            );
        }
        catch (Exception ex)
        {
      Console.WriteLine($"Return stock update failed: {ex.Message}");
     }

        return Results.Ok(DailyDeliverySqlHelper.ToSerializableList(dt));
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});
```

---

## 3?? Alternative: Direct Database SP Calls

If you prefer **NOT** to use HTTP calls between endpoints, you can call the stock SPs directly:

### Example: Purchase Integration (Direct SP Call)

```csharp
// Instead of HTTP call, call SP directly
using var stockConn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
using var stockCmd = new SqlCommand("sp_UpdateStockFromPurchase", stockConn)
{
    CommandType = CommandType.StoredProcedure
};

stockCmd.Parameters.AddWithValue("@PurchaseId", savedPurchaseId);
stockCmd.Parameters.AddWithValue("@ProductId", request.ProductId);
stockCmd.Parameters.AddWithValue("@Quantity", request.Quantity);
stockCmd.Parameters.AddWithValue("@Remarks", $"Purchase #{savedPurchaseId}");

await stockConn.OpenAsync();
await stockCmd.ExecuteNonQueryAsync();
```

### Example: Delivery Integration (Direct SP Call)

```csharp
// Deduct stock when delivery created
using var stockConn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
using var stockCmd = new SqlCommand("sp_UpdateStockFromDeliveryAssignment", stockConn)
{
    CommandType = CommandType.StoredProcedure
};

stockCmd.Parameters.AddWithValue("@DeliveryId", deliveryId);

await stockConn.OpenAsync();
await stockCmd.ExecuteNonQueryAsync();
```

---

## ?? Stock Flow Diagram

```
????????????????????????????????????????????????????????????????
?           PURCHASE ENTRY        ?
?  User creates purchase ? sp_CreatePurchase        ?
?       ?               ?
?  PurchaseId = 5, ProductId = 3, Qty = 100        ?
??                ?
?  ? Call sp_UpdateStockFromPurchase      ?
?   ?        ?
?  FilledStock += 100            ?
?  Transaction: Type=Purchase, FilledChange=+100       ?
????????????????????????????????????????????????????????????????

????????????????????????????????????????????????????????????????
?      DELIVERY ASSIGNMENT  ?
?  User creates delivery ? sp_CreateDailyDelivery ?
?          ?   ?
?  DeliveryId = 10, Items: [{ProductId=3, Qty=20}]           ?
?          ?         ?
?  ? Call sp_UpdateStockFromDeliveryAssignment             ?
?            ? ?
?  FilledStock -= 20                ?
?  Transaction: Type=DeliveryOut, FilledChange=-20        ?
????????????????????????????????????????????????????????????????

????????????????????????????????????????????????????????????????
?             DELIVERY RETURN        ?
?  Driver returns ? sp_UpdateDailyDeliveryActuals  ?
?      ?  ?
?  EmptyReturned = 18, DamagedReturned = 2    ?
?            ?   ?
?  ? Call sp_UpdateStockFromDeliveryReturn        ?
?    ?        ?
?  EmptyStock += 18       ?
?  DamagedStock += 2     ?
?  Transaction: Type=DeliveryReturn           ?
????????????????????????????????????????????????????????????????
```

---

## ?? Testing Integration

### Test Purchase Integration:

1. Create a purchase:
```bash
POST /api/purchases
{
  "productId": 3,
  "quantity": 100,
  ...
}
```

2. Check stock increased:
```bash
GET /api/stockregister?productId=3
# FilledStock should be +100
```

3. Check transaction created:
```bash
GET /api/stockregister/transactions?productId=3
# Should show: Type=Purchase, FilledChange=+100
```

---

### Test Delivery Integration:

1. Create delivery:
```bash
POST /api/dailydelivery
{
  "items": [{ "productId": 3, "noOfCylinders": 20 }]
}
```

2. Check stock decreased:
```bash
GET /api/stockregister?productId=3
# FilledStock should be -20
```

3. Check transaction:
```bash
GET /api/stockregister/transactions?productId=3
# Should show: Type=DeliveryOut, FilledChange=-20
```

4. Close delivery with returns:
```bash
PUT /api/dailydelivery/5/actuals
{
  "emptyCylindersReturned": 18,
  ...
}
```

5. Check empty stock increased:
```bash
GET /api/stockregister?productId=3
# EmptyStock should be +18
```

---

## ? Integration Checklist

- [ ] Purchase Entry calls stock update SP
- [ ] Delivery Creation calls stock deduction SP
- [ ] Delivery Close calls return stock SP
- [ ] Console logs show stock updates
- [ ] Test purchase ? stock increases
- [ ] Test delivery ? stock decreases
- [ ] Test return ? empty stock increases
- [ ] All transactions logged

---

## ?? Common Issues

### Issue 1: Stock not updating after purchase
**Solution:** Check console logs for errors, verify SP exists, check connection string

### Issue 2: HttpClient base address error
**Solution:** Use your actual API URL, e.g., `https://localhost:7183`

### Issue 3: Stock updates but purchase fails
**Solution:** Wrap stock update in try-catch, don't throw errors that block main operation

### Issue 4: Circular dependency
**Solution:** Use HTTP calls between endpoints OR direct SP calls, never circular endpoint calls

---

## ?? Summary

**3 Integration Points:**
1. ? Purchase Entry ? Increase FilledStock
2. ? Delivery Assignment ? Decrease FilledStock
3. ? Delivery Return ? Increase EmptyStock/DamagedStock

**Choose Integration Method:**
- **Option A:** HTTP calls between endpoints (easier to test separately)
- **Option B:** Direct SP calls (faster, no HTTP overhead)

**Both methods work - choose based on your preference!**

---

**Status:** Ready for integration. Add the provided code snippets to your existing endpoints.
