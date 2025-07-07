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

### 🔐 Authentication & Authorization
- `POST /api/auth/login` - User authentication
- `POST /api/auth/refresh` - Token refresh
- `POST /api/auth/revoke` - Token revocation

### 📊 Dashboard & Analytics

#### SuperAdmin Dashboard
- `GET /api/admin/super-dashboard` - System-wide analytics across all companies

#### Company Dashboard
- `GET /api/company/dashboard` - Company-specific analytics and metrics

### 👥 User & Admin Management
- `GET /api/admin/roles` - Get all admin roles (SuperAdmin, Admin)
- `POST /api/admin/role/assign` - Assign roles to users
- `GET /api/admin/list` - Get paginated admin list with search (SuperAdmin only)

### 🏦 Loan Product Management

#### SuperAdmin Endpoints
- `GET /api/admin/loan-products` - Get all loan products across all companies with filtering

#### Admin Endpoints
- `GET /api/company/loan-products` - Get company loan products with filtering
- `POST /api/company/product` - Create new loan product
- `PUT /api/company/loan-product/{id}` - Update loan product

### 💰 Loan Management

#### SuperAdmin Endpoints
- `GET /api/admin/loans` - Get all loans across all companies with comprehensive filtering
- `GET /api/admin/loans/company/{companyId}` - Get loans for specific company
- `GET /api/admin/loans/{loanId}` - Get detailed loan information

#### Admin Endpoints
- `GET /api/company/loans` - Get company loans with filtering (company ID from JWT)
- `GET /api/company/loans/{loanId}` - Get company loan details with access control

#### Loan Filtering & Search
**Available on all loan endpoints:**
- Search: User name, email, loan purpose, account number
- Status: `Pending`, `Approved`, `Disbursed` (ongoing/unpaid), `Overdue` (unpaid), `Repaid`, `Rejected`, `Cancelled`
- Amount range: `minAmount`, `maxAmount`
- Duration range: `minDuration`, `maxDuration`
- Date ranges: `startDate`, `endDate`, `approvedAfter`, `approvedBefore`
- Product filter: `productId`
- Mandate status: `isMandateGenerated`
- Pagination: `page`, `pageSize`
- Sorting: `sortBy`, `sortOrder` (amount, duration, status, username, companyname, createdat, etc.)

**Example Usage:**
```http
# Get ongoing loans across all companies (SuperAdmin)
GET /api/admin/loans?status=Disbursed

# Get overdue loans for admin's company
GET /api/company/loans?status=Overdue

# Search for loans with complex filters
GET /api/admin/loans?search=john&status=Approved&minAmount=50000&maxAmount=200000&sortBy=amount&sortOrder=desc
```

### ⚙️ Administrative Settings
- `GET /api/admin/settings` - Get all administrative settings
- `POST /api/admin/settings` - Create new setting (SuperAdmin only)
- `PUT /api/admin/settings/{id}` - Update setting

### ✅ Approval Management
- `GET /api/admin/approvals/pending` - Get pending approvals
- `POST /api/admin/approvals/{id}/approve` - Approve request
- `POST /api/admin/approvals/{id}/reject` - Reject request

### 🎫 Support System
- `POST /api/support/tickets` - Create support ticket
- `GET /api/support/tickets` - List tickets
- `GET /api/support/dashboard` - Support metrics

### 🏢 Company Management
- `GET /api/company/info` - Get company details
- `PUT /api/company/update` - Update company information

### 💳 Finance Operations
- `GET /api/finance/dashboard` - Financial dashboard
- `GET /api/finance/disbursements` - Disbursement history
- `GET /api/finance/repayments` - Repayment tracking

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
- **Complete loan management system with advanced filtering**
- **SuperAdmin system-wide access across all companies**
- **Admin company-scoped access with JWT-based isolation**
- **Comprehensive loan product management**
- **Advanced search and filtering capabilities**
- **Dashboard analytics for both SuperAdmin and Admin roles**
- **Multi-tenant security enforcement**
- **Pagination and sorting for all listing endpoints**
- **Role-based access control with proper authorization**
- Compilation and build verification

### 🔄 In Progress
- Database migration execution
- Support ticket persistence implementation
- Comprehensive error handling and validation

### 📋 Pending
- Unit and integration tests
- **Audit trail system implementation**
- API documentation (OpenAPI/Swagger)
- Performance optimization
- Security audit and hardening
- Deployment configuration

## 🎯 Platform Capabilities

### Multi-Tenancy Features
- **SuperAdmin**: System-wide access to all companies, loans, products, and users
- **Admin**: Company-scoped access with automatic company ID extraction from JWT
- **Data Isolation**: Complete separation between companies at the service layer
- **Role-Based Security**: Proper authorization checks on all endpoints

### Loan Management Features
- **Comprehensive Filtering**: Status, amount range, duration, dates, search terms
- **Advanced Search**: User details, loan purpose, account numbers
- **Multiple Sorting Options**: Amount, dates, status, user names, company names
- **Status Tracking**: Full loan lifecycle from pending to repaid/overdue
- **Ongoing Loan Monitoring**: Easy identification of active and overdue loans
- **Company-Specific Analytics**: Detailed metrics and statistics

### Analytics & Reporting
- **System-Wide Dashboard**: Complete overview for SuperAdmin
- **Company Dashboard**: Focused analytics for Admin users
- **Financial Metrics**: Total amounts, averages, distributions
- **Performance Tracking**: Growth trends and historical data
- **Risk Analytics**: Overdue rates and status distributions

### Data Management
- **Efficient Pagination**: Handle large datasets with proper pagination
- **Database-Level Filtering**: Performance-optimized queries
- **Real-Time Statistics**: Live counts and summaries with every request
- **Export-Ready Data**: Structured responses suitable for reporting

## Contributing
1. Follow the existing code structure and naming conventions
2. Implement proper error handling and logging
3. Add unit tests for new features
4. Update documentation for API changes

## License
[Add appropriate license information]
