using System;
using System.Linq;
using System.Threading.Tasks;
using HaircutBookingSystem.Models;
using HaircutBookingSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

static async Task EnsureRootAdminAsync(IServiceProvider services, IConfiguration config)
{
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    var enabled = config.GetValue<bool>("RootAdmin:Enabled");
    var email = config["RootAdmin:Email"];
    var password = config["RootAdmin:Password"];
    const string adminRole = "Admin";

    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        return;

    // Ensure Admin role exists
    if (!await roleManager.RoleExistsAsync(adminRole))
        await roleManager.CreateAsync(new IdentityRole(adminRole));

    // Try by username first (UserName == Email in this app), then by email
    var user = await userManager.FindByNameAsync(email)
               ?? await userManager.FindByEmailAsync(email);

    if (enabled)
    {
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };
            var create = await userManager.CreateAsync(user, password);
            if (!create.Succeeded)
            {
                var msg = string.Join("; ", create.Errors.Select(e => $"{e.Code}:{e.Description}"));
                throw new Exception($"Root admin create failed: {msg}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, adminRole))
            await userManager.AddToRoleAsync(user, adminRole);

        await userManager.SetLockoutEnabledAsync(user, false);
        await userManager.SetLockoutEndDateAsync(user, null);
    }
    else
    {
        if (user != null)
        {
            await userManager.SetLockoutEnabledAsync(user, true);
            await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            // Optional: also remove Admin role while disabled
            // if (await userManager.IsInRoleAsync(user, adminRole))
            //     await userManager.RemoveFromRoleAsync(user, adminRole);
        }
    }
}

var builder = WebApplication.CreateBuilder(args);

// ---------- Database ----------
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\mssqllocaldb;Database=HaircutBookingDb;Trusted_Connection=True;";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// ---------- Identity (cookie auth under the hood) ----------
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";          // your controller login
    options.LogoutPath = "/Account/Logout";        // your controller logout
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
});

// ---------- MVC / Razor Pages / Session ----------
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // Identity pages if you use any
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ---------- (Optional) SMTP wiring ----------
var smtpOptions = builder.Configuration.GetSection("Smtp").Get<SmtpOptions>() ?? new SmtpOptions();
builder.Services.AddSingleton(smtpOptions);
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();

var app = builder.Build();

// ---------- Middleware ----------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();   // before Authorization
app.UseAuthorization();

app.UseSession();

// ---------- Endpoints ----------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // fine to keep even with controller-based auth screens

// ---------- Apply migrations (optional) + Seed/Toggle Root Admin ----------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // Optional: apply EF migrations at startup (comment out if you prefer manual)
    try
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
    }
    catch
    {
        // swallow or log if you don't want startup to fail on migration errors
        // var logger = services.GetRequiredService<ILogger<Program>>();
        // logger.LogError(ex, "Database migration failed");
    }

    await EnsureRootAdminAsync(services, app.Configuration);
}

app.Run();
