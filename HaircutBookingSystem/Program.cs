using HaircutBookingSystem.Services;


var builder = WebApplication.CreateBuilder(args);


// MVC
builder.Services.AddControllersWithViews();


// Bind SMTP settings from configuration
var smtpSection = builder.Configuration.GetSection("Smtp");
var smtpOptions = smtpSection.Get<SmtpOptions>() ?? new SmtpOptions();


// Register email sender with the bound options
builder.Services.AddSingleton(smtpOptions);
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();


var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();


// Default route goes to Booking/Index
app.MapControllerRoute(
name: "default",
pattern: "{controller=Booking}/{action=Index}/{id?}");


app.Run();