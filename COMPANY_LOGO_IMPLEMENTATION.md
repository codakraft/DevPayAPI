# Company Logo Implementation Documentation

## Overview
This document outlines the implementation of company logo support in the LendingSolution system, where company logos are stored as document references.

## Implementation Details

### 1. Database Changes
- **New Field**: Added `LogoDocumentId` (Guid?, nullable) to the `Company` table
- **Migration**: `20250716180204_AddCompanyLogo.cs` 
- **Default Value**: NULL for existing companies (backward compatibility)

### 2. Model Updates

#### Company Model (`LendingSolution.Core.Models.Company.cs`)
```csharp
public class Company : Base
{
    // ... existing properties ...
    public Guid? LogoDocumentId { get; set; }
    // ... existing properties ...
}
```

### 3. DTOs Updated

#### CompanyDto (`LendingSolution.Core.Dtos.CompanyDto.cs`)
- Added `LogoDocumentId` property to base `CompanyDto`
- Inherited by `CompanyResponseDto`, `CreateCompanyRequestDto`, `UpdateCompanyRequestDto`
- New: `UpdateCompanyLogoDto` for logo-only updates

#### CompanyListDto (`LendingSolution.Core.Dtos.SupportDto.cs`)
- Added `LogoDocumentId` property
- Added `LogoUrl` property for display purposes (can be populated by document service)

### 4. Service Layer Updates

#### CompanyService (`LendingSolution.Application.Services.Implementations.CompanyService.cs`)
Updated methods to handle logo field:
- `CreateCompany()` - Includes logo in company creation
- `UpdateCompany()` - Includes logo in company updates
- `GetCompanyById()` - Returns logo in response
- `GetUserCompany()` - Returns logo in response
- `GetCompanyByIdAsync()` - Includes logo in detailed company info
- `GetAllCompaniesAsync()` - Includes logo in company listings

### 5. API Endpoints

#### New Endpoints Added

##### Admin Logo Management
```
PATCH /api/company/logo
```
- **Description**: Allows company admins to update their own company logo
- **Authorization**: Admin role required
- **Body**: `UpdateCompanyLogoDto { LogoDocumentId }`
- **Response**: Updated company information

##### SuperAdmin Logo Management
```
PATCH /api/company/sa/{companyId}/logo
```
- **Description**: Allows SuperAdmin to update any company's logo
- **Authorization**: SuperAdmin role required
- **Parameters**: `companyId` (Guid)
- **Body**: `UpdateCompanyLogoDto { LogoDocumentId }`
- **Response**: Updated company information

#### Updated Existing Endpoints
All existing company endpoints now include logo support:
- `POST /api/company/sa/create` - Create company with logo
- `PUT /api/company/info` - Update company info including logo
- `GET /api/company/my-company` - Get company with logo
- `GET /api/company/sa/all` - List all companies with logos
- `GET /api/company/sa/{companyId}` - Get specific company with logo

### 6. Integration with Document Service

The logo system integrates with the existing document upload feature:

1. **Upload Document**: Use `POST /api/documents/upload` to upload logo image
2. **Get Document ID**: Receive `DocumentId` from upload response
3. **Update Company Logo**: Use the document ID in company logo endpoints
4. **Retrieve Logo**: Use `GET /api/documents/{documentId}` to get logo URL

### 7. Usage Flow

#### For Company Admins:
1. Upload logo image via document service
2. Get the returned document ID
3. Update company logo using `PATCH /api/company/logo`

#### For SuperAdmins:
1. Upload logo image via document service
2. Get the returned document ID  
3. Update any company's logo using `PATCH /api/company/sa/{companyId}/logo`

### 8. Backward Compatibility

- Existing companies have `LogoDocumentId` set to NULL
- All existing endpoints continue to work
- Logo field is optional in all operations
- No breaking changes to existing API contracts

### 9. Data Validation

- `LogoDocumentId` is nullable and optional
- When provided, should reference a valid document in the system
- Document validation can be added in future iterations

### 10. Future Enhancements

1. **Logo URL Population**: Integrate with document service to automatically populate `LogoUrl` field
2. **Logo Validation**: Validate that the document ID references an image file
3. **Logo Constraints**: Add file type and size restrictions for logos
4. **Caching**: Implement logo URL caching for better performance

## Testing

### Test Scenarios:
1. Create company with logo
2. Create company without logo
3. Update company logo (Admin)
4. Update company logo (SuperAdmin)
5. Get company details with logo
6. List companies with logos
7. Handle invalid document IDs

### Sample Request/Response:

#### Update Company Logo
```json
PATCH /api/company/logo
{
    "logoDocumentId": "123e4567-e89b-12d3-a456-426614174000"
}
```

Response:
```json
{
    "success": true,
    "message": "Company logo updated successfully",
    "data": {
        "id": "456e7890-e89b-12d3-a456-426614174001",
        "name": "TechCorp Solutions",
        "shortName": "TechCorp",
        "logoDocumentId": "123e4567-e89b-12d3-a456-426614174000",
        // ... other company fields
    }
}
```

## Migration Instructions

1. **Apply Migration**: `dotnet ef database update`
2. **Verify Schema**: Check that `LogoDocumentId` column exists in `Companies` table
3. **Test Endpoints**: Verify all company endpoints work with new logo field
4. **Update Client Applications**: Update frontend to support logo upload and display

---

**Implementation Date**: July 16, 2025  
**Status**: ✅ COMPLETED  
**Migration Applied**: ✅ YES  
**Backward Compatible**: ✅ YES
