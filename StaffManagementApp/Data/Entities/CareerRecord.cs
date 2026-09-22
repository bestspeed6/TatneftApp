namespace StaffManagementApp.Data.Entities;

public enum EventType { Hire, Transfer, Dismissal }

public class CareerRecord
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    public string Position { get; set; } = string.Empty;
    public EventType EventType { get; set; }
    public DateTime EventDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Comment { get; set; }
}
