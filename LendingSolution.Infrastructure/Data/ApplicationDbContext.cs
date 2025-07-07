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
    public DbSet<Account> Accounts { get; set; }
    public DbSet<AdminSettings> AdminSettings { get; set; }
    public DbSet<Approval> Approvals { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<SupportTicket> SupportTickets { get; set; }
    public DbSet<SupportComment> SupportComments { get; set; }

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

        // Configure Account relationship (if you have one)
        modelBuilder.Entity<Loan>()
            .HasOne(l => l.Account)
            .WithMany()
            .HasForeignKey(l => l.AccountId)
            .OnDelete(DeleteBehavior.NoAction);

        // Configure User email unique index
        modelBuilder.Entity<ApplicationUser>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Configure AdminSettings unique key index
        modelBuilder.Entity<AdminSettings>()
            .HasIndex(s => s.SettingKey)
            .IsUnique();

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
            .Property(r => r.Amount)
            .HasColumnType("decimal(18,2)");
            
        modelBuilder.Entity<Disbursement>()
            .Property(d => d.Amount)
            .HasColumnType("decimal(18,2)");

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

        // Configure decimal precision for financial entities
        modelBuilder.Entity<Disbursement>()
            .Property(d => d.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Repayment>()
            .Property(r => r.Amount)
            .HasPrecision(18, 2);

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
    }
}