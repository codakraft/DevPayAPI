# Fix for GUID Parameter Issue in Borrower Current Step Endpoint

## Problem
The endpoint was receiving a GUID (`84684B74-ECA4-4D83-8857-ED9C86E1B177`) instead of an email address, causing the error:
```
No borrower application found for email: 84684B74-ECA4-4D83-8857-ED9C86E1B177
```

## Root Cause
The client was calling the endpoint with a borrower application ID (GUID) instead of an email address, but the system was only designed to handle email lookups.

## Solution Implemented

### 1. Enhanced Repository Method
Updated `BorrowerApplicationRepository.GetByEmailAsync()` to handle both email addresses and GUIDs:

```csharp
public async Task<BorrowerApplication?> GetByEmailAsync(string email)
{
    // Check if the input is a GUID - if so, search by ID instead
    if (Guid.TryParse(email, out var applicationId))
    {
        return await _context.BorrowerApplications
            .Include(ba => ba.Company)
            .Include(ba => ba.Product)
            .Include(ba => ba.Loan)
            .FirstOrDefaultAsync(ba => ba.Id == applicationId && ba.IsActive);
    }

    // Otherwise, search by email
    return await _context.BorrowerApplications
        .Include(ba => ba.Company)
        .Include(ba => ba.Product)
        .Include(ba => ba.Loan)
        .Where(ba => ba.Email == email && ba.IsActive)
        .OrderByDescending(ba => ba.CreatedAt)
        .FirstOrDefaultAsync();
}
```

### 2. Updated Controller Documentation
Modified the controller parameter and documentation to reflect that it accepts both email and ID:

```csharp
/// <summary>
/// Get current step status for borrower application (RESTful GET endpoint)
/// </summary>
/// <param name="emailOrId">The borrower's email address or application ID</param>
[HttpGet("{emailOrId}")]
public async Task<IActionResult> GetCurrentStepByEmail(string emailOrId)
```

### 3. Improved Error Messages
Enhanced the service layer to provide more specific error messages:

```csharp
if (borrowerApplication == null)
{
    // Check if it's a GUID to provide more specific error message
    if (Guid.TryParse(request.Email, out _))
    {
        throw new AppException($"No borrower application found for ID: {request.Email}", 404);
    }
    else
    {
        throw new AppException($"No borrower application found for email: {request.Email}", 404);
    }
}
```

## Benefits

1. **Backward Compatibility**: Existing email-based calls continue to work
2. **ID Support**: Now supports lookup by borrower application ID (GUID)
3. **Better Error Messages**: Users get clearer error messages based on input type
4. **Flexible API**: Single endpoint handles both email and ID lookups

## API Usage Examples

### By Email
```http
GET /api/borrower/user@example.com
```

### By Application ID
```http
GET /api/borrower/84684B74-ECA4-4D83-8857-ED9C86E1B177
```

Both calls will now work correctly and return the same borrower current step information.

## Testing
- ✅ Build successful
- ✅ Handles GUID input correctly
- ✅ Maintains email lookup functionality
- ✅ Provides appropriate error messages

The fix resolves the immediate issue while making the API more flexible for future use cases.
