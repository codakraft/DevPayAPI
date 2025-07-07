# Lending Solution - Multi-Tenant Loan Management System

A comprehensive .NET 8 Web API for managing loan operations in a multi-tenant environment.

## Features

### Multi-Tenancy
- **Company-based tenant isolation**: Each company operates as a separate tenant
- **Unique user emails**: Users can only belong to one company with unique email addresses
- **Data segregation**: All loan and financial data is scoped to specific companies

### Core Modules

#### 1. Authentication & Authorization
- JWT-based authentication with refresh tokens
- Multi-step login process (company selection + user credentials)
- Role-based access control (Super Admin, Company Admin, Company User)
- Session management with secure token refresh

#### 2. Company Management
- Company registration and profile management
- Company-specific settings and configurations
- User management within company scope

#### 3. Loan Management
- Loan product configuration and management
- Loan application processing
- Loan eligibility assessment
- Disbursement and repayment tracking
- Multi-status loan workflow

#### 4. Finance Operations
- Financial reporting and analytics
- Disbursement management
- Repayment processing
- Account balance tracking
- Transaction history

#### 5. Support System
- Support ticket management
- User account support
- Loan support operations
- Support dashboard with metrics
- Internal/external ticket categorization

#### 6. Admin Features
- System-wide administrative settings
- Approval workflow management
- User role and permission management
- Company oversight and management

## Technical Architecture

### Backend Structure
```
LendingSolution.API/           # Web API Controllers & Configuration
├── Controllers/              # API Controllers organized by responsibility
├── Extensions/              # Service registration and configuration
└── Models/                  # API-specific DTOs and models

LendingSolution.Core/         # Domain Models & DTOs
├── Models/                  # Entity models
├── Dtos/                   # Data Transfer Objects
├── Enum/                   # Application enumerations
└── Settings/               # Configuration settings

LendingSolution.Application/  # Business Logic & Services
├── Services/               # Business logic implementations
├── Repositories/           # Data access abstractions
├── Helpers/               # Utility functions
└── Exceptions/            # Custom exception types

LendingSolution.Infrastructure/ # Data Access & External Services
├── Data/                   # Entity Framework context
├── Migrations/            # Database migrations
└── Repositories/          # Repository implementations
```

### Key Controllers
- **AuthController**: Authentication and authorization
- **AdminController**: System administration
- **CompanyController**: Company management
- **LoanController**: Loan operations
- **FinanceController**: Financial operations
- **SupportController**: Support and ticketing
- **UserController**: User management
- **BorrowerController**: Borrower-specific operations

### Database Models
- **Company**: Tenant isolation entity
- **ApplicationUser**: Multi-tenant user with company association
- **Loan**: Loan entity with company and user associations
- **LoanProduct**: Company-specific loan products
- **Account**: Financial account management
- **Disbursement/Repayment**: Financial transaction tracking
- **AdminSettings**: System configuration
- **Approval**: Workflow management
- **RefreshToken**: Session management

## API Endpoints

### Authentication
- `POST /api/auth/login` - User authentication
- `POST /api/auth/refresh` - Token refresh
- `POST /api/auth/revoke` - Token revocation

### Company Management
- `GET /api/company/info` - Get company details
- `PUT /api/company/update` - Update company information

### Loan Operations
- `GET /api/loan/products` - Get loan products
- `POST /api/loan/apply` - Submit loan application
- `GET /api/loan/status/{id}` - Check loan status

### Finance
- `GET /api/finance/dashboard` - Financial dashboard
- `GET /api/finance/disbursements` - Disbursement history
- `GET /api/finance/repayments` - Repayment tracking

### Support
- `POST /api/support/tickets` - Create support ticket
- `GET /api/support/tickets` - List tickets
- `GET /api/support/dashboard` - Support metrics

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB or full instance)
- Entity Framework Core CLI tools

### Setup
1. Clone the repository
2. Update connection strings in `appsettings.json`
3. Run database migrations:
   ```bash
   dotnet ef database update --project LendingSolution.Infrastructure --startup-project LendingSolution.API
   ```
4. Build and run:
   ```bash
   dotnet build
   dotnet run --project LendingSolution.API
   ```

### Configuration
- JWT settings in `appsettings.json`
- Database connection strings
- Remita integration settings (if applicable)

## Development Status

### ✅ Completed
- Multi-tenant architecture implementation
- Core entity models and relationships
- Repository pattern with dependency injection
- Service layer with business logic
- Authentication and authorization system
- All major API controllers and endpoints
- Data access layer with Entity Framework
- JWT token management with refresh tokens
- Compilation and build verification

### 🔄 In Progress
- Database migration execution
- Support ticket persistence implementation
- Comprehensive error handling and validation

### 📋 Pending
- Unit and integration tests
- API documentation (OpenAPI/Swagger)
- Performance optimization
- Security audit and hardening
- Deployment configuration

## Contributing
1. Follow the existing code structure and naming conventions
2. Implement proper error handling and logging
3. Add unit tests for new features
4. Update documentation for API changes

## License
[Add appropriate license information]
