using StaffManagementApp.Data.Entities;

namespace StaffManagementApp.Services;

public interface ICareerService
{
    Task HireAsync(Employee employee, int departmentId, string position, DateTime date, string? comment = null);
    Task TransferAsync(int employeeId, int newDepartmentId, string newPosition, DateTime date, string? comment = null);
    Task DismissAsync(int employeeId, DateTime date, string? comment = null);
    Task<CareerRecord?> GetCurrentRecordAsync(int employeeId);
    Task<List<CareerRecord>> GetHistoryAsync(int employeeId);
    Task<bool> IsEmployeeActiveAsync(int employeeId);
}
