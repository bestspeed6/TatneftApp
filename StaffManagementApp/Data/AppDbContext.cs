using Microsoft.EntityFrameworkCore;
using StaffManagementApp.Data.Entities;

namespace StaffManagementApp.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<CareerRecord> CareerRecords => Set<CareerRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Department
        modelBuilder.Entity<Department>(e =>
        {
            e.HasKey(d => d.Id);
            e.Property(d => d.Code).HasMaxLength(4).IsRequired();
            e.HasIndex(d => d.Code).IsUnique();
            e.Property(d => d.Name).HasMaxLength(200).IsRequired();
            e.Property(d => d.Abbreviation).HasMaxLength(50).IsRequired();
            e.Property(d => d.CreatedAt).HasDefaultValueSql("NOW()");

            e.HasOne(d => d.ParentDepartment)
                .WithMany(d => d.ChildrenDepartments)
                .HasForeignKey(d => d.ParentDepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Employee
        modelBuilder.Entity<Employee>(e =>
        {
            e.HasKey(emp => emp.Id);
            e.Property(emp => emp.PersonnelNumber).HasMaxLength(20).IsRequired();
            e.HasIndex(emp => emp.PersonnelNumber).IsUnique();
            e.Property(emp => emp.LastName).HasMaxLength(100).IsRequired();
            e.Property(emp => emp.FirstName).HasMaxLength(100).IsRequired();
            e.Property(emp => emp.MiddleName).HasMaxLength(100);
            e.Property(emp => emp.Gender).HasConversion<string>();
            e.Property(emp => emp.Phone).HasMaxLength(30);
            e.Property(emp => emp.Email).HasMaxLength(150);
            e.Property(emp => emp.PhotoPath).HasMaxLength(500);
        });

        // CareerRecord
        modelBuilder.Entity<CareerRecord>(e =>
        {
            e.HasKey(cr => cr.Id);
            e.Property(cr => cr.Position).HasMaxLength(200).IsRequired();
            e.Property(cr => cr.EventType).HasConversion<string>();
            e.Property(cr => cr.Comment).HasMaxLength(500);

            e.HasOne(cr => cr.Employee)
                .WithMany(emp => emp.CareerRecords)
                .HasForeignKey(cr => cr.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(cr => cr.Department)
                .WithMany(d => d.CareerRecords)
                .HasForeignKey(cr => cr.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
