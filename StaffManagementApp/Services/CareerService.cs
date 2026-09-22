using Microsoft.EntityFrameworkCore;
using StaffManagementApp.Data;
using StaffManagementApp.Data.Entities;

namespace StaffManagementApp.Services;

public class CareerService(IDbContextFactory<AppDbContext> factory) : ICareerService
{
    public async Task HireAsync(Employee employee, int departmentId, string position, DateTime date, string? comment = null)
    {
        // --- Валидация дат ---
        var today = DateTime.UtcNow.Date;
        var birthDate = employee.BirthDate.Date;
        var hireDate = date.Date;

        // Дата рождения: не ранее 1935, не в будущем
        if (birthDate < new DateTime(1935, 1, 1))
            throw new InvalidOperationException("Дата рождения не может быть ранее 1935 года.");
        if (birthDate > today)
            throw new InvalidOperationException("Дата рождения не может быть в будущем.");

        // Возраст от 16 до 90 лет
        var age = CalculateAge(birthDate, today);
        if (age < 16)
            throw new InvalidOperationException("Возраст сотрудника должен быть не менее 16 лет.");
        if (age > 90)
            throw new InvalidOperationException("Возраст сотрудника не может превышать 90 лет.");

        // Дата приёма: не ранее BirthDate + 16 лет
        var minHireDate = birthDate.AddYears(16);
        if (hireDate < minHireDate)
            throw new InvalidOperationException(
                $"Дата приёма не может быть ранее {minHireDate:dd.MM.yyyy} (16 лет с момента рождения).");

        // Дата приёма: не более чем на 30 дней в будущее
        if (hireDate > today.AddDays(30))
            throw new InvalidOperationException("Дата приёма не может быть более чем на 30 дней в будущем.");

        await using var ctx = await factory.CreateDbContextAsync();
        await using var tx = await ctx.Database.BeginTransactionAsync();

        ctx.Employees.Add(employee);
        await ctx.SaveChangesAsync();

        var record = new CareerRecord
        {
            EmployeeId = employee.Id,
            DepartmentId = departmentId,
            Position = position,
            EventType = EventType.Hire,
            EventDate = date.ToUniversalTime(),
            Comment = comment
        };
        ctx.CareerRecords.Add(record);
        await ctx.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public async Task TransferAsync(int employeeId, int newDepartmentId, string newPosition, DateTime date, string? comment = null)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        await using var tx = await ctx.Database.BeginTransactionAsync();

        // Close current active record
        var current = await ctx.CareerRecords
            .Where(cr => cr.EmployeeId == employeeId && cr.EndDate == null && cr.EventType != EventType.Dismissal)
            .OrderByDescending(cr => cr.EventDate)
            .FirstOrDefaultAsync();

        // Дата перевода не может быть раньше даты предыдущего события
        if (current != null)
        {
            var transferDate = date.Date;
            var lastEventDate = current.EventDate.Date;
            if (transferDate < lastEventDate)
                throw new InvalidOperationException(
                    $"Дата перевода не может быть раньше даты предыдущего кадрового события ({lastEventDate:dd.MM.yyyy}).");

            current.EndDate = date.ToUniversalTime();
        }

        var record = new CareerRecord
        {
            EmployeeId = employeeId,
            DepartmentId = newDepartmentId,
            Position = newPosition,
            EventType = EventType.Transfer,
            EventDate = date.ToUniversalTime(),
            Comment = comment
        };
        ctx.CareerRecords.Add(record);
        await ctx.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public async Task DismissAsync(int employeeId, DateTime date, string? comment = null)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        await using var tx = await ctx.Database.BeginTransactionAsync();

        // Дата увольнения не может быть в будущем
        var dismissDate = date.Date;
        var today = DateTime.UtcNow.Date;
        if (dismissDate > today)
            throw new InvalidOperationException("Дата увольнения не может быть в будущем.");

        // Close current active record
        var current = await ctx.CareerRecords
            .Where(cr => cr.EmployeeId == employeeId && cr.EndDate == null && cr.EventType != EventType.Dismissal)
            .OrderByDescending(cr => cr.EventDate)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Активная запись о работе сотрудника не найдена.");

        // Дата увольнения не может быть раньше даты предыдущего события
        var lastEventDate = current.EventDate.Date;
        if (dismissDate < lastEventDate)
            throw new InvalidOperationException(
                $"Дата увольнения не может быть раньше даты предыдущего кадрового события ({lastEventDate:dd.MM.yyyy}).");

        current.EndDate = date.ToUniversalTime();

        var dismissRecord = new CareerRecord
        {
            EmployeeId = employeeId,
            DepartmentId = current.DepartmentId,
            Position = current.Position,
            EventType = EventType.Dismissal,
            EventDate = date.ToUniversalTime(),
            Comment = comment
        };
        ctx.CareerRecords.Add(dismissRecord);
        await ctx.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public async Task<CareerRecord?> GetCurrentRecordAsync(int employeeId)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        return await ctx.CareerRecords
            .Include(cr => cr.Department)
            .Where(cr => cr.EmployeeId == employeeId && cr.EndDate == null && cr.EventType != EventType.Dismissal)
            .OrderByDescending(cr => cr.EventDate)
            .FirstOrDefaultAsync();
    }

    public async Task<List<CareerRecord>> GetHistoryAsync(int employeeId)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        return await ctx.CareerRecords
            .Include(cr => cr.Department)
            .Where(cr => cr.EmployeeId == employeeId)
            .OrderByDescending(cr => cr.EventDate)
            .ToListAsync();
    }

    public async Task<bool> IsEmployeeActiveAsync(int employeeId)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        var lastRecord = await ctx.CareerRecords
            .Where(cr => cr.EmployeeId == employeeId)
            .OrderByDescending(cr => cr.EventDate)
            .ThenByDescending(cr => cr.Id)
            .FirstOrDefaultAsync();
        return lastRecord != null && lastRecord.EventType != EventType.Dismissal;
    }

    // --- Вспомогательный метод расчёта возраста ---
    private static int CalculateAge(DateTime birthDate, DateTime asOf)
    {
        var age = asOf.Year - birthDate.Year;
        if (asOf < birthDate.AddYears(age)) age--;
        return age;
    }
}
