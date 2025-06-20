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
    public DbSet<Account> Account { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Loan>()
            .Property(l => l.Amount)
            .HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Repayment>()
            .Property(r => r.Amount)
            .HasColumnType("decimal(18,2)");
    }
}