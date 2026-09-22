using StaffManagementApp.Data.Entities;

namespace StaffManagementApp.Services;

public class DepartmentTreeNode
{
    public Department Department { get; set; } = null!;
    public List<DepartmentTreeNode> Children { get; set; } = new();
}

public interface IDepartmentService
{
    Task<List<Department>> GetAllAsync();
    Task<List<DepartmentTreeNode>> GetTreeAsync();
    Task<Department?> GetByIdAsync(int id);
    Task<Department> CreateAsync(Department department);
    Task<Department> UpdateAsync(Department department);
    Task LiquidateAsync(int id);
    Task<List<Department>> GetActiveAsync();
    /// <summary>
    /// Проверяет уникальность кода подразделения. При редактировании передаётся excludeDepartmentId.
    /// </summary>
    Task<bool> IsCodeUniqueAsync(string code, int? excludeDepartmentId = null);
    /// <summary>
    /// Восстанавливает ликвидированное подразделение (снимает ClosedAt). 
    /// Проверяет, что родитель активен.
    /// </summary>
    Task RestoreAsync(int id);
    /// <summary>
    /// Физически удаляет подразделение из БД.
    /// Разрешено только если нет CareerRecords и нет дочерних подразделений.
    /// </summary>
    Task HardDeleteAsync(int id);
}
