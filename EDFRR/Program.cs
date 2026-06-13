using EDFRR.Data;
using EDFRR.Models.Entities;
using EDFRR.Repositories.Interfaces;
using EDFRR.Repositories.Implementations;
using EDFRR.Services.Interfaces;
using EDFRR.Services.Implementations;
using EDFRR.Scheduling.Engine;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IProcessRepository, ProcessRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<IResultRepository, ResultRepository>();
builder.Services.AddScoped<IExecutionLogRepository, ExecutionLogRepository>();
builder.Services.AddScoped<IComparisonRepository, ComparisonRepository>();

builder.Services.AddScoped<IProcessService, ProcessService>();
builder.Services.AddScoped<IProcessImportService, ProcessImportService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<ISimulationService, SimulationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IComparisonService, ComparisonService>();

builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
builder.Services.AddScoped<IActivityLogService, ActivityLogService>();

builder.Services.AddSingleton<SchedulingEngine>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "Admin/{controller=AdminDashboard}/{action=Index}/{id?}",
    defaults: new { area = "Admin" });
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();
    var logger = services.GetRequiredService<ILogger<Program>>();
    db.Database.Migrate();
    await DataSeeder.SeedAsync(services);

    try
    {
        var sqlPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Sql", "StoredProcedures.sql");
        if (File.Exists(sqlPath))
        {
            var sql = await File.ReadAllTextAsync(sqlPath);
            var batches = System.Text.RegularExpressions.Regex.Split(sql, @"^\s*GO\s*$", System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            foreach (var batch in batches)
            {
                var trimmed = batch.Trim();
                if (trimmed.Length > 0)
                {
                    await db.Database.ExecuteSqlRawAsync(trimmed);
                }
            }
            logger.LogInformation("Stored procedures deployed from {Path}", sqlPath);
        }
        else
        {
            logger.LogWarning("Stored procedures file not found at {Path}", sqlPath);
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Could not deploy stored procedures (they may already exist)");
    }
}

app.Run();
