-- =============================================
-- ALTERNATIVE FIX: Simple and Safe
-- Uses FLOAT for division to avoid DECIMAL precision issues
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
    
    -- Metrics (integers - no conversion needed)
    ISNULL(m.CompletedInvoices, 0) AS CompletedInvoices,
    ISNULL(m.PendingInvoices, 0) AS PendingInvoices,
    ISNULL(m.EmptyCylindersReturned, 0) AS EmptyCylindersReturned,
 ISNULL(m.OtherItemsDelivered, 0) AS OtherItemsDelivered,
ISNULL(m.CylindersDelivered, 0) AS CylindersDelivered,
    ISNULL(m.NonCylItemsDelivered, 0) AS NonCylItemsDelivered,
    ISNULL(m.InvoiceCount, 0) AS InvoiceCount,
    ISNULL(m.DeliveryCount, 0) AS DeliveryCount,
    ISNULL(m.PlannedInvoices, 0) AS PlannedInvoices,
    
    -- Decimal columns - safe conversion
    ISNULL(CAST(m.CashCollected AS DECIMAL(18, 2)), 0.00) AS CashCollected,
    ISNULL(CAST(m.CashCollected AS DECIMAL(18, 2)), 0.00) AS TotalCollection,
    
    -- Integer addition (no overflow risk)
    (ISNULL(m.CylindersDelivered, 0) + ISNULL(m.NonCylItemsDelivered, 0)) AS TotalItemsDelivered,
    
    -- ? SIMPLEST FIX: Use FLOAT for division, then cast to DECIMAL
    CASE 
      WHEN ISNULL(m.InvoiceCount, 0) = 0 THEN 0.00
        ELSE CAST(
         (CAST(ISNULL(m.CompletedInvoices, 0) AS FLOAT) / CAST(m.InvoiceCount AS FLOAT)) * 100.0 
  AS DECIMAL(5, 2)
 )
    END AS DeliveryCompletionRate

FROM dbo.DailyDelivery dd
LEFT JOIN dbo.DailyDeliveryMetrics m ON m.DeliveryId = dd.DeliveryId
LEFT JOIN dbo.Vehicles v ON v.VehicleId = dd.VehicleId;
GO

-- Test the view
SELECT TOP 5 
    DeliveryId,
  CompletedInvoices,
    InvoiceCount,
  DeliveryCompletionRate
FROM dbo.vw_DailyDeliverySummary
ORDER BY DeliveryDate DESC;
GO

PRINT '? View created successfully!';
