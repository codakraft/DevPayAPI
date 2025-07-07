# LendingSolution Platform API Documentation

## Overview
The LendingSolution platform is a comprehensive multi-tenant loan management system with role-based access control supporting SuperAdmin and Admin roles. The platform provides complete loan lifecycle management, analytics, user management, and administrative functions.

---

## 🔐 **Authentication & Authorization**

### Roles
- **SuperAdmin**: System-wide access across all companies
- **Admin**: Company-scoped access (restricted to their own company)

### JWT Token Structure
- Contains user ID, role, and company ID (for Admin users)
- Company ID automatically extracted from JWT for Admin operations

---

## 🏢 **Multi-Tenancy**

The platform enforces strict multi-tenancy:
- **SuperAdmin**: Can access data across all companies
- **Admin**: Restricted to their company's data only
- Company isolation enforced at the service layer

---

## 📊 **Dashboard & Analytics**

### SuperAdmin Dashboard
Get comprehensive system-wide analytics across all companies.

```http
GET /api/admin/super-dashboard
Authorization: Bearer {token}
Role Required: SuperAdmin
```

**Returns:**
- Total companies, users, loans across system
- Financial metrics (total loan amount, disbursed, repaid)
- Loan status distribution
- Company performance metrics
- Growth trends and analytics

### Company Dashboard  
Get analytics for a specific company (Admin's own company).

```http
GET /api/company/dashboard
Authorization: Bearer {token}
Role Required: Admin
```

**Returns:**
- Company-specific user and loan statistics
- Financial analytics for the company
- Gender distribution of borrowers
- Risk analytics and trends
- Time-based graph data

---

## 👥 **User & Admin Management**

### Get All Admin Roles
Retrieve all available admin roles in the system.

```http
GET /api/admin/roles
Authorization: Bearer {token}
Role Required: SuperAdmin, Admin
```

### Assign Role to User
Assign administrative roles to users.

```http
POST /api/admin/role/assign
Authorization: Bearer {token}
Content-Type: application/json

{
  "userId": "string",
  "roleId": "string"
}
```

### Get Admin List (SuperAdmin)
Get paginated list of all admins with filtering and search.

```http
GET /api/admin/list?search={term}&page={num}&pageSize={size}&sortBy={field}&sortOrder={asc|desc}
Authorization: Bearer {token}
Role Required: SuperAdmin
```

**Query Parameters:**
- `search`: Search by name, email, company
- `page`: Page number (default: 1)
- `pageSize`: Items per page (default: 20)
- `sortBy`: Sort field (name, email, company, etc.)
- `sortOrder`: asc or desc

---

## 🏦 **Loan Product Management**

### Get All Loan Products (SuperAdmin)
Retrieve all loan products across all companies with filtering.

```http
GET /api/admin/loan-products?search={term}&companyId={id}&isActive={bool}&minInterestRate={rate}&maxInterestRate={rate}&page={num}&pageSize={size}
Authorization: Bearer {token}
Role Required: SuperAdmin
```

### Get Company Loan Products (Admin)
Retrieve loan products for admin's company only.

```http
GET /api/company/loan-products?search={term}&isActive={bool}&minInterestRate={rate}&maxInterestRate={rate}&page={num}&pageSize={size}
Authorization: Bearer {token}
Role Required: Admin
```

### Create Loan Product
Create a new loan product for a company.

```http
POST /api/company/product
Authorization: Bearer {token}
Role Required: Admin
Content-Type: application/json

{
  "name": "string",
  "description": "string",
  "interestRate": decimal,
  "minAmount": decimal,
  "maxAmount": decimal,
  "minTenor": int,
  "maxTenor": int,
  "companyId": "guid"
}
```

### Update Loan Product
Update existing loan product (admin can only update their company's products).

```http
PUT /api/company/loan-product/{productId}
Authorization: Bearer {token}
Role Required: Admin
Content-Type: application/json

{
  "name": "string",
  "description": "string",
  "interestRate": decimal,
  "minAmount": decimal,
  "maxAmount": decimal,
  "minTenor": int,
  "maxTenor": int
}
```

---

## 💰 **Loan Management**

### Get All Loans (SuperAdmin)
Retrieve all loans across all companies with comprehensive filtering.

```http
GET /api/admin/loans?search={term}&companyId={id}&status={status}&minAmount={amount}&maxAmount={amount}&startDate={date}&endDate={date}&page={num}&pageSize={size}&sortBy={field}&sortOrder={asc|desc}
Authorization: Bearer {token}
Role Required: SuperAdmin
```

### Get Company Loans by ID (SuperAdmin)
Retrieve all loans for a specific company.

```http
GET /api/admin/loans/company/{companyId}?search={term}&status={status}&minAmount={amount}&maxAmount={amount}&page={num}&pageSize={size}
Authorization: Bearer {token}
Role Required: SuperAdmin
```

### Get Company Loans (Admin)
Retrieve loans for admin's company only (company ID from JWT).

```http
GET /api/company/loans?search={term}&status={status}&minAmount={amount}&maxAmount={amount}&startDate={date}&endDate={date}&page={num}&pageSize={size}&sortBy={field}&sortOrder={asc|desc}
Authorization: Bearer {token}
Role Required: Admin
```

### Get Loan by ID (SuperAdmin)
Retrieve detailed information for a specific loan.

```http
GET /api/admin/loans/{loanId}
Authorization: Bearer {token}
Role Required: SuperAdmin
```

### Get Company Loan by ID (Admin)
Retrieve detailed information for a specific loan (company verification applied).

```http
GET /api/company/loans/{loanId}
Authorization: Bearer {token}
Role Required: Admin
```

#### Loan Filter Parameters
- `search`: User name, email, loan purpose, account number
- `companyId`: Filter by company (SuperAdmin only)
- `status`: Loan status (Pending, Approved, Disbursed, Repaid, Overdue, etc.)
- `minAmount` / `maxAmount`: Amount range filter
- `minDuration` / `maxDuration`: Duration range filter
- `startDate` / `endDate`: Creation date range
- `approvedAfter` / `approvedBefore`: Approval date range
- `productId`: Filter by loan product
- `isMandateGenerated`: Filter by mandate status
- `page` / `pageSize`: Pagination
- `sortBy`: Sort field (amount, duration, status, username, companyname, createdat, etc.)
- `sortOrder`: asc or desc

#### Loan Status Values
- `Pending`: Loan application submitted, awaiting approval
- `NotBooked`: Registered but not yet applied
- `Approved`: Loan approved, awaiting disbursement
- `Rejected`: Loan application rejected
- `Disbursed`: Loan disbursed, actively being repaid (ONGOING/UNPAID)
- `Repaid`: Loan fully repaid
- `Overdue`: Loan past due date (UNPAID)
- `Cancelled`: Loan cancelled

---

## ⚙️ **Administrative Settings**

### Get All Settings
Retrieve all administrative settings.

```http
GET /api/admin/settings
Authorization: Bearer {token}
Role Required: SuperAdmin, Admin
```

### Create Setting
Create new administrative setting.

```http
POST /api/admin/settings
Authorization: Bearer {token}
Role Required: SuperAdmin
Content-Type: application/json

{
  "settingKey": "string",
  "settingValue": "string",
  "description": "string"
}
```

### Update Setting
Update existing administrative setting.

```http
PUT /api/admin/settings/{id}
Authorization: Bearer {token}
Role Required: SuperAdmin, Admin
Content-Type: application/json

{
  "settingKey": "string",
  "settingValue": "string",
  "description": "string"
}
```

---

## ✅ **Approval Management**

### Get Pending Approvals
Retrieve all pending approval requests.

```http
GET /api/admin/approvals/pending
Authorization: Bearer {token}
Role Required: SuperAdmin, Admin
```

### Approve Request
Approve a pending request.

```http
POST /api/admin/approvals/{id}/approve
Authorization: Bearer {token}
Role Required: SuperAdmin, Admin
Content-Type: application/json

{
  "reason": "string (optional)"
}
```

### Reject Request
Reject a pending request.

```http
POST /api/admin/approvals/{id}/reject
Authorization: Bearer {token}
Role Required: SuperAdmin, Admin
Content-Type: application/json

{
  "reason": "string (optional)"
}
```

---

## 🏢 **Company Management**

### Get All Companies (SuperAdmin)
Retrieve all companies with advanced filtering and analytics.

```http
GET /api/company/sa/all?search={term}&isActive={bool}&createdFrom={date}&createdTo={date}&minUsers={num}&maxUsers={num}&minLoans={num}&maxLoans={num}&minLoanAmount={amount}&maxLoanAmount={amount}&minDefaultRate={rate}&maxDefaultRate={rate}&page={num}&pageSize={size}&sortBy={field}&sortOrder={asc|desc}
Authorization: Bearer {token}
Role Required: SuperAdmin
```

**Query Parameters:**
- `search`: Search company name, short name, or address
- `isActive`: Filter by active/inactive status
- `createdFrom`/`createdTo`: Date range filters
- `minUsers`/`maxUsers`: User count range
- `minLoans`/`maxLoans`: Loan count range
- `minLoanAmount`/`maxLoanAmount`: Total loan amount range
- `minDefaultRate`/`maxDefaultRate`: Default rate percentage range
- Standard pagination and sorting

**Returns:**
- Paginated list of companies with analytics
- User metrics (total/active users)
- Loan statistics (total/active/pending loans)
- Financial metrics (amounts, default rates)
- Activity tracking

### Get Company by ID (SuperAdmin)
Retrieve detailed information for a specific company.

```http
GET /api/company/sa/{companyId}
Authorization: Bearer {token}
Role Required: SuperAdmin
```

**Returns:**
- Complete company details
- Real-time user and loan analytics
- Financial performance metrics
- Activity history

### Get My Company (Admin)
Retrieve admin's own company information without providing company ID.

```http
GET /api/company/my-company
Authorization: Bearer {token}
Role Required: Admin
```

**Returns:**
- Company details for the authenticated admin
- Company ID automatically extracted from JWT token
- Same detailed analytics as SuperAdmin company view

### Create Company (SuperAdmin)
Create a new company in the system.

```http
POST /api/company/sa/create
Authorization: Bearer {token}
Role Required: SuperAdmin
Content-Type: application/json

{
  "name": "string",
  "shortName": "string",
  "street": "string",
  "city": "string",
  "state": "string",
  "country": "string"
}
```

### Update Company (Admin)
Update admin's own company information.

```http
PUT /api/company/info
Authorization: Bearer {token}
Role Required: Admin
Content-Type: application/json

{
  "name": "string",
  "shortName": "string",
  "street": "string",
  "city": "string",
  "state": "string",
  "country": "string"
}
```

### Activate/Deactivate Company (SuperAdmin)
Control company status.

```http
GET /api/company/sa/activate/{companyId}
GET /api/company/sa/deactivate/{companyId}
Authorization: Bearer {token}
Role Required: SuperAdmin
```

---

## 🎯 **Common Use Cases**

### 1. SuperAdmin Monitoring All Loans
```http
# Get system overview
GET /api/admin/super-dashboard

# Get all ongoing loans across all companies
GET /api/admin/loans?status=Disbursed

# Get overdue loans across all companies
GET /api/admin/loans?status=Overdue

# Search for specific user's loans across all companies
GET /api/admin/loans?search=john.doe@email.com

# Get loans for specific company
GET /api/admin/loans/company/12345678-1234-1234-1234-123456789012
```

### 2. Admin Managing Company Loans
```http
# Get company dashboard
GET /api/company/dashboard

# Get all company loans
GET /api/company/loans

# Get ongoing company loans
GET /api/company/loans?status=Disbursed

# Get overdue company loans
GET /api/company/loans?status=Overdue

# Search company loans
GET /api/company/loans?search=car loan&minAmount=10000

# Get company loan products
GET /api/company/loan-products
```

### 3. Filtering and Searching
```http
# Complex filtering example
GET /api/admin/loans?search=john&status=Disbursed&minAmount=50000&maxAmount=200000&startDate=2024-01-01&endDate=2024-12-31&page=1&pageSize=50&sortBy=amount&sortOrder=desc

# Get loans by date range
GET /api/company/loans?startDate=2024-06-01&endDate=2024-06-30

# Get loans by amount range
GET /api/admin/loans?minAmount=100000&maxAmount=500000

# Get recently approved loans
GET /api/admin/loans?status=Approved&approvedAfter=2024-06-01
```

### 4. User and Product Management
```http
# Get all admins (SuperAdmin only)
GET /api/admin/list?search=admin&page=1&pageSize=20

# Get all loan products (SuperAdmin only)
GET /api/admin/loan-products?isActive=true

# Get company-specific loan products (Admin)
GET /api/company/loan-products?isActive=true
```

---

## 📊 **Response Formats**

### Standard API Response
```json
{
  "success": true,
  "message": "Operation successful",
  "data": { /* Response data */ }
}
```

### Paginated Response
```json
{
  "success": true,
  "message": "Data retrieved successfully",
  "data": {
    "loans": [ /* Array of loan objects */ ],
    "totalCount": 150,
    "page": 1,
    "pageSize": 20,
    "totalPages": 8,
    "hasNextPage": true,
    "hasPreviousPage": false,
    "totalLoanAmount": 2500000.00,
    "averageAmount": 16666.67,
    "statusCounts": {
      "Disbursed": 45,
      "Approved": 12,
      "Pending": 8,
      "Repaid": 85
    }
  }
}
```

### Error Response
```json
{
  "success": false,
  "message": "Error description",
  "data": null
}
```

---

## 🔒 **Security Considerations**

1. **Multi-Tenancy**: Admin users can only access their company's data
2. **Role-Based Access**: Different endpoints require different roles
3. **JWT Validation**: All endpoints require valid JWT tokens
4. **Company Isolation**: Service layer enforces company boundaries
5. **Access Control**: Individual resource access verified against user's company

---

## 🚀 **Performance Features**

1. **Pagination**: All listing endpoints support pagination
2. **Database-Level Filtering**: Filters applied at database level for performance
3. **Optimized Queries**: Proper includes to prevent N+1 query problems
4. **Indexing**: Database indexes on frequently queried fields
5. **IQueryable**: Efficient query building for complex filters

---

## 📈 **Analytics Capabilities**

1. **System-Wide Analytics**: Total loans, amounts, trends (SuperAdmin)
2. **Company Analytics**: Company-specific metrics (Admin)
3. **Financial Metrics**: Total amounts, averages, distributions
4. **Status Analytics**: Loan status distributions and counts
5. **Time-Based Data**: Growth trends and historical data
6. **Risk Analytics**: Overdue rates and risk metrics

---

## 🛠️ **Integration Examples**

### Frontend Dashboard Integration
```javascript
// Get SuperAdmin dashboard
const response = await fetch('/api/admin/super-dashboard', {
  headers: { 'Authorization': `Bearer ${token}` }
});
const dashboard = await response.json();

// Get filtered loans
const loans = await fetch('/api/admin/loans?status=Disbursed&page=1&pageSize=20', {
  headers: { 'Authorization': `Bearer ${token}` }
});
```

### Admin Company Management
```javascript
// Get company loans with search
const companyLoans = await fetch('/api/company/loans?search=car&status=Approved', {
  headers: { 'Authorization': `Bearer ${token}` }
});

// Get company dashboard
const companyDashboard = await fetch('/api/company/dashboard', {
  headers: { 'Authorization': `Bearer ${token}` }
});
```

---

This documentation covers all major platform capabilities. The system provides comprehensive loan management with robust filtering, searching, multi-tenancy, and role-based access control suitable for enterprise-level loan management operations.
