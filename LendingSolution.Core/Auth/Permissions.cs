namespace LendingSolution.Core.Auth;

/// <summary>
/// Permission names carried in the JWT as "permission" claims and checked with [HasPermission].
/// </summary>
public static class Permissions
{
    public const string ClaimType = "permission";

    public static class Loans
    {
        public const string View = "loans.view";
        public const string Manage = "loans.manage";     // offer letters, document re-upload requests
        public const string Approve = "loans.approve";   // approve / reject / process
        public const string Disburse = "loans.disburse";
    }

    public static class Collections
    {
        public const string Manage = "collections.manage"; // stop collection, reconcile
    }

    public static class Products
    {
        public const string View = "products.view";
        public const string Manage = "products.manage";
    }

    public static class Company
    {
        public const string View = "company.view";
        public const string Manage = "company.manage";
    }

    public static class Users
    {
        public const string View = "users.view";
        public const string Manage = "users.manage";
    }

    public static class Finance
    {
        public const string View = "finance.view";
        public const string Manage = "finance.manage";
    }

    public static class Audit
    {
        public const string View = "audit.view";
    }

    public static class Support
    {
        public const string View = "support.view";
        public const string Manage = "support.manage";
    }

    public static readonly IReadOnlyList<string> All =
    [
        Loans.View, Loans.Manage, Loans.Approve, Loans.Disburse,
        Collections.Manage,
        Products.View, Products.Manage,
        Company.View, Company.Manage,
        Users.View, Users.Manage,
        Finance.View, Finance.Manage,
        Audit.View,
        Support.View, Support.Manage,
    ];
}

/// <summary>
/// Default permissions per role. Synced to the role claims table on startup, so this file
/// is the source of truth: edit it and redeploy to change what a role can do.
/// SuperAdmin-only platform endpoints (companies, settings, cross-company views) stay role-gated.
/// </summary>
public static class RolePermissions
{
    public static readonly IReadOnlyDictionary<string, string[]> Defaults = new Dictionary<string, string[]>
    {
        ["SuperAdmin"] = [.. Permissions.All],
        ["Admin"] = [.. Permissions.All],
        ["LoanOfficer"] =
        [
            Permissions.Loans.View, Permissions.Loans.Manage,
            Permissions.Products.View, Permissions.Company.View,
        ],
        ["Underwriter"] =
        [
            Permissions.Loans.View, Permissions.Loans.Manage, Permissions.Loans.Approve,
            Permissions.Products.View, Permissions.Company.View,
        ],
        ["CollectionsOfficer"] =
        [
            Permissions.Loans.View, Permissions.Collections.Manage,
            Permissions.Finance.View, Permissions.Company.View,
        ],
        ["FinanceOfficer"] =
        [
            Permissions.Loans.View, Permissions.Loans.Disburse,
            Permissions.Finance.View, Permissions.Finance.Manage, Permissions.Company.View,
        ],
        ["SupportAgent"] =
        [
            Permissions.Support.View, Permissions.Support.Manage, Permissions.Loans.View,
        ],
        ["Auditor"] =
        [
            Permissions.Loans.View, Permissions.Products.View, Permissions.Company.View,
            Permissions.Users.View, Permissions.Finance.View, Permissions.Audit.View,
        ],
        ["Viewer"] =
        [
            Permissions.Loans.View, Permissions.Products.View, Permissions.Company.View,
        ],
    };
}
