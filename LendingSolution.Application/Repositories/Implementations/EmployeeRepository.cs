using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Core.Models;
using LendingSolution.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LendingSolution.Application.Repositories.Implementations;

public class EmployeeRepository(ApplicationDbContext context) : IEmployeeRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<bool> CreateEmployeeAsync(Employee employee)
    {
        _context.Employees.Add(employee);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<Employee?> GetEmployeeByUserIdAndLoanIdAsync(string userId, Guid loanId)
    {
        return await _context.Employees
            .FirstOrDefaultAsync(e => e.UserId == userId && e.LoanId == loanId);
    }

    public async Task<Employee?> GetEmployeeByIdAsync(Guid id)
    {
        return await _context.Employees.FindAsync(id);
    }

    public async Task<IEnumerable<Employee>> GetEmployeesByUserIdAsync(string userId)
    {
        return await _context.Employees
            .Where(e => e.UserId == userId)
            .ToListAsync();
    }

    public async Task<bool> UpdateEmployeeAsync(Employee employee)
    {
        _context.Employees.Update(employee);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteEmployeeAsync(Guid id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return false;

        _context.Employees.Remove(employee);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> ExistsAsync(string userId, Guid loanId)
    {
        return await _context.Employees
            .AnyAsync(e => e.UserId == userId && e.LoanId == loanId);
    }
}
