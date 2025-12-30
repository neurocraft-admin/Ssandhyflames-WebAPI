-- =============================================
-- FIX: Arithmetic Overflow in vw_DailyDeliverySummary
-- Issue: DeliveryCompletionRate calculation causes overflow
-- ROBUST FIX: Handles extreme values and NULL cases
-- =============================================

USE [sandhyaflames]
GO

-- Drop the existing view
DROP VIEW IF EXISTS dbo.vw_DailyDeliverySummary;
GO

-- Recreate with ultra-safe calculations
CREATE VIEW dbo.vw_DailyDeliverySummary
AS
SELECT 
    dd.DeliveryId,
    dd.DeliveryDate,
    dd.VehicleId,
    v.VehicleNumber,
    dd.Status,
    dd.ReturnTime,
    dd.Remarks,
    
    -- Metrics - All safe integer conversions
    ISNULL(m.CompletedInvoices, 0) AS CompletedInvoices,
    ISNULL(m.PendingInvoices, 0) AS PendingInvoices,
    
    -- ? ULTRA-SAFE: Cap CashCollected to prevent overflow
    CASE 
        WHEN m.CashCollected IS NULL THEN 0.00
        WHEN m.CashCollected > 999999999999.99 THEN 999999999999.99  -- Max safe value
        WHEN m.CashCollected < -999999999999.99 THEN -999999999999.99
        ELSE CAST(m.CashCollected AS DECIMAL(18, 2))
    END AS CashCollected,
    
    ISNULL(m.EmptyCylindersReturned, 0) AS EmptyCylindersReturned,
    ISNULL(m.OtherItemsDelivered, 0) AS OtherItemsDelivered,
    ISNULL(m.CylindersDelivered, 0) AS CylindersDelivered,
    ISNULL(m.NonCylItemsDelivered, 0) AS NonCylItemsDelivered,
    ISNULL(m.InvoiceCount, 0) AS InvoiceCount,
    ISNULL(m.DeliveryCount, 0) AS DeliveryCount,
    ISNULL(m.PlannedInvoices, 0) AS PlannedInvoices,
  
    -- Derived fields
    -- ? ULTRA-SAFE: TotalCollection with bounds checking
    CASE 
        WHEN m.CashCollected IS NULL THEN 0.00
        WHEN m.CashCollected > 999999999999.99 THEN 999999999999.99
        WHEN m.CashCollected < -999999999999.99 THEN -999999999999.99
        ELSE CAST(m.CashCollected AS DECIMAL(18, 2))
    END AS TotalCollection,
    
    (ISNULL(m.CylindersDelivered, 0) + ISNULL(m.NonCylItemsDelivered, 0)) AS TotalItemsDelivered,
    
    -- ? ULTRA-SAFE: DeliveryCompletionRate with multiple safeguards
    CASE 
        WHEN m.InvoiceCount IS NULL OR m.InvoiceCount = 0 THEN 0.00
        WHEN m.CompletedInvoices IS NULL THEN 0.00
        -- Prevent extreme percentages
        WHEN m.CompletedInvoices > m.InvoiceCount THEN 100.00
        ELSE 
    -- Safe calculation: Direct integer division then multiply
    CAST(
  ROUND((CAST(m.CompletedInvoices AS FLOAT) / CAST(m.InvoiceCount AS FLOAT)) * 100, 2)
    AS DECIMAL(5, 2)
      )
    END AS DeliveryCompletionRate

FROM dbo.DailyDelivery dd
LEFT JOIN dbo.DailyDeliveryMetrics m ON m.DeliveryId = dd.DeliveryId
LEFT JOIN dbo.Vehicles v ON v.VehicleId = dd.VehicleId;
GO

-- Verify the view works
PRINT 'Testing view...';
GO

SELECT TOP 10 * FROM dbo.vw_DailyDeliverySummary
ORDER BY DeliveryDate DESC;
GO

PRINT '? View recreated and tested successfully!';
GO
