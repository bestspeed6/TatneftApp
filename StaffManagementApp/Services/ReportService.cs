using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using StaffManagementApp.Data;
using StaffManagementApp.Data.Entities;

namespace StaffManagementApp.Services;

public class ReportService(IDbContextFactory<AppDbContext> factory) : IReportService
{
    public async Task<List<ReportRow>> GetReportAsync(DateTime startDate, DateTime endDate, EventType? eventType = null)
    {
        await using var ctx = await factory.CreateDbContextAsync();

        var start = startDate.ToUniversalTime();
        var end = endDate.Date.AddDays(1).ToUniversalTime(); // inclusive end date

        var query = ctx.CareerRecords
            .Include(cr => cr.Employee)
            .Include(cr => cr.Department)
            .Where(cr => cr.EventDate >= start && cr.EventDate < end);

        if (eventType.HasValue)
            query = query.Where(cr => cr.EventType == eventType.Value);

        var records = await query.OrderBy(cr => cr.EventDate).ToListAsync();

        return records.Select(cr => new ReportRow
        {
            PersonnelNumber = cr.Employee.PersonnelNumber,
            FullName = cr.Employee.FullName,
            DepartmentName = cr.Department.Name,
            Position = cr.Position,
            EventType = cr.EventType,
            EventDate = cr.EventDate,
            Comment = cr.Comment
        }).ToList();
    }

    public byte[] ExportToExcel(List<ReportRow> rows)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Кадровый отчёт");

        // Header
        sheet.Cell(1, 1).Value = "Таб. №";
        sheet.Cell(1, 2).Value = "ФИО";
        sheet.Cell(1, 3).Value = "Подразделение";
        sheet.Cell(1, 4).Value = "Должность";
        sheet.Cell(1, 5).Value = "Событие";
        sheet.Cell(1, 6).Value = "Дата";
        sheet.Cell(1, 7).Value = "Комментарий";

        var headerRow = sheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightBlue;

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            sheet.Cell(i + 2, 1).Value = row.PersonnelNumber;
            sheet.Cell(i + 2, 2).Value = row.FullName;
            sheet.Cell(i + 2, 3).Value = row.DepartmentName;
            sheet.Cell(i + 2, 4).Value = row.Position;
            sheet.Cell(i + 2, 5).Value = row.EventType switch
            {
                EventType.Hire => "Приём",
                EventType.Transfer => "Перевод",
                EventType.Dismissal => "Увольнение",
                _ => row.EventType.ToString()
            };
            sheet.Cell(i + 2, 6).Value = row.EventDate.ToLocalTime().ToString("dd.MM.yyyy");
            sheet.Cell(i + 2, 7).Value = row.Comment ?? "";
        }

        sheet.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }
}
