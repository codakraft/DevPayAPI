using LendingSolution.Core.Models;

namespace LendingSolution.Application.Repositories.Interfaces;

public interface IEmployeeRepository
{
    Task<bool> CreateEmployeeAsync(Employee employee);
    Task<Employee?> GetEmployeeByUserIdAndLoanIdAsync(string userId, Guid loanId);
    Task<Employee?> GetEmployeeByIdAsync(Guid id);
    Task<IEnumerable<Employee>> GetEmployeesByUserIdAsync(string userId);
    Task<bool> UpdateEmployeeAsync(Employee employee);
    Task<bool> DeleteEmployeeAsync(Guid id);
    Task<bool> ExistsAsync(string userId, Guid loanId);
}
