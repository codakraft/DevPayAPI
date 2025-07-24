# Company Users API Endpoints

This document describes the new API endpoints for retrieving users that belong to a company.

## Endpoints

### 1. Get Company Users (Admin)
**GET** `/api/company/users`

**Description:** Allows an admin to get a paginated list of all users that belong to their company, with detailed filtering and statistics.

**Authorization:** Admin role required

**Query Parameters:**
- `Search` (string, optional): Search term to filter users by name, email, or phone number
- `IsActive` (boolean, optional): Filter by user active status
- `Gender` (string, optional): Filter by user gender
- `CreatedFrom` (DateTime, optional): Filter users created after this date
- `CreatedTo` (DateTime, optional): Filter users created before this date
- `LastLoginFrom` (DateTime, optional): Filter users with last login after this date
- `LastLoginTo` (DateTime, optional): Filter users with last login before this date
- `MinLoans` (int, optional): Filter users with at least this many loans
- `MaxLoans` (int, optional): Filter users with at most this many loans
- `MinLoanAmount` (decimal, optional): Filter users with total loan amount >= this value
- `MaxLoanAmount` (decimal, optional): Filter users with total loan amount <= this value
- `Page` (int, default: 1): Page number for pagination
- `PageSize` (int, default: 20): Number of items per page
- `SortBy` (string, default: "CreatedAt"): Sort field (firstname, lastname, email, isactive, totalloans, totalloanamount, lastloginat, createdat)
- `SortOrder` (string, default: "desc"): Sort order (asc, desc)

**Response:**
```json
{
  "success": true,
  "message": "Company users fetched successfully",
  "data": {
    "users": [
      {
        "id": "string",
        "firstName": "string",
        "lastName": "string",
        "fullName": "string",
        "email": "string",
        "phoneNumber": "string",
        "address": "string",
        "city": "string",
        "state": "string",
        "gender": "string",
        "dateOfBirth": "2024-01-01T00:00:00Z",
        "isActive": true,
        "createdAt": "2024-01-01T00:00:00Z",
        "lastLoginAt": "2024-01-01T00:00:00Z",
        "totalLoans": 0,
        "activeLoans": 0,
        "pendingLoans": 0,
        "totalLoanAmount": 0.0,
        "outstandingAmount": 0.0
      }
    ],
    "totalCount": 0,
    "page": 1,
    "pageSize": 20,
    "totalPages": 0,
    "hasNextPage": false,
    "hasPreviousPage": false,
    "totalActiveUsers": 0,
    "totalInactiveUsers": 0,
    "totalUsersWithLoans": 0,
    "totalLoanAmountAcrossUsers": 0.0,
    "averageLoanAmountPerUser": 0.0
  }
}
```

### 2. Get Company Users by Company ID (SuperAdmin)
**GET** `/api/company/sa/{companyId}/users`

**Description:** Allows a SuperAdmin to get a paginated list of all users that belong to a specific company.

**Authorization:** SuperAdmin role required

**Path Parameters:**
- `companyId` (Guid): The ID of the company to get users for

**Query Parameters:** Same as the Admin endpoint above

**Response:** Same as the Admin endpoint above

## Usage Examples

### Admin getting their company users
```bash
GET /api/company/users?page=1&pageSize=10&sortBy=createdAt&sortOrder=desc
Authorization: Bearer <admin-jwt-token>
```

### Admin searching for users
```bash
GET /api/company/users?search=john&isActive=true&minLoans=1
Authorization: Bearer <admin-jwt-token>
```

### SuperAdmin getting users for a specific company
```bash
GET /api/company/sa/123e4567-e89b-12d3-a456-426614174000/users?page=1&pageSize=20
Authorization: Bearer <superadmin-jwt-token>
```

## Error Responses

**400 Bad Request:**
```json
{
  "success": false,
  "message": "Company ID not found in token"
}
```

**401 Unauthorized:**
```json
{
  "success": false,
  "message": "Unauthorized"
}
```

**403 Forbidden:**
```json
{
  "success": false,
  "message": "Insufficient permissions"
}
```

**404 Not Found:**
```json
{
  "success": false,
  "message": "Company not found"
}
```

**500 Internal Server Error:**
```json
{
  "success": false,
  "message": "An unexpected error occurred"
}
```

## Key Features

1. **Advanced Filtering:** Search and filter users by multiple criteria including personal info, activity status, and loan statistics
2. **Pagination:** Efficient pagination for large user lists
3. **Sorting:** Sort by various fields in ascending or descending order
4. **Statistics:** Summary statistics including active/inactive users, loan counts, and amounts
5. **Authorization:** Role-based access control with Admin and SuperAdmin permissions
6. **Company Isolation:** Admins can only see users from their own company
7. **Performance Optimized:** Efficient queries with proper data loading strategies

## Security Considerations

- Admins can only access users from their own company (based on JWT CompanyId claim)
- SuperAdmins can access users from any company
- All endpoints require proper authentication and authorization
- Sensitive user data is appropriately protected
- Input validation and sanitization applied to all query parameters
