namespace StaffManagementApp.Data.Entities;

public enum Gender { Male, Female }

public class Employee
{
    public int Id { get; set; }
    public string PersonnelNumber { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public Gender Gender { get; set; }
    public DateTime BirthDate { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? PhotoPath { get; set; }

    public ICollection<CareerRecord> CareerRecords { get; set; } = new List<CareerRecord>();

    public string FullName => string.IsNullOrWhiteSpace(MiddleName)
        ? $"{LastName} {FirstName}"
        : $"{LastName} {FirstName} {MiddleName}";
}
