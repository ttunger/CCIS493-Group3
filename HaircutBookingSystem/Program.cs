using HaircutBookingSystem.Models;
using Microsoft.AspNetCore.Identity;
using HaircutBookingSystem.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connection = $"Server=(localdb)\\mssqllocaldb;Database=HaircutBookingDb;Trusted_Connection=True;";
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connection));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


// Register services (must happen before Build)
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Bind SMTP settings from configuration
var smtpSection = builder.Configuration.GetSection("Smtp");
var smtpOptions = smtpSection.Get<SmtpOptions>() ?? new SmtpOptions();
builder.Services.AddSingleton(smtpOptions);
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure middleware (after Build)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication(); // if using Identity
app.UseAuthorization();

// Default route goes to Homepage
app.UseSession();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();