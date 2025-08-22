@echo off
REM MoneyTracker Backend Build and Setup Script
REM This script builds the solution and sets up the development environment

echo === MoneyTracker Backend Setup ===
echo.

REM Check if .NET 8 SDK is installed
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo Error: .NET 8 SDK is not installed.
    echo Please install .NET 8 SDK from: https://dotnet.microsoft.com/download/dotnet/8.0
    exit /b 1
)

REM Display .NET version
echo Detected .NET version:
dotnet --version
echo.

REM Restore NuGet packages
echo Restoring NuGet packages...
dotnet restore

if %errorlevel% neq 0 (
    echo Error: Failed to restore NuGet packages.
    exit /b 1
)

REM Build the solution
echo Building the solution...
dotnet build --configuration Debug --no-restore

if %errorlevel% neq 0 (
    echo Error: Build failed.
    exit /b 1
)

echo.
echo === Build Successful ===
echo.
echo Next steps:
echo 1. Choose your database provider by updating appsettings.json
echo 2. Start database containers:
echo    - For MySQL: docker-compose -f docker-compose.mysql.yml up -d
echo    - For SQL Server: docker-compose -f docker-compose.sqlserver.yml up -d
echo 3. Run the application:
echo    cd src\MoneyTracker.API ^&^& dotnet run
echo.
echo API will be available at:
echo - HTTPS: https://localhost:7000
echo - HTTP: http://localhost:5000
echo - Swagger UI: https://localhost:7000 (development only)
echo.
echo Health checks:
echo - General: GET /health
echo - Database: GET /api/health/database
echo.