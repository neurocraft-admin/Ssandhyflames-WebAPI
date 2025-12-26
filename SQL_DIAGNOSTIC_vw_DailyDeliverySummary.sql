-- =============================================
-- DIAGNOSTIC SCRIPT: Find Arithmetic Overflow in vw_DailyDeliverySummary
-- =============================================

PRINT '========================================';
PRINT '1. View Definition';
PRINT '========================================';
SELECT OBJECT_DEFINITION(OBJECT_ID('vw_DailyDeliverySummary')) AS ViewDefinition;
GO

PRINT '';
PRINT '========================================';
PRINT '2. Column Data Types in View';
PRINT '========================================';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    NUMERIC_PRECISION,
    NUMERIC_SCALE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'vw_DailyDeliverySummary'
ORDER BY ORDINAL_POSITION;
GO

PRINT '';
PRINT '========================================';
PRINT '3. Test Each Column Individually';
PRINT '========================================';

-- Test selecting each column one at a time to find which one causes overflow
-- Start with non-calculated columns
BEGIN TRY
    SELECT DeliveryId FROM vw_DailyDeliverySummary;
    PRINT '? DeliveryId - OK';
END TRY
BEGIN CATCH
    PRINT '? DeliveryId - ERROR: ' + ERROR_MESSAGE();
END CATCH;

BEGIN TRY
    SELECT DeliveryDate FROM vw_DailyDeliverySummary;
    PRINT '? DeliveryDate - OK';
END TRY
BEGIN CATCH
  PRINT '? DeliveryDate - ERROR: ' + ERROR_MESSAGE();
END CATCH;

-- Test potential problem columns (decimals/numerics)
BEGIN TRY
    SELECT CashCollected FROM vw_DailyDeliverySummary;
    PRINT '? CashCollected - OK';
END TRY
BEGIN CATCH
    PRINT '? CashCollected - ERROR: ' + ERROR_MESSAGE();
END CATCH;

-- Add more columns as needed based on your view structure
-- Repeat this pattern for each column in your view

PRINT '';
PRINT '========================================';
PRINT '4. Check Base Table Data Types';
PRINT '========================================';
SELECT 
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length,
    c.precision,
    c.scale,
    CASE 
        WHEN c.precision > 18 THEN '?? HIGH PRECISION - May cause overflow'
 WHEN c.scale > 4 THEN '?? HIGH SCALE - May cause overflow'
        ELSE '? OK'
 END AS Status
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('DailyDelivery')
    AND t.name IN ('decimal', 'numeric', 'money', 'float', 'real')
ORDER BY c.column_id;
GO

PRINT '';
PRINT '========================================';
PRINT '5. Find Rows with Extreme Values';
PRINT '========================================';

-- Check for very large values that might cause overflow when summed
SELECT TOP 10
    DeliveryId,
    CashCollected,
    CAST(CashCollected AS DECIMAL(38,10)) AS HighPrecisionCash
FROM DailyDelivery
WHERE CashCollected IS NOT NULL
ORDER BY CashCollected DESC;
GO

PRINT '';
PRINT '========================================';
PRINT '6. Recommended Fix Template';
PRINT '========================================';
PRINT 'Based on the results above, modify your view to:';
PRINT '1. CAST all DECIMAL/NUMERIC columns to DECIMAL(18,2)';
PRINT '2. Use ISNULL() to handle NULL values';
PRINT '3. For divisions, use NULLIF to prevent divide by zero';
PRINT '';
PRINT 'Example:';
PRINT 'CAST(ISNULL(SUM(Amount), 0) AS DECIMAL(18,2)) AS TotalAmount';
GO
