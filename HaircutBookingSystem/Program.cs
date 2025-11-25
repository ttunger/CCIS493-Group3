using HaircutBookingSystem.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// EF Core
builder.Services.AddDbContext<SalonContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Default route → Booking/Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Booking}/{action=Index}/{id?}");

app.Run();
