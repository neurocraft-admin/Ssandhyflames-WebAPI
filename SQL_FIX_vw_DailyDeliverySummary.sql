-- =============================================
-- FIX: Arithmetic Overflow in vw_DailyDeliverySummary
-- =============================================

-- Step 1: Drop the existing view
DROP VIEW IF EXISTS vw_DailyDeliverySummary;
GO

-- Step 2: Recreate with proper CAST to prevent overflow
CREATE VIEW vw_DailyDeliverySummary
AS
SELECT 
    dd.DeliveryId,
    dd.DeliveryDate,
    
    -- Cast all NUMERIC/DECIMAL columns to DECIMAL(18,2)
    CAST(ISNULL(dd.CashCollected, 0) AS DECIMAL(18,2)) AS CashCollected,
    
    -- If you have calculated columns, wrap them in CAST
    CAST(ISNULL(SUM(ddi.SomeAmount), 0) AS DECIMAL(18,2)) AS TotalAmount,
    
    -- For division, use NULLIF to prevent divide by zero
    CAST(SUM(ddi.SomeValue) / NULLIF(COUNT(*), 0) AS DECIMAL(18,2)) AS AvgValue,
  
    -- Integer columns don't need CAST
    dd.CompletedInvoices,
    dd.PendingInvoices,
    dd.EmptyCylindersReturned,
    
    -- String columns
    dd.Remarks,
    dd.Status
    
FROM DailyDelivery dd
LEFT JOIN DailyDeliveryItems ddi ON dd.DeliveryId = ddi.DeliveryId
GROUP BY 
    dd.DeliveryId,
    dd.DeliveryDate,
    dd.CashCollected,
    dd.CompletedInvoices,
    dd.PendingInvoices,
    dd.EmptyCylindersReturned,
    dd.Remarks,
    dd.Status;
GO

-- Step 3: Test the view
SELECT * FROM vw_DailyDeliverySummary;
GO
