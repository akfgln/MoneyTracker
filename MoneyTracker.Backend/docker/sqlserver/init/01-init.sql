-- This script will be executed when the SQL Server container starts
-- Add any initial database setup here

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'MoneyTrackerDb_Dev')
BEGIN
    CREATE DATABASE MoneyTrackerDb_Dev;
END
GO

USE MoneyTrackerDb_Dev;
GO

PRINT 'MoneyTracker SQL Server Database initialized successfully!';