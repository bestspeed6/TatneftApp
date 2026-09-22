using StaffManagementApp.Data.Entities;

namespace StaffManagementApp.Services;

public class ReportRow
{
    public string PersonnelNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public EventType EventType { get; set; }
    public DateTime EventDate { get; set; }
    public string? Comment { get; set; }
}

public interface IReportService
{
    Task<List<ReportRow>> GetReportAsync(DateTime startDate, DateTime endDate, EventType? eventType = null);
    byte[] ExportToExcel(List<ReportRow> rows);
}
