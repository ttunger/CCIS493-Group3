using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HaircutBookingSystem.Models;
using HaircutBookingSystem.Services;      // <-- IEmailSender lives here
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;

namespace HaircutBookingSystem.Controllers
{
    /*
     HOW SMTP PLUGS IN (read this once, then skim the rest)
     ------------------------------------------------------
     1) TODAY (no SMTP): We show a confirmation screen only.
        - appsettings.json has:  "Email": { "Send": false }
        - Controller reads that flag and SKIPS sending.

     2) WHEN YOU GET AN SMTP PROVIDER (Brevo, Mailjet, Gmail Workspace, SES, etc.):
        - Put their SMTP settings under "Smtp" in appsettings.json (Host/Port/SSL/Username/Password/FromAddress/FromName).
        - Flip the switch:  "Email": { "Send": true }
        - Make sure Program.cs registers SmtpEmailSender as IEmailSender (the earlier guide already showed this).
          Example in Program.cs:
              var smtpOptions = builder.Configuration.GetSection("Smtp").Get<SmtpOptions>() ?? new SmtpOptions();
              builder.Services.AddSingleton(smtpOptions);
              builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();

     3) OPTIONAL BUT RECOMMENDED:
        - Add a Reply-To so customers can answer (see comment below near SendAsync).
        - Store secrets safely (User Secrets in dev, environment variables in prod).
        - Add SPF/DKIM/DMARC DNS records at your domain so email avoids spam.

     4) EXAMPLE appsettings.json SNIPPET (you can paste into your file and edit):
        {
          "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
          "AllowedHosts": "*",

          "Email": { "Send": false },  // <-- set to true when ready to send real emails

          "Smtp": {
            "Host": "smtp-relay.brevo.com",   // or smtp.gmail.com, smtp.mailjet.com, email-smtp.us-east-1.amazonaws.com, etc.
            "Port": 587,
            "UseSsl": true,
            "Username": "YOUR_SMTP_USERNAME_OR_apikey",
            "Password": "YOUR_SMTP_PASSWORD_OR_API_KEY",   // store in User Secrets / env var in real life
            "FromAddress": "noreply@yourdomain.com",
            "FromName": "Good Vibes Barbershop"
          }
        }

     5) WHEN YOU HAVE A DATABASE:
        - Create and save a Booking record BEFORE sending the email.
        - Include a confirmation number in the email body (nice touch).
    */

    public class BookingController : Controller
    {
        private readonly IEmailSender _emailSender;   // <-- This will send real email once Email:Send = true + SMTP is configured
        private readonly bool _sendEmails;            // <-- Our simple on/off switch from appsettings.json

        // For now we hard-code a few services. Replace with DB later.
        private static readonly List<Service> _services = new()
        {
            new Service { Id = 1, Name = "Haircut",       Price = 25 },
            new Service { Id = 2, Name = "Hair Styling",  Price = 40 },
            new Service { Id = 3, Name = "Hair Coloring", Price = 80 },
            new Service { Id = 4, Name = "Beard Trim",    Price = 15 }
        };

        public BookingController(IEmailSender emailSender, IConfiguration config)
        {
            _emailSender = emailSender;

            // Reads Email:Send from appsettings.json (false = skip sending, true = send via SMTP)
            // Example: "Email": { "Send": false }
            _sendEmails = config.GetValue<bool>("Email:Send");
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.ServiceList = new SelectList(_services, nameof(Service.Id), nameof(Service.Name));
            return View(new BookingRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(BookingRequest model)
        {
            ViewBag.ServiceList = new SelectList(_services, nameof(Service.Id), nameof(Service.Name));

            if (!ModelState.IsValid)
                return View(model);

            // (When you have a DB) — Create and save a Booking entity here, then continue.
            // Example (later): _db.Bookings.Add(booking); await _db.SaveChangesAsync();

            var chosenService = _services.First(s => s.Id == model.ServiceId);
            var dateText = model.Date?.ToString("dddd, MMM d, yyyy") ?? "(date)";
            var timeText = model.Time?.ToString() ?? "(time)";

            // Email subject + HTML body (safe to reuse for both customer and shop inbox).
            string subject = $"Your {chosenService.Name} booking request";
            string html = $@"
                <h2>Thanks for your request, {System.Net.WebUtility.HtmlEncode(model.FullName)}!</h2>
                <p>We received your booking:</p>
                <ul>
                    <li><strong>Service:</strong> {System.Net.WebUtility.HtmlEncode(chosenService.Name)}</li>
                    <li><strong>Date:</strong> {dateText}</li>
                    <li><strong>Time:</strong> {timeText}</li>
                </ul>
                <p>If you need to change anything, just reply to this email.</p>";

            if (_sendEmails)
            {
                try
                {
                    // REAL EMAIL PATH (runs when Email:Send = true AND Program.cs wires IEmailSender to SmtpEmailSender).
                    // Inside SmtpEmailSender.SendAsync we build the MailMessage:
                    //   - From = SmtpOptions.FromAddress/FromName
                    //   - To = model.Email
                    //   - Subject/Body = provided below
                    //
                    // TIP: Add a Reply-To so customers can respond to a monitored inbox.
                    // In SmtpEmailSender before SendMailAsync(...):
                    //     message.ReplyToList.Add(new MailAddress("appointments@yourdomain.com", "Appointments"));
                    //
                    // TIP: You can also BCC the shop or send a separate notification:
                    //     await _emailSender.SendAsync("appointments@yourdomain.com", $"New booking from {model.FullName}", html);

                    await _emailSender.SendAsync(model.Email, subject, html);
                }
                catch (Exception)
                {
                    // Do not crash the booking flow. Show a friendly note.
                    // Also log the real exception with ILogger<BookingController> in production.
                    TempData["EmailNote"] = "We could not send an email right now, but your request was received.";
                }
            }
            else
            {
                // DEV/temporary mode: tell the user this is screen-only confirmation.
                TempData["EmailNote"] = "Email sending is temporarily disabled; this is a screen confirmation only.";
            }

            // Pass booking details to the success page (simple approach for now).
            TempData["CustomerName"] = model.FullName;
            TempData["ServiceName"] = chosenService.Name;
            TempData["DateText"] = dateText;
            TempData["TimeText"] = timeText;

            return RedirectToAction(nameof(Success));
        }

        public IActionResult Success()
        {
            // Renders Views/Booking/Success.cshtml
            return View();
        }
    }
}