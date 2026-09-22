using Microsoft.EntityFrameworkCore;

namespace StaffManagementApp.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Применяем миграции для создания структуры БД
        await context.Database.MigrateAsync();
    }
}