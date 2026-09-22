namespace StaffManagementApp.Data.Entities;

public class Department
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Abbreviation { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }

    public int? ParentDepartmentId { get; set; }
    public Department? ParentDepartment { get; set; }
    public ICollection<Department> ChildrenDepartments { get; set; } = new List<Department>();

    public ICollection<CareerRecord> CareerRecords { get; set; } = new List<CareerRecord>();

    public bool IsActive => ClosedAt == null;
}
