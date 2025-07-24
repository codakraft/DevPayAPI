# Company Users Management API

This API provides endpoints for managing and retrieving users within a company context. Company administrators can get a comprehensive list of users associated with their company.

## Endpoint

**GET** `/api/company/{companyId}/users`

## Query Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| Search | string | No | Search in firstname, lastname, email, or phone number |
| IsActive | boolean | No | Filter by user active status |
| Gender | string | No | Filter by gender |
| CreatedFrom | datetime | No | Filter users created from this date |
| CreatedTo | datetime | No | Filter users created up to this date |
| LastLoginFrom | datetime | No | Filter users who logged in from this date |
| LastLoginTo | datetime | No | Filter users who logged in up to this date |
| Role | string | No | Filter by user role (e.g., "Admin", "SuperAdmin", "User") |
| Page | integer | No | Page number (default: 1) |
| PageSize | integer | No | Number of items per page (default: 20, max: 100) |
| SortBy | string | No | Sort field: "FirstName", "LastName", "Email", "IsActive", "Role", "CreatedAt", "LastLoginAt" |
| SortOrder | string | No | Sort direction: "asc" or "desc" (default: "desc") |

## Response

```json
{
  "status": "Success",
  "message": "Company users retrieved successfully",
  "data": {
    "users": [
      {
        "id": "0bd1bedc-cebb-44b9-adb4-ab15bfc08d7e",
        "firstName": "Dev Lending",
        "lastName": "Admin",
        "fullName": "Dev Lending Admin",
        "email": "dev-lending-admin@yopmail.com",
        "phoneNumber": null,
        "address": null,
        "city": null,
        "state": null,
        "gender": null,
        "dateOfBirth": null,
        "isActive": true,
        "createdAt": "2025-07-15T09:08:17.3732474",
        "lastLoginAt": null,
        "role": "Admin"
      }
    ],
    "totalCount": 25,
    "page": 1,
    "pageSize": 20,
    "totalPages": 2,
    "hasNextPage": true,
    "hasPreviousPage": false,
    "totalActiveUsers": 20,
    "totalInactiveUsers": 5
  }
}
```

## Response Fields

### User Object
- **id**: Unique identifier for the user
- **firstName**: User's first name
- **lastName**: User's last name
- **fullName**: Computed full name (firstName + lastName)
- **email**: User's email address
- **phoneNumber**: User's phone number (nullable)
- **address**: User's address (nullable)
- **city**: User's city (nullable)
- **state**: User's state (nullable)
- **gender**: User's gender (nullable)
- **dateOfBirth**: User's date of birth (nullable)
- **isActive**: Whether the user account is active
- **createdAt**: When the user account was created
- **lastLoginAt**: When the user last logged in (nullable)
- **role**: Primary role assigned to the user (e.g., "Admin")

### Pagination & Summary
- **totalCount**: Total number of users matching the filter
- **page**: Current page number
- **pageSize**: Number of items per page
- **totalPages**: Total number of pages
- **hasNextPage**: Whether there are more pages
- **hasPreviousPage**: Whether there are previous pages
- **totalActiveUsers**: Count of active users in the filtered results
- **totalInactiveUsers**: Count of inactive users in the filtered results

## Usage Examples

### Basic Request
```bash
curl -X GET "https://your-api-domain.com/api/company/123e4567-e89b-12d3-a456-426614174000/users"
```

### Filtered Request
```bash
curl -X GET "https://your-api-domain.com/api/company/123e4567-e89b-12d3-a456-426614174000/users?Search=admin&IsActive=true&Role=Admin&Page=1&PageSize=10&SortBy=CreatedAt&SortOrder=desc"
```

### Advanced Filtering
```bash
curl -X GET "https://your-api-domain.com/api/company/123e4567-e89b-12d3-a456-426614174000/users?Gender=Male&CreatedFrom=2024-01-01&CreatedTo=2024-12-31&Role=SuperAdmin&SortBy=Role&SortOrder=asc"
```

## Error Responses

### 404 - Company Not Found
```json
{
  "status": "Failed",
  "message": "Company not found"
}
```

### 401 - Unauthorized
```json
{
  "status": "Failed",
  "message": "Unauthorized access"
}
```

### 500 - Server Error
```json
{
  "status": "Failed",
  "message": "Something went wrong"
}
```

## Use Cases

1. **User Management**: Company admins can view and manage all users in their organization
2. **User Search**: Find specific users by name, email, or phone number
3. **Activity Monitoring**: Track user login activity and account status
4. **Role-Based Management**: Filter and sort users by their assigned roles
5. **Reporting**: Generate reports on user demographics, activity, and role distribution
6. **Compliance**: Audit user accounts, access patterns, and role assignments

## Important Notes

- **Clean Separation**: This endpoint only returns user information. Loan-related data is handled separately through the BorrowerApplication system
- **No Loan Statistics**: Unlike previous versions, this endpoint does not include loan counts, amounts, or loan-related filters since users and borrowers are separate entities
- **Pagination**: Large datasets are paginated for performance
- **Flexible Filtering**: Multiple filters can be combined for precise results
- **Security**: Company-scoped access ensures users can only see users from their own company

## Security & Authorization

- Requires proper authentication and authorization
- Company admins can only access users from their own company
- User data is filtered based on the requesting user's company association
- Sensitive information is appropriately handled based on user roles

## Changes Made

### ✅ **Added User Roles Support**
- ✅ Added `role` field to user response objects showing the primary role assigned to each user
- ✅ Added role-based filtering capability (`Role` parameter)
- ✅ Added role-based sorting option (`SortBy=Role`)
- ✅ Enhanced user management with role visibility and filtering
- ✅ Maintained performance by efficiently fetching roles using UserManager

### ✅ **Enhanced Filtering & Sorting**
- ✅ Role filtering: Filter users by specific roles (Admin, SuperAdmin, User, etc.)
- ✅ Role sorting: Sort users alphabetically by their role assignments
- ✅ Combined filtering: Mix role filters with existing filters (search, gender, dates, etc.)

### ✅ **Response Structure Updates**
- ✅ Each user object now includes a `role` field containing the primary assigned role
- ✅ Roles are fetched using ASP.NET Core Identity's UserManager for accuracy
- ✅ Role information is displayed as an array of strings for easy parsing

### ✅ **Architectural Benefits**
- ✅ **Proper Role Integration**: Uses ASP.NET Core Identity role system
- ✅ **Efficient Queries**: Roles are fetched asynchronously for each user batch
- ✅ **Flexible Filtering**: Role-based access control support
- ✅ **Maintainable Code**: Clean separation between user data and role assignments
- Removed `totalLoans`, `activeLoans`, `pendingLoans`, `totalLoanAmount`, `outstandingAmount` from user objects
- Removed loan-related filter parameters (`MinLoans`, `MaxLoans`, `MinLoanAmount`, `MaxLoanAmount`)
- Removed loan-related sorting options (`TotalLoans`, `TotalLoanAmount`)
- Removed loan statistics from summary (`totalUsersWithLoans`, `totalLoanAmountAcrossUsers`, `averageLoanAmountPerUser`)

### ✅ Simplified Response Structure
- Focused on core user information only
- Maintained essential filtering and pagination features
- Kept user activity tracking (login dates, account status)

### ✅ Architectural Correctness
- Company users are now properly separated from borrower/loan concerns
- BorrowerApplication entity handles all loan-related tracking
- Clean separation of concerns between user management and loan management
