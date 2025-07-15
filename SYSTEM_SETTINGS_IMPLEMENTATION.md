# System Settings Implementation

## Overview
Implemented a centralized system settings management feature that allows SuperAdmins to configure platform-wide fee structures including legal fees, management fees, and processing fees.

## Features

### System Settings Model
- **Legal Fees**: Configurable legal processing fees
- **Management Fees**: Platform management charges
- **Processing Fees**: Transaction processing costs
- **Additional Fees**: Penalty, late, and documentation fees (optional)
- **Fee Types**: Support for both PERCENTAGE and FIXED fee calculations
- **Audit Trail**: Tracks creation and modification history

### API Endpoints (SuperAdmin Only)

#### Base URL: `/api/system/settings`

1. **GET /api/system/settings**
   - Retrieves all system settings (historical records)
   - Returns ordered list by creation date

2. **GET /api/system/settings/active**
   - Retrieves currently active system settings
   - Only one settings record can be active at a time

3. **GET /api/system/settings/{id}**
   - Retrieves specific settings by ID
   - Useful for viewing historical configurations

4. **POST /api/system/settings**
   - Creates new system settings
   - Automatically deactivates any existing active settings
   - Ensures only one active configuration exists

5. **PUT /api/system/settings/{id}**
   - Updates existing system settings
   - Maintains audit trail with update timestamp and user

## Business Rules

1. **Single Active Configuration**: Only one SystemSettings record can be active at any time
2. **Automatic Deactivation**: Creating new settings automatically deactivates old ones
3. **Audit Trail**: All changes are tracked with user ID and timestamps
4. **Validation**: 
   - All fee amounts must be positive values
   - Fee types must be either "PERCENTAGE" or "FIXED"
   - All required fields must be provided

## Security
- All endpoints restricted to SuperAdmin role only
- User authentication required for all operations
- All changes logged with user identification

## Database Design

### SystemSettings Table
```sql
CREATE TABLE SystemSettings (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    LegalFees DECIMAL(18,2) NOT NULL,
    ManagementFees DECIMAL(18,2) NOT NULL,
    ProcessingFees DECIMAL(18,2) NOT NULL,
    PenaltyFees DECIMAL(18,2) NULL,
    LateFees DECIMAL(18,2) NULL,
    DocumentationFees DECIMAL(18,2) NULL,
    LegalFeesType NVARCHAR(50) NOT NULL DEFAULT 'PERCENTAGE',
    ManagementFeesType NVARCHAR(50) NOT NULL DEFAULT 'PERCENTAGE',
    ProcessingFeesType NVARCHAR(50) NOT NULL DEFAULT 'PERCENTAGE',
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL,
    CreatedBy NVARCHAR(450) NULL,
    UpdatedBy NVARCHAR(450) NULL
);
```

## Implementation Components

### Models
- `SystemSettings.cs`: Core entity model
- `SystemSettingsDto.cs`: Data transfer objects

### Repository
- `ISystemSettingsRepository.cs`: Repository interface
- `SystemSettingsRepository.cs`: Repository implementation

### Service
- `ISystemSettingsService.cs`: Service interface  
- `SystemSettingsService.cs`: Business logic implementation

### Controller
- `SystemSettingsController.cs`: API endpoints

### Configuration
- Added to `ApplicationDbContext.cs`
- Registered in `ServiceExtensions.cs`
- Database migration created and applied

## Usage Example

### Creating New Settings
```json
POST /api/system/settings
{
    "legalFees": 5.0,
    "managementFees": 3.5,
    "processingFees": 2.0,
    "penaltyFees": 10.0,
    "lateFees": 15.0,
    "documentationFees": 1.0,
    "legalFeesType": "PERCENTAGE",
    "managementFeesType": "PERCENTAGE", 
    "processingFeesType": "FIXED"
}
```

### Response Format
```json
{
    "success": true,
    "message": "System settings created successfully",
    "data": {
        "id": "guid",
        "legalFees": 5.0,
        "managementFees": 3.5,
        "processingFees": 2.0,
        "penaltyFees": 10.0,
        "lateFees": 15.0,
        "documentationFees": 1.0,
        "legalFeesType": "PERCENTAGE",
        "managementFeesType": "PERCENTAGE",
        "processingFeesType": "FIXED",
        "isActive": true,
        "createdAt": "2025-07-15T10:30:00Z",
        "updatedAt": "2025-07-15T10:30:00Z",
        "createdBy": "user-id",
        "updatedBy": "user-id"
    }
}
```

## Notes
- Backward compatible design
- No duplicate endpoints for system settings management
- Clear separation between AdminSettings (general config) and SystemSettings (fee management)
- Comprehensive error handling and logging
- Follows existing project patterns and conventions
