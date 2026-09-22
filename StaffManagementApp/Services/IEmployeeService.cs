using StaffManagementApp.Data.Entities;

namespace StaffManagementApp.Services;

public class EmployeeWithStatus
{
    public Employee Employee { get; set; } = null!;
    public CareerRecord? CurrentRecord { get; set; }
    public bool IsActive { get; set; }
}

public interface IEmployeeService
{
    Task<List<EmployeeWithStatus>> GetAllWithStatusAsync(string? search = null);
    Task<Employee?> GetByIdAsync(int id);
    Task<Employee> CreateAsync(Employee employee);
    Task<Employee> UpdateAsync(Employee employee);
    /// <summary>
    /// Проверяет уникальность табельного номера. При редактировании передаётся excludeEmployeeId.
    /// </summary>
    Task<bool> IsPersonnelNumberUniqueAsync(string personnelNumber, int? excludeEmployeeId = null);
}
