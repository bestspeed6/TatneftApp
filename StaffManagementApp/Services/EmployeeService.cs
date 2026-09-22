using Microsoft.EntityFrameworkCore;
using StaffManagementApp.Data;
using StaffManagementApp.Data.Entities;

namespace StaffManagementApp.Services;

public class EmployeeService(IDbContextFactory<AppDbContext> factory) : IEmployeeService
{
    public async Task<List<EmployeeWithStatus>> GetAllWithStatusAsync(string? search = null)
    {
        await using var ctx = await factory.CreateDbContextAsync();

        var query = ctx.Employees
            .Include(e => e.CareerRecords)
                .ThenInclude(cr => cr.Department)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(e =>
                e.LastName.ToLower().Contains(s) ||
                e.FirstName.ToLower().Contains(s) ||
                (e.MiddleName != null && e.MiddleName.ToLower().Contains(s)) ||
                e.PersonnelNumber.ToLower().Contains(s));
        }

        var employees = await query.OrderBy(e => e.LastName).ThenBy(e => e.FirstName).ToListAsync();

        return employees.Select(emp =>
        {
            var lastRecord = emp.CareerRecords
                .OrderByDescending(cr => cr.EventDate)
                .ThenByDescending(cr => cr.Id)   // при одинаковой дате — берём запись с большим Id (создана позже)
                .FirstOrDefault();
            var isDismissed = lastRecord?.EventType == EventType.Dismissal;
            var currentActive = emp.CareerRecords
                .Where(cr => cr.EventType != EventType.Dismissal && cr.EndDate == null)
                .OrderByDescending(cr => cr.EventDate)
                .ThenByDescending(cr => cr.Id)
                .FirstOrDefault();

            return new EmployeeWithStatus
            {
                Employee = emp,
                CurrentRecord = isDismissed ? lastRecord : currentActive,
                IsActive = !isDismissed
            };
        }).ToList();
    }

    public async Task<Employee?> GetByIdAsync(int id)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        return await ctx.Employees
            .Include(e => e.CareerRecords)
                .ThenInclude(cr => cr.Department)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Employee> CreateAsync(Employee employee)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        ctx.Employees.Add(employee);
        await ctx.SaveChangesAsync();
        return employee;
    }

    public async Task<Employee> UpdateAsync(Employee employee)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        var existing = await ctx.Employees.FindAsync(employee.Id)
            ?? throw new InvalidOperationException("Employee not found");
        existing.PersonnelNumber = employee.PersonnelNumber;
        existing.LastName = employee.LastName;
        existing.FirstName = employee.FirstName;
        existing.MiddleName = employee.MiddleName;
        existing.Gender = employee.Gender;
        existing.BirthDate = employee.BirthDate;
        existing.Phone = employee.Phone;
        existing.Email = employee.Email;
        if (employee.PhotoPath != null) existing.PhotoPath = employee.PhotoPath;
        await ctx.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> IsPersonnelNumberUniqueAsync(string personnelNumber, int? excludeEmployeeId = null)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        var query = ctx.Employees.Where(e => e.PersonnelNumber == personnelNumber);
        if (excludeEmployeeId.HasValue)
            query = query.Where(e => e.Id != excludeEmployeeId.Value);
        return !await query.AnyAsync();
    }
}
