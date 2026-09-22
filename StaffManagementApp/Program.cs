using Microsoft.EntityFrameworkCore;
using StaffManagementApp.Components;
using StaffManagementApp.Data;
using StaffManagementApp.Services;

var builder = WebApplication.CreateBuilder(args);

// добавление сервисов в контейнер
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ef core
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// сервисы приложения
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ICareerService, CareerService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();

var app = builder.Build();

// Миграция базы данных
using (var scope = app.Services.CreateScope())
{
    var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    await using var context = await contextFactory.CreateDbContextAsync();
    await DbInitializer.SeedAsync(context);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

// UseStaticFiles — для динамически загруженных файлов (uploads/avatars),
// которые не входят в манифест MapStaticAssets (формируется при компиляции).
app.UseStaticFiles();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
