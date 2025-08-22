#!/bin/bash

# MoneyTracker Backend Complete Validation Script
# This script validates the entire Clean Architecture setup with dual database support

echo "==========================================="
echo "MoneyTracker Backend Validation"
echo "==========================================="
echo ""

# Set up environment
export PATH="$PATH:/home/minimax/.dotnet:/home/minimax/.dotnet/tools"

# Color codes for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Success and error counters
SUCCESS=0
ERRORS=0

print_success() {
    echo -e "${GREEN}✓ $1${NC}"
    ((SUCCESS++))
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
    ((ERRORS++))
}

print_info() {
    echo -e "${YELLOW}ℹ $1${NC}"
}

print_info "Starting comprehensive validation..."
echo ""

# 1. Validate .NET SDK Installation
print_info "1. Validating .NET SDK..."
if command -v dotnet &> /dev/null; then
    VERSION=$(dotnet --version)
    if [[ $VERSION == 8.* ]]; then
        print_success ".NET 8 SDK detected: $VERSION"
    else
        print_error ".NET 8 SDK required, found: $VERSION"
    fi
else
    print_error ".NET SDK not found"
fi
echo ""

# 2. Validate Solution Structure
print_info "2. Validating solution structure..."
if [ -f "MoneyTracker.Backend.sln" ]; then
    print_success "Solution file exists"
else
    print_error "Solution file missing"
fi

# Check project files
PROJECTS=("src/MoneyTracker.Domain/MoneyTracker.Domain.csproj" "src/MoneyTracker.Application/MoneyTracker.Application.csproj" "src/MoneyTracker.Infrastructure/MoneyTracker.Infrastructure.csproj" "src/MoneyTracker.API/MoneyTracker.API.csproj")
for project in "${PROJECTS[@]}"; do
    if [ -f "$project" ]; then
        print_success "Project exists: $project"
    else
        print_error "Project missing: $project"
    fi
done
echo ""

# 3. Validate Package Restore
print_info "3. Validating package restore..."
if dotnet restore &> /dev/null; then
    print_success "Package restore successful"
else
    print_error "Package restore failed"
fi
echo ""

# 4. Validate Build
print_info "4. Validating build..."
if dotnet build --configuration Release --no-restore --verbosity quiet &> /dev/null; then
    print_success "Build successful (Release configuration)"
else
    print_error "Build failed"
fi
echo ""

# 5. Validate Database Contexts
print_info "5. Validating database contexts..."
if [ -f "src/MoneyTracker.Infrastructure/Persistence/MySqlDbContext.cs" ]; then
    print_success "MySqlDbContext exists"
else
    print_error "MySqlDbContext missing"
fi

if [ -f "src/MoneyTracker.Infrastructure/Persistence/SqlServerDbContext.cs" ]; then
    print_success "SqlServerDbContext exists"
else
    print_error "SqlServerDbContext missing"
fi
echo ""

# 6. Validate Design-Time Factories
print_info "6. Validating design-time factories..."
if [ -f "src/MoneyTracker.Infrastructure/Persistence/Factories/MySqlDbContextFactory.cs" ]; then
    print_success "MySQL design-time factory exists"
else
    print_error "MySQL design-time factory missing"
fi

if [ -f "src/MoneyTracker.Infrastructure/Persistence/Factories/SqlServerDbContextFactory.cs" ]; then
    print_success "SQL Server design-time factory exists"
else
    print_error "SQL Server design-time factory missing"
fi
echo ""

# 7. Validate Migrations
print_info "7. Validating Entity Framework migrations..."
if [ -d "src/MoneyTracker.Infrastructure/Migrations/MySql" ]; then
    MYSQL_MIGRATIONS=$(ls src/MoneyTracker.Infrastructure/Migrations/MySql/*.cs 2>/dev/null | wc -l)
    if [ $MYSQL_MIGRATIONS -gt 0 ]; then
        print_success "MySQL migrations exist ($MYSQL_MIGRATIONS files)"
    else
        print_error "No MySQL migration files found"
    fi
else
    print_error "MySQL migrations directory missing"
fi

if [ -d "src/MoneyTracker.Infrastructure/Migrations/SqlServer" ]; then
    SQLSERVER_MIGRATIONS=$(ls src/MoneyTracker.Infrastructure/Migrations/SqlServer/*.cs 2>/dev/null | wc -l)
    if [ $SQLSERVER_MIGRATIONS -gt 0 ]; then
        print_success "SQL Server migrations exist ($SQLSERVER_MIGRATIONS files)"
    else
        print_error "No SQL Server migration files found"
    fi
else
    print_error "SQL Server migrations directory missing"
fi
echo ""

# 8. Validate Configuration Files
print_info "8. Validating configuration files..."
CONFIG_FILES=("src/MoneyTracker.API/appsettings.json" "src/MoneyTracker.API/appsettings.Development.json" "src/MoneyTracker.API/appsettings.Production.json")
for config in "${CONFIG_FILES[@]}"; do
    if [ -f "$config" ]; then
        print_success "Configuration exists: $config"
    else
        print_error "Configuration missing: $config"
    fi
done
echo ""

# 9. Validate Docker Support
print_info "9. Validating Docker support..."
if [ -f "docker-compose.mysql.yml" ]; then
    print_success "MySQL Docker Compose exists"
else
    print_error "MySQL Docker Compose missing"
fi

if [ -f "docker-compose.sqlserver.yml" ]; then
    print_success "SQL Server Docker Compose exists"
else
    print_error "SQL Server Docker Compose missing"
fi
echo ""

# 10. Validate Clean Architecture Structure
print_info "10. Validating Clean Architecture compliance..."
ARCH_COMPONENTS=("src/MoneyTracker.Domain/Common/BaseEntity.cs" "src/MoneyTracker.Application/Common/Interfaces/IApplicationDbContext.cs" "src/MoneyTracker.Infrastructure/DependencyInjection.cs" "src/MoneyTracker.API/Program.cs")
for component in "${ARCH_COMPONENTS[@]}"; do
    if [ -f "$component" ]; then
        print_success "Architecture component exists: $(basename $component)"
    else
        print_error "Architecture component missing: $component"
    fi
done
echo ""

# 11. Validate Test Infrastructure
print_info "11. Validating test infrastructure..."
if [ -f "src/MoneyTracker.API/Controllers/TestController.cs" ]; then
    print_success "Test controller exists"
else
    print_error "Test controller missing"
fi

if [ -f "src/MoneyTracker.API/Controllers/HealthController.cs" ]; then
    print_success "Health controller exists"
else
    print_error "Health controller missing"
fi
echo ""

# 12. Final Summary
echo "==========================================="
echo "VALIDATION SUMMARY"
echo "==========================================="
print_success "Successful validations: $SUCCESS"
if [ $ERRORS -gt 0 ]; then
    print_error "Failed validations: $ERRORS"
    echo ""
    echo "Some validations failed. Please review the errors above."
    exit 1
else
    print_success "All validations passed! ✨"
    echo ""
    echo "The MoneyTracker backend is ready for development."
    echo ""
    echo "Next steps:"
    echo "1. Start a database container (MySQL or SQL Server)"
    echo "2. Run database migrations"
    echo "3. Start the API: cd src/MoneyTracker.API && dotnet run"
    echo "4. Test the endpoints at https://localhost:7000"
fi
echo ""