-- =============================================
-- DIAGNOSTIC: Find problematic data in DailyDeliveryMetrics
-- =============================================

USE [sandhyaflames]
GO

PRINT '========================================';
PRINT 'DIAGNOSTIC REPORT';
PRINT '========================================';
PRINT '';

-- 1. Check CashCollected data type and values
PRINT '1. CashCollected Column Info:';
SELECT 
    c.name AS ColumnName,
    t.name AS DataType,
    c.precision AS Precision,
    c.scale AS Scale,
    CONCAT('DECIMAL(', c.precision, ',', c.scale, ')') AS FullType
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('DailyDeliveryMetrics')
    AND c.name = 'CashCollected';
GO

PRINT '';
PRINT '2. Extreme CashCollected Values:';
SELECT TOP 10
    DeliveryId,
  CashCollected,
 LEN(CAST(CashCollected AS VARCHAR(50))) AS DigitCount
FROM DailyDeliveryMetrics
WHERE CashCollected IS NOT NULL
ORDER BY ABS(CashCollected) DESC;
GO

PRINT '';
PRINT '3. CompletedInvoices vs InvoiceCount:';
SELECT TOP 10
    DeliveryId,
    CompletedInvoices,
    InvoiceCount,
    CASE 
   WHEN InvoiceCount = 0 THEN 'DIVISION BY ZERO'
  WHEN CompletedInvoices > InvoiceCount THEN 'COMPLETED > TOTAL'
        ELSE 'OK'
    END AS Status
FROM DailyDeliveryMetrics
WHERE InvoiceCount IS NOT NULL
ORDER BY DeliveryId DESC;
GO

PRINT '';
PRINT '4. Check for NULL values:';
SELECT 
    COUNT(*) AS TotalRows,
    SUM(CASE WHEN CashCollected IS NULL THEN 1 ELSE 0 END) AS NullCashCollected,
    SUM(CASE WHEN CompletedInvoices IS NULL THEN 1 ELSE 0 END) AS NullCompletedInvoices,
 SUM(CASE WHEN InvoiceCount IS NULL THEN 1 ELSE 0 END) AS NullInvoiceCount
FROM DailyDeliveryMetrics;
GO

PRINT '';
PRINT '5. Test Problematic Calculation Directly:';
-- This will show which row causes the issue
DECLARE @TestDeliveryId INT;
DECLARE @CompletedInvoices INT;
DECLARE @InvoiceCount INT;
DECLARE @Result DECIMAL(5,2);

DECLARE test_cursor CURSOR FOR
SELECT DeliveryId, CompletedInvoices, InvoiceCount
FROM DailyDeliveryMetrics
WHERE InvoiceCount > 0;

OPEN test_cursor;
FETCH NEXT FROM test_cursor INTO @TestDeliveryId, @CompletedInvoices, @InvoiceCount;

WHILE @@FETCH_STATUS = 0
BEGIN
    BEGIN TRY
-- Try the old calculation
 SET @Result = CAST((1.0 * @CompletedInvoices / NULLIF(@InvoiceCount, 0)) * 100 AS DECIMAL(5,2));
  PRINT CONCAT('? DeliveryId ', @TestDeliveryId, ': ', @CompletedInvoices, '/', @InvoiceCount, ' = ', @Result, '%');
    END TRY
    BEGIN CATCH
   PRINT CONCAT('? OVERFLOW at DeliveryId ', @TestDeliveryId, ': ', @CompletedInvoices, '/', @InvoiceCount);
        PRINT CONCAT('   Error: ', ERROR_MESSAGE());
    END CATCH;
    
    FETCH NEXT FROM test_cursor INTO @TestDeliveryId, @CompletedInvoices, @InvoiceCount;
END

CLOSE test_cursor;
DEALLOCATE test_cursor;
GO

PRINT '';
PRINT '========================================';
PRINT 'DIAGNOSTIC COMPLETE';
PRINT '========================================';
