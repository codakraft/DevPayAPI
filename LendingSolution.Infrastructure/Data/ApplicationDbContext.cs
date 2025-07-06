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
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<LoanProduct> LoanProducts { get; set; }
    public DbSet<Account> Accounts { get; set; }

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

        modelBuilder.Entity<Loan>()
            .Property(l => l.Amount)
            .HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Repayment>()
            .Property(r => r.Amount)
            .HasColumnType("decimal(18,2)");
    }
}