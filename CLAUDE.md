# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MoneyTracker is a German-localized financial management application with a .NET 8 backend and Angular 17+ frontend. The system supports dual database providers (MySQL/SQL Server) and includes comprehensive German localization for business use.

## Architecture

### Backend (.NET 8 Clean Architecture)
- **Domain Layer**: Core entities, value objects, enums (`MoneyTracker.Domain`)
- **Application Layer**: Business logic, DTOs, services, validators (`MoneyTracker.Application`) 
- **Infrastructure Layer**: Data access, repositories, external services (`MoneyTracker.Infrastructure`)
- **API Layer**: Controllers, middleware, authentication (`MoneyTracker.API`)

### Frontend (Angular 17+)
- **Core Module**: Services, guards, interceptors, models
- **Features**: Auth, dashboard, transactions, categories, reports, settings
- **Shared**: Reusable components, pipes, validators
- German localization with custom pipes and validators

## Common Development Commands

### Backend Commands
Navigate to `MoneyTracker.Backend/` directory for all backend operations.

**Build and validation:**
```bash
# Linux/macOS
bash build.sh          # Build solution
bash validate.sh       # Comprehensive validation (25+ checks)

# Windows  
build.bat              # Build solution
validate.bat           # Comprehensive validation
```

**Development:**
```bash
dotnet restore                           # Restore packages
dotnet build                            # Build solution
dotnet run --project src/MoneyTracker.API  # Run API (https://localhost:7000)
```

**Database operations:**
```bash
# Start database containers
docker-compose -f docker-compose.mysql.yml up -d      # MySQL
docker-compose -f docker-compose.sqlserver.yml up -d  # SQL Server

# Entity Framework migrations (from MoneyTracker.Infrastructure)
dotnet ef migrations add MigrationName --context MySqlDbContext --output-dir Migrations/MySql
dotnet ef migrations add MigrationName --context SqlServerDbContext --output-dir Migrations/SqlServer
```

**Testing:**
```bash
dotnet test                             # Run all tests
dotnet test --logger "console;verbosity=detailed"  # Verbose test output
```

### Frontend Commands
Navigate to `MoneyTracker.Frontend/` directory for all frontend operations.

**Development:**
```bash
npm install            # Install dependencies
npm start             # Start dev server (http://localhost:4200)
ng serve              # Alternative start command
```

**Build and test:**
```bash
npm run build         # Production build
ng build             # Development build  
npm test             # Run unit tests
ng test              # Alternative test command
```

**Linting and formatting:**
```bash
ng lint              # Run linter (if configured)
```

## Key Configuration Files

### Backend Configuration
- `MoneyTracker.Backend/src/MoneyTracker.API/appsettings.json` - Main configuration
- Database provider selection via `DatabaseSettings.Provider` ("MySql" or "SqlServer")
- JWT settings, file storage, logging configuration included
- Connection strings for both MySQL and SQL Server

### Frontend Configuration
- `MoneyTracker.Frontend/src/environments/environment.ts` - Development config
- `MoneyTracker.Frontend/src/environments/environment.prod.ts` - Production config
- API URL: `https://localhost:7001/api` (development)
- German localization settings (de-DE locale, EUR currency)

## Database Support

The backend supports dual database providers with separate Entity Framework contexts:
- **MySqlDbContext** with Pomelo.EntityFrameworkCore.MySql
- **SqlServerDbContext** with Microsoft.EntityFrameworkCore.SqlServer

Switch providers by updating `DatabaseSettings.Provider` in appsettings.json.

## German Localization Features

### Frontend Localization
- Complete German translations in `src/assets/i18n/de.json`
- Custom German formatting pipes: `GermanCurrencyPipe`, `GermanDatePipe`, `GermanNumberPipe`
- German-specific validators: IBAN, phone numbers, postal codes, tax IDs
- Date format: DD.MM.YYYY, Currency: EUR with comma decimal separator

### Business Logic
- German tax rates (19% and 7% VAT) implemented
- German business categories and expense types
- IBAN validation and German banking standards

## API Endpoints

### Health and Testing
- `GET /health` - Application health check
- `GET /api/health/database` - Database-specific health
- `GET /api/test/database-setup` - Database configuration validation
- `GET /api/test/migration-status` - Migration status check

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration
- `POST /api/auth/refresh` - Token refresh

## Development Workflow

1. **Backend Setup**: Run `validate.sh/validate.bat` to verify complete setup
2. **Database**: Start appropriate database container via docker-compose
3. **Backend**: Run API from `src/MoneyTracker.API` with `dotnet run`
4. **Frontend**: Start Angular dev server with `npm start`
5. **Testing**: Use health endpoints to verify backend connectivity

## File Structure Patterns

### Backend
- Controllers in `src/MoneyTracker.API/Controllers/`
- Entities in `src/MoneyTracker.Domain/Entities/`
- Services in `src/MoneyTracker.Application/` and `src/MoneyTracker.Infrastructure/Services/`
- Data configurations in `src/MoneyTracker.Infrastructure/Persistence/Configurations/`

### Frontend  
- Feature modules in `src/app/features/{feature}/`
- Shared components in `src/app/shared/components/`
- Core services in `src/app/core/services/`
- German translations in `src/assets/i18n/de.json`

## Testing Strategy

### Backend
- Integration tests in `tests/MoneyTracker.IntegrationTests/`
- Unit tests in `tests/MoneyTracker.UnitTests/`
- Health endpoints provide runtime validation

### Frontend
- Jasmine/Karma setup for unit testing
- Component and service test templates included
- German localization requires testing with de-DE locale

## Production Considerations

- Database provider is runtime-configurable
- Separate connection strings for MySQL and SQL Server
- Serilog configured for file and console logging
- JWT settings include production-ready security options
- File upload restrictions and virus scanning enabled
- CORS and security middleware configured