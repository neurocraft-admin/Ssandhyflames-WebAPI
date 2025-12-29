-- =============================================
-- FINAL FIX: Handle CompletedInvoices > InvoiceCount
-- This is the real issue causing overflow!
-- =============================================

USE [sandhyaflames]
GO

-- Drop the existing view
DROP VIEW IF EXISTS dbo.vw_DailyDeliverySummary;
GO

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
    
    -- Metrics
ISNULL(m.CompletedInvoices, 0) AS CompletedInvoices,
    ISNULL(m.PendingInvoices, 0) AS PendingInvoices,
 ISNULL(m.CashCollected, 0.00) AS CashCollected,
    ISNULL(m.EmptyCylindersReturned, 0) AS EmptyCylindersReturned,
    ISNULL(m.OtherItemsDelivered, 0) AS OtherItemsDelivered,
    ISNULL(m.CylindersDelivered, 0) AS CylindersDelivered,
    ISNULL(m.NonCylItemsDelivered, 0) AS NonCylItemsDelivered,
    ISNULL(m.InvoiceCount, 0) AS InvoiceCount,
    ISNULL(m.DeliveryCount, 0) AS DeliveryCount,
    ISNULL(m.PlannedInvoices, 0) AS PlannedInvoices,
    
    -- Derived fields
    ISNULL(m.CashCollected, 0.00) AS TotalCollection,
    (ISNULL(m.CylindersDelivered, 0) + ISNULL(m.NonCylItemsDelivered, 0)) AS TotalItemsDelivered,
    
    -- ? FINAL FIX: Cap percentage at 100% to prevent overflow
    CASE 
        WHEN ISNULL(m.InvoiceCount, 0) = 0 THEN 0.00
        -- If completed > total, cap at 100%
   WHEN m.CompletedInvoices >= m.InvoiceCount THEN 100.00
   -- Otherwise calculate normally
        ELSE 
   CAST(
      (CAST(m.CompletedInvoices AS FLOAT) / CAST(m.InvoiceCount AS FLOAT)) * 100.0 
 AS DECIMAL(5, 2)
    )
    END AS DeliveryCompletionRate

FROM dbo.DailyDelivery dd
LEFT JOIN dbo.DailyDeliveryMetrics m ON m.DeliveryId = dd.DeliveryId
LEFT JOIN dbo.Vehicles v ON v.VehicleId = dd.VehicleId;
GO

-- Test the view
PRINT 'Testing view...';
SELECT TOP 10 
    DeliveryId,
    CompletedInvoices,
    InvoiceCount,
    DeliveryCompletionRate,
    CASE 
        WHEN CompletedInvoices > InvoiceCount THEN 'CAPPED AT 100%'
        ELSE 'OK'
    END AS Status
FROM dbo.vw_DailyDeliverySummary
ORDER BY DeliveryId DESC;
GO

PRINT '';
PRINT '? View recreated successfully!';
PRINT '? DeliveryId 24 (10/1) will now show 100% instead of overflow';
