# Company Users API Test Examples

## Test Case 1: Basic User List with Roles
**Request:**
```bash
GET /api/company/{companyId}/users
```

**Expected Response:**
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
        "roles": ["Admin", "User"]
      }
    ],
    "totalCount": 1,
    "page": 1,
    "pageSize": 20,
    "totalPages": 1,
    "hasNextPage": false,
    "hasPreviousPage": false,
    "totalActiveUsers": 1,
    "totalInactiveUsers": 0
  }
}
```

## Test Case 2: Filter by Role
**Request:**
```bash
GET /api/company/{companyId}/users?Role=Admin
```

**Expected Behavior:**
- Should only return users who have the "Admin" role
- Users with multiple roles including "Admin" should be included
- Users without "Admin" role should be excluded

## Test Case 3: Sort by Role
**Request:**
```bash
GET /api/company/{companyId}/users?SortBy=Role&SortOrder=asc
```

**Expected Behavior:**
- Users should be sorted alphabetically by their role names
- Users with multiple roles will be sorted by the concatenated role string
- Example order: "Admin", "Admin, SuperAdmin", "SuperAdmin", "User"

## Test Case 4: Combined Filtering
**Request:**
```bash
GET /api/company/{companyId}/users?Search=admin&Role=Admin&IsActive=true&SortBy=CreatedAt&SortOrder=desc
```

**Expected Behavior:**
- Apply search filter first (firstName, lastName, email, phone contains "admin")
- Filter by users having "Admin" role
- Filter by active users only
- Sort by creation date in descending order
- Return paginated results

## Validation Points

### ✅ Role Data Integrity
- Each user should have accurate role information
- Roles should match what's stored in ASP.NET Core Identity
- Empty roles array should be returned for users with no roles

### ✅ Performance Considerations
- Role fetching should be efficient (batch processing)
- Large user lists should be properly paginated
- Database queries should be optimized

### ✅ Security & Authorization
- Only company users should be returned (proper company scoping)
- Role information should be appropriately filtered based on requester permissions
- No sensitive role information should be exposed unnecessarily

## Common Use Cases

1. **Admin Dashboard**: List all company users with their roles for management
2. **Role Management**: Filter users by specific roles for bulk operations
3. **Access Control Audit**: Sort and filter users to verify role assignments
4. **User Search**: Find specific users and their roles for support purposes
