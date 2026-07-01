using LendingSolution.Core.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LendingSolution.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Loan> Loans { get; set; }
    public DbSet<Repayment> Repayments { get; set; }
    public DbSet<Disbursement> Disbursements { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<LoanProduct> LoanProducts { get; set; }
    public DbSet<AdminSettings> AdminSettings { get; set; }
    public DbSet<Settings> Settings { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<Approval> Approvals { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<SupportTicket> SupportTickets { get; set; }
    public DbSet<SupportComment> SupportComments { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<WalletTransaction> WalletTransactions { get; set; }
    public DbSet<BorrowerApplication> BorrowerApplications { get; set; }
    public DbSet<RemitaSalaryHistory> RemitaSalaryHistories { get; set; }
    public DbSet<RemitaSalaryPayment> RemitaSalaryPayments { get; set; }
    public DbSet<RemitaLoanCollectionNotification> RemitaLoanCollectionNotifications { get; set; }
    public DbSet<MonoMandateReference> MonoMandateReferences { get; set; }
    public DbSet<MonoBvnVerificationRecord> MonoBvnVerificationRecords { get; set; }
    public DbSet<MonoCreditAnalysisRecord> MonoCreditAnalysisRecords { get; set; }
    public DbSet<Otp> Otps { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<MfaSession> MfaSessions { get; set; }
    public DbSet<AppLog> AppLogs { get; set; }
    public DbSet<EWallet> EWallets { get; set; }
    public DbSet<EWalletTransaction> EWalletTransactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User relationship with NO ACTION cascade
        modelBuilder.Entity<Loan>()
            .HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        // Configure Company relationship with NO ACTION cascade  
        modelBuilder.Entity<Loan>()
            .HasOne(l => l.Company)
            .WithMany()
            .HasForeignKey(l => l.CompanyId)
            .OnDelete(DeleteBehavior.NoAction);

        // Configure User email unique index
        modelBuilder.Entity<ApplicationUser>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Configure AdminSettings unique key index
        modelBuilder.Entity<AdminSettings>()
            .HasIndex(s => s.SettingKey)
            .IsUnique();

        // Configure Settings decimal precision
        modelBuilder.Entity<Settings>()
            .Property(s => s.LegalFee)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Settings>()
            .Property(s => s.MaintenanceFee)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Settings>()
            .Property(s => s.ProcessingFee)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Settings>()
            .Property(s => s.PenaltyFee)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Settings>()
            .Property(s => s.LateFee)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Settings>()
            .Property(s => s.OtpFee)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Settings>()
            .Property(s => s.DocumentationFee)
            .HasPrecision(18, 2);

        // Configure enum conversion for FeeType
        modelBuilder.Entity<Settings>()
            .Property(s => s.OtpFeeType)
            .HasConversion<int>();

        modelBuilder.Entity<Settings>()
            .Property(s => s.LegalFeeType)
            .HasConversion<int>();

        modelBuilder.Entity<Settings>()
            .Property(s => s.MaintenanceFeeType)
            .HasConversion<int>();

        modelBuilder.Entity<Settings>()
            .Property(s => s.ProcessingFeeType)
            .HasConversion<int>();

        // Configure Approval relationships
        modelBuilder.Entity<Approval>()
            .HasOne(a => a.RequestedByUser)
            .WithMany()
            .HasForeignKey(a => a.RequestedBy)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Approval>()
            .HasOne(a => a.ProcessedByUser)
            .WithMany()
            .HasForeignKey(a => a.ProcessedBy)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Approval>()
            .HasOne(a => a.Company)
            .WithMany()
            .HasForeignKey(a => a.CompanyId)
            .OnDelete(DeleteBehavior.NoAction);

        // Configure RefreshToken relationships
        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.RevokedByUser)
            .WithMany()
            .HasForeignKey(rt => rt.RevokedByUserId)
            .OnDelete(DeleteBehavior.NoAction);

        // Configure RefreshToken token unique index
        modelBuilder.Entity<RefreshToken>()
            .HasIndex(rt => rt.Token)
            .IsUnique();

        // Configure decimal precision for financial entities
        modelBuilder.Entity<Loan>()
            .Property(l => l.Amount)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Repayment>()
            .Property(r => r.TotalDue)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Repayment>()
            .Property(r => r.TotalRepaid)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Repayment>()
            .Property(r => r.AmountUnpaid)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Disbursement>()
            .Property(d => d.Amount)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<LoanProduct>()
            .Property(lp => lp.InterestRate)
            .HasPrecision(5, 4); // Allow for rates like 15.2500%

        modelBuilder.Entity<LoanProduct>()
            .Property(lp => lp.MaxAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<LoanProduct>()
            .Property(lp => lp.MinAmount)
            .HasPrecision(18, 2);

        // Configure SupportTicket relationships
        modelBuilder.Entity<SupportTicket>()
            .HasOne(st => st.User)
            .WithMany()
            .HasForeignKey(st => st.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<SupportTicket>()
            .HasOne(st => st.Company)
            .WithMany()
            .HasForeignKey(st => st.CompanyId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<SupportTicket>()
            .HasOne(st => st.AssignedToUser)
            .WithMany()
            .HasForeignKey(st => st.AssignedTo)
            .OnDelete(DeleteBehavior.NoAction);

        // Configure SupportComment relationships
        modelBuilder.Entity<SupportComment>()
            .HasOne(sc => sc.Ticket)
            .WithMany(st => st.Comments)
            .HasForeignKey(sc => sc.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SupportComment>()
            .HasOne(sc => sc.User)
            .WithMany()
            .HasForeignKey(sc => sc.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        // Configure BorrowerApplication relationships with NO ACTION cascade
        modelBuilder.Entity<BorrowerApplication>()
            .HasOne(ba => ba.Company)
            .WithMany()
            .HasForeignKey(ba => ba.CompanyId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<BorrowerApplication>()
            .HasOne(ba => ba.Product)
            .WithMany()
            .HasForeignKey(ba => ba.ProductId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<BorrowerApplication>()
            .HasOne(ba => ba.Loan)
            .WithOne(l => l.BorrowerApplication)
            .HasForeignKey<BorrowerApplication>(ba => ba.LoanId)
            .OnDelete(DeleteBehavior.NoAction);

        // Configure decimal precision for BorrowerApplication
        modelBuilder.Entity<BorrowerApplication>()
            .Property(ba => ba.MaxLoanEligible)
            .HasPrecision(18, 2);

        modelBuilder.Entity<BorrowerApplication>()
            .Property(ba => ba.MinLoanEligible)
            .HasPrecision(18, 2);

        // Configure indexes for BorrowerApplication
        modelBuilder.Entity<BorrowerApplication>()
            .HasIndex(ba => ba.Email);

        modelBuilder.Entity<BorrowerApplication>()
            .HasIndex(ba => ba.CompanyId);

        modelBuilder.Entity<BorrowerApplication>()
            .HasIndex(ba => ba.ProductId);

        modelBuilder.Entity<BorrowerApplication>()
            .HasIndex(ba => ba.CurrentStep);

        // Configure indexes for better performance
        modelBuilder.Entity<SupportTicket>()
            .HasIndex(st => st.Status);

        modelBuilder.Entity<SupportTicket>()
            .HasIndex(st => st.Priority);

        modelBuilder.Entity<SupportTicket>()
            .HasIndex(st => st.Category);

        modelBuilder.Entity<SupportTicket>()
            .HasIndex(st => st.CreatedAt);

        modelBuilder.Entity<SupportTicket>()
            .HasIndex(st => new { st.CompanyId, st.Status });

        modelBuilder.Entity<SupportTicket>()
            .HasIndex(st => new { st.UserId, st.Status });

        // Configure Wallet relationships
        modelBuilder.Entity<Wallet>()
            .HasOne(w => w.Company)
            .WithMany()
            .HasForeignKey(w => w.CompanyId)
            .OnDelete(DeleteBehavior.NoAction);

        // Configure WalletTransaction relationships
        modelBuilder.Entity<WalletTransaction>()
            .HasOne(wt => wt.Wallet)
            .WithMany(w => w.Transactions)
            .HasForeignKey(wt => wt.WalletId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WalletTransaction>()
            .HasOne(wt => wt.InitiatedByUser)
            .WithMany()
            .HasForeignKey(wt => wt.InitiatedBy)
            .OnDelete(DeleteBehavior.NoAction);

        // Configure decimal precision for wallet entities
        modelBuilder.Entity<Wallet>()
            .Property(w => w.Balance)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Wallet>()
            .Property(w => w.TotalCredits)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Wallet>()
            .Property(w => w.TotalDebits)
            .HasPrecision(18, 2);

        modelBuilder.Entity<WalletTransaction>()
            .Property(wt => wt.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<WalletTransaction>()
            .Property(wt => wt.BalanceAfter)
            .HasPrecision(18, 2);

        // Configure indexes for wallet entities
        modelBuilder.Entity<Wallet>()
            .HasIndex(w => w.CompanyId)
            .IsUnique();

        modelBuilder.Entity<Wallet>()
            .HasIndex(w => w.IsSuperAdminWallet);

        modelBuilder.Entity<WalletTransaction>()
            .HasIndex(wt => wt.WalletId);

        modelBuilder.Entity<WalletTransaction>()
            .HasIndex(wt => wt.TransactionType);

        modelBuilder.Entity<WalletTransaction>()
            .HasIndex(wt => wt.CreatedAt);

        modelBuilder.Entity<WalletTransaction>()
            .HasIndex(wt => wt.PaystackReference);

        // Configure RemitaSalaryHistory relationships (one-to-one with BorrowerApplication)
        modelBuilder.Entity<RemitaSalaryHistory>()
            .HasOne(rsh => rsh.BorrowerApplication)
            .WithOne(ba => ba.RemitaSalaryHistory)
            .HasForeignKey<RemitaSalaryHistory>(rsh => rsh.BorrowerApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RemitaSalaryPayment>()
            .HasOne(rsp => rsp.RemitaSalaryHistory)
            .WithMany(rsh => rsh.SalaryPayments)
            .HasForeignKey(rsp => rsp.RemitaSalaryHistoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Add indexes for RemitaSalaryHistory
        modelBuilder.Entity<RemitaSalaryHistory>()
            .HasIndex(rsh => rsh.BorrowerApplicationId);

        modelBuilder.Entity<RemitaSalaryHistory>()
            .HasIndex(rsh => rsh.CustomerId);

        modelBuilder.Entity<RemitaSalaryHistory>()
            .HasIndex(rsh => new { rsh.AccountNumber, rsh.BankCode });

        modelBuilder.Entity<RemitaSalaryPayment>()
            .HasIndex(rsp => rsp.RemitaSalaryHistoryId);

        modelBuilder.Entity<RemitaSalaryPayment>()
            .HasIndex(rsp => rsp.PaymentDate);

        // Configure MonoMandateReference foreign key relationships with NO ACTION to avoid cascade cycles
        modelBuilder.Entity<MonoMandateReference>()
            .HasOne(m => m.Company)
            .WithMany()
            .HasForeignKey(m => m.CompanyId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<MonoMandateReference>()
            .HasOne(m => m.Loan)
            .WithMany()
            .HasForeignKey(m => m.LoanId)
            .OnDelete(DeleteBehavior.NoAction);

        // Configure MonoBvnVerificationRecord indexes for performance
        modelBuilder.Entity<MonoBvnVerificationRecord>()
            .HasIndex(b => b.BvnHash);

        modelBuilder.Entity<MonoBvnVerificationRecord>()
            .HasIndex(b => b.CreatedAt);

        modelBuilder.Entity<MonoBvnVerificationRecord>()
            .HasIndex(b => b.ExpiresAt);

        // Configure MonoCreditAnalysisRecord indexes for performance
        modelBuilder.Entity<MonoCreditAnalysisRecord>()
            .HasIndex(c => c.BvnHash);

        modelBuilder.Entity<MonoCreditAnalysisRecord>()
            .HasIndex(c => c.CreatedAt);

        modelBuilder.Entity<MonoCreditAnalysisRecord>()
            .HasIndex(c => c.ExpiresAt);
    }

    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // {
    //     if (!optionsBuilder.IsConfigured)
    //     {
    //         // Enable retry on failure for transient errors
    //         optionsBuilder.UseSqlServer(connectionString, options =>
    //         {
    //             options.EnableRetryOnFailure(
    //                 maxRetryCount: 5,
    //                 maxRetryDelay: TimeSpan.FromSeconds(30),
    //                 errorNumbersToAdd: null);
    //         });

    //         // Suppress pending changes warning
    //         optionsBuilder.ConfigureWarnings(warnings => 
    //             warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    //     }
    // }
}