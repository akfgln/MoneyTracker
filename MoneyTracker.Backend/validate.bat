@echo off
REM MoneyTracker Backend Complete Validation Script
REM This script validates the entire Clean Architecture setup with dual database support

echo ==========================================
echo MoneyTracker Backend Validation
echo ==========================================
echo.

setlocal enabledelayedexpansion
set SUCCESS=0
set ERRORS=0

echo [INFO] Starting comprehensive validation...
echo.

REM 1. Validate .NET SDK Installation
echo [INFO] 1. Validating .NET SDK...
dotnet --version >nul 2>&1
if %errorlevel% equ 0 (
    for /f "tokens=*" %%i in ('dotnet --version') do set VERSION=%%i
    echo [SUCCESS] .NET SDK detected: !VERSION!
    set /a SUCCESS+=1
) else (
    echo [ERROR] .NET SDK not found
    set /a ERRORS+=1
)
echo.

REM 2. Validate Solution Structure
echo [INFO] 2. Validating solution structure...
if exist "MoneyTracker.Backend.sln" (
    echo [SUCCESS] Solution file exists
    set /a SUCCESS+=1
) else (
    echo [ERROR] Solution file missing
    set /a ERRORS+=1
)

REM Check project files
set PROJECTS=src\MoneyTracker.Domain\MoneyTracker.Domain.csproj src\MoneyTracker.Application\MoneyTracker.Application.csproj src\MoneyTracker.Infrastructure\MoneyTracker.Infrastructure.csproj src\MoneyTracker.API\MoneyTracker.API.csproj
for %%p in (%PROJECTS%) do (
    if exist "%%p" (
        echo [SUCCESS] Project exists: %%p
        set /a SUCCESS+=1
    ) else (
        echo [ERROR] Project missing: %%p
        set /a ERRORS+=1
    )
)
echo.

REM 3. Validate Package Restore
echo [INFO] 3. Validating package restore...
dotnet restore >nul 2>&1
if %errorlevel% equ 0 (
    echo [SUCCESS] Package restore successful
    set /a SUCCESS+=1
) else (
    echo [ERROR] Package restore failed
    set /a ERRORS+=1
)
echo.

REM 4. Validate Build
echo [INFO] 4. Validating build...
dotnet build --configuration Release --no-restore --verbosity quiet >nul 2>&1
if %errorlevel% equ 0 (
    echo [SUCCESS] Build successful ^(Release configuration^)
    set /a SUCCESS+=1
) else (
    echo [ERROR] Build failed
    set /a ERRORS+=1
)
echo.

REM 5. Validate Database Contexts
echo [INFO] 5. Validating database contexts...
if exist "src\MoneyTracker.Infrastructure\Persistence\MySqlDbContext.cs" (
    echo [SUCCESS] MySqlDbContext exists
    set /a SUCCESS+=1
) else (
    echo [ERROR] MySqlDbContext missing
    set /a ERRORS+=1
)

if exist "src\MoneyTracker.Infrastructure\Persistence\SqlServerDbContext.cs" (
    echo [SUCCESS] SqlServerDbContext exists
    set /a SUCCESS+=1
) else (
    echo [ERROR] SqlServerDbContext missing
    set /a ERRORS+=1
)
echo.

REM 6. Validate Migrations
echo [INFO] 6. Validating Entity Framework migrations...
if exist "src\MoneyTracker.Infrastructure\Migrations\MySql" (
    echo [SUCCESS] MySQL migrations directory exists
    set /a SUCCESS+=1
) else (
    echo [ERROR] MySQL migrations directory missing
    set /a ERRORS+=1
)

if exist "src\MoneyTracker.Infrastructure\Migrations\SqlServer" (
    echo [SUCCESS] SQL Server migrations directory exists
    set /a SUCCESS+=1
) else (
    echo [ERROR] SQL Server migrations directory missing
    set /a ERRORS+=1
)
echo.

REM 7. Final Summary
echo ==========================================
echo VALIDATION SUMMARY
echo ==========================================
echo [SUCCESS] Successful validations: !SUCCESS!
if !ERRORS! gtr 0 (
    echo [ERROR] Failed validations: !ERRORS!
    echo.
    echo Some validations failed. Please review the errors above.
    exit /b 1
) else (
    echo [SUCCESS] All validations passed! ✨
    echo.
    echo The MoneyTracker backend is ready for development.
    echo.
    echo Next steps:
    echo 1. Start a database container ^(MySQL or SQL Server^)
    echo 2. Run database migrations
    echo 3. Start the API: cd src\MoneyTracker.API ^&^& dotnet run
    echo 4. Test the endpoints at https://localhost:7000
)
echo.