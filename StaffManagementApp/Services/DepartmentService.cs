using Microsoft.EntityFrameworkCore;
using StaffManagementApp.Data;
using StaffManagementApp.Data.Entities;

namespace StaffManagementApp.Services;

public class DepartmentService(IDbContextFactory<AppDbContext> factory) : IDepartmentService
{
    public async Task<List<Department>> GetAllAsync()
    {
        await using var ctx = await factory.CreateDbContextAsync();
        return await ctx.Departments
            .OrderBy(d => d.Code)
            .ToListAsync();
    }

    public async Task<List<Department>> GetActiveAsync()
    {
        await using var ctx = await factory.CreateDbContextAsync();
        return await ctx.Departments
            .Where(d => d.ClosedAt == null)
            .OrderBy(d => d.Code)
            .ToListAsync();
    }

    public async Task<List<DepartmentTreeNode>> GetTreeAsync()
    {
        await using var ctx = await factory.CreateDbContextAsync();
        var all = await ctx.Departments.OrderBy(d => d.Code).ToListAsync();
        return BuildTree(all, null);
    }

    private static List<DepartmentTreeNode> BuildTree(List<Department> all, int? parentId)
    {
        return all
            .Where(d => d.ParentDepartmentId == parentId)
            .Select(d => new DepartmentTreeNode
            {
                Department = d,
                Children = BuildTree(all, d.Id)
            })
            .ToList();
    }

    public async Task<Department?> GetByIdAsync(int id)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        return await ctx.Departments.FindAsync(id);
    }

    public async Task<Department> CreateAsync(Department department)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        ctx.Departments.Add(department);
        await ctx.SaveChangesAsync();
        return department;
    }

    public async Task<Department> UpdateAsync(Department department)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        var existing = await ctx.Departments.FindAsync(department.Id)
            ?? throw new InvalidOperationException("Department not found");
        existing.Code = department.Code;
        existing.Name = department.Name;
        existing.Abbreviation = department.Abbreviation;
        existing.ParentDepartmentId = department.ParentDepartmentId;
        await ctx.SaveChangesAsync();
        return existing;
    }

    public async Task LiquidateAsync(int id)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        var dept = await ctx.Departments.FindAsync(id)
            ?? throw new InvalidOperationException("Department not found");

        if (dept.ClosedAt != null)
            throw new InvalidOperationException("Подразделение уже ликвидировано.");

        // Check for active child departments
        var hasActiveChildren = await ctx.Departments
            .AnyAsync(d => d.ParentDepartmentId == id && d.ClosedAt == null);
        if (hasActiveChildren)
            throw new InvalidOperationException("Невозможно ликвидировать подразделение: существуют активные дочерние подразделения.");

        // Check for active employees (employees without a Dismissal record as last event)
        var activeEmployees = await ctx.CareerRecords
            .Where(cr => cr.DepartmentId == id && cr.EndDate == null && cr.EventType != EventType.Dismissal)
            .AnyAsync();
        if (activeEmployees)
            throw new InvalidOperationException("Невозможно ликвидировать подразделение: в нём числятся активные сотрудники.");

        dept.ClosedAt = DateTime.UtcNow;
        await ctx.SaveChangesAsync();
    }

    public async Task<bool> IsCodeUniqueAsync(string code, int? excludeDepartmentId = null)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        var query = ctx.Departments.Where(d => d.Code == code);
        if (excludeDepartmentId.HasValue)
            query = query.Where(d => d.Id != excludeDepartmentId.Value);
        return !await query.AnyAsync();
    }

    public async Task RestoreAsync(int id)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        var dept = await ctx.Departments
            .Include(d => d.ParentDepartment)
            .FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new InvalidOperationException("Подразделение не найдено.");

        if (dept.ClosedAt == null)
            throw new InvalidOperationException("Подразделение не ликвидировано.");

        // Если есть родитель — он должен быть активен
        if (dept.ParentDepartment != null && dept.ParentDepartment.ClosedAt != null)
            throw new InvalidOperationException(
                $"Невозможно восстановить подразделение: родительское подразделение «{dept.ParentDepartment.Name}» также ликвидировано. Сначала восстановите его.");

        dept.ClosedAt = null;
        await ctx.SaveChangesAsync();
    }

    public async Task HardDeleteAsync(int id)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        var dept = await ctx.Departments.FindAsync(id)
            ?? throw new InvalidOperationException("Подразделение не найдено.");

        // Проверяем, были ли когда-либо сотрудники
        var hasCareerRecords = await ctx.CareerRecords.AnyAsync(cr => cr.DepartmentId == id);
        if (hasCareerRecords)
            throw new InvalidOperationException(
                "Невозможно удалить подразделение: в нём есть или были сотрудники. Доступна только ликвидация.");

        // Проверяем дочерние подразделения
        var hasChildren = await ctx.Departments.AnyAsync(d => d.ParentDepartmentId == id);
        if (hasChildren)
            throw new InvalidOperationException(
                "Невозможно удалить подразделение: у него есть дочерние подразделения.");

        ctx.Departments.Remove(dept);
        await ctx.SaveChangesAsync();
    }
}
