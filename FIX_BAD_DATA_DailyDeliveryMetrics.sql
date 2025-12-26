-- =============================================
-- FIX BAD DATA: CompletedInvoices > InvoiceCount
-- This should not happen in real data
-- =============================================

USE [sandhyaflames]
GO

PRINT '========================================';
PRINT 'FIXING BAD DATA IN DailyDeliveryMetrics';
PRINT '========================================';
PRINT '';

-- Show current bad data
PRINT '1. Current Bad Data:';
SELECT 
    DeliveryId,
    CompletedInvoices,
    InvoiceCount,
    CAST((1.0 * CompletedInvoices / InvoiceCount) * 100 AS VARCHAR(20)) + '%' AS CurrentPercentage
FROM DailyDeliveryMetrics
WHERE CompletedInvoices > InvoiceCount
    AND InvoiceCount > 0;
GO

-- Fix Option 1: Set CompletedInvoices = InvoiceCount (Cap at 100%)
PRINT '';
PRINT '2. Applying Fix: Set CompletedInvoices = InvoiceCount where Completed > Total';

UPDATE DailyDeliveryMetrics
SET CompletedInvoices = InvoiceCount
WHERE CompletedInvoices > InvoiceCount
    AND InvoiceCount > 0;

PRINT CONCAT('   Rows updated: ', @@ROWCOUNT);
GO

-- Verify fix
PRINT '';
PRINT '3. Verification - Should be no rows:';
SELECT 
    DeliveryId,
    CompletedInvoices,
    InvoiceCount
FROM DailyDeliveryMetrics
WHERE CompletedInvoices > InvoiceCount
AND InvoiceCount > 0;
GO

PRINT '';
PRINT '========================================';
PRINT '? DATA FIX COMPLETE';
PRINT '========================================';
PRINT '';
PRINT 'Next step: Recreate the view using FIX_FINAL_vw_DailyDeliverySummary_CAPPED.sql';
