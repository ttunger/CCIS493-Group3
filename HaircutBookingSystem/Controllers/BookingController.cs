using System;
using System.Linq;
using System.Threading.Tasks;
using HaircutBookingSystem.Data;
using HaircutBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HaircutBookingSystem.Controllers
{
    public class BookingController : Controller
    {
        private readonly SalonContext _db;
        private readonly TimeZoneInfo _localTz =
            TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"); // adjust if needed

        public BookingController(SalonContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await LoadDropdowns();
            return View(new BookingRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(BookingRequest model)
        {
            await LoadDropdowns(); // repopulate on postback

            if (!ModelState.IsValid) return View(model);

            // 30-minute boundary enforcement (e.g., 9:00, 9:30, 10:00)
            var mins = model.Time!.Value.Minutes;
            var secs = model.Time.Value.Seconds;
            if (secs != 0 || (mins % 30) != 0)
            {
                ModelState.AddModelError(nameof(model.Time),
                    "Please choose a time in 30-minute steps (e.g., 9:00, 9:30, 10:00).");
                return View(model);
            }

            // Validate service
            var type = await _db.AppointmentTypes
                                .FirstOrDefaultAsync(t => t.TypeID == model.TypeID && t.IsActive);
            if (type == null)
            {
                ModelState.AddModelError(nameof(model.TypeID), "Please pick a valid service.");
                return View(model);
            }

            // Validate stylist
            var stylist = await _db.Stylists
                                   .Include(s => s.User)
                                   .FirstOrDefaultAsync(s => s.StylistID == model.StylistID && s.IsActive);
            if (stylist == null)
            {
                ModelState.AddModelError(nameof(model.StylistID), "Please pick a valid stylist.");
                return View(model);
            }

            // Build local → UTC window
            var localDateTime = model.Date!.Value.Date + model.Time.Value;
            var startUtc = TimeZoneInfo.ConvertTimeToUtc(localDateTime, _localTz);
            var endUtc = startUtc.AddMinutes(type.DurationMinutes);

            // Find or create client (no auth yet)
            var client = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (client == null)
            {
                client = new User
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Phone = model.Phone,
                    Role = "Client",
                    PasswordHash = "BOOKING_PORTAL_PLACEHOLDER",
                    CreatedAtUtc = DateTime.UtcNow,
                    IsActive = true
                };
                _db.Users.Add(client);
                await _db.SaveChangesAsync();
            }

            // Overlap check for the SELECTED stylist ONLY
            bool conflict = await _db.Appointments.AnyAsync(a =>
                a.StylistID == stylist.StylistID &&
                a.Status != "Cancelled" &&
                a.Status != "NoShow" &&
                a.StartDateTimeUtc < endUtc &&
                (a.EndDateTimeUtc ?? a.StartDateTimeUtc) > startUtc
            );

            if (conflict)
            {
                TempData["ServiceName"] = type.Name;
                TempData["StylistName"] = stylist.User?.FullName ?? $"Stylist #{stylist.StylistID}";
                var localStart = TimeZoneInfo.ConvertTimeFromUtc(startUtc, _localTz);
                TempData["DateText"] = localStart.ToString("dddd, MMM d, yyyy");
                TempData["TimeText"] = localStart.ToString("h:mm tt");
                return RedirectToAction(nameof(Unavailable));
            }

            // Create appointment
            var appt = new Appointment
            {
                ClientUserID = client.UserID,
                StylistID = stylist.StylistID,
                TypeID = type.TypeID,
                StartDateTimeUtc = startUtc,
                EndDateTimeUtc = endUtc,
                Status = "Confirmed",
                ClientNote = model.Note,
                CreatedAtUtc = DateTime.UtcNow
            };
            _db.Appointments.Add(appt);
            await _db.SaveChangesAsync();

            // Confirmation screen
            var local = TimeZoneInfo.ConvertTimeFromUtc(startUtc, _localTz);
            TempData["CustomerName"] = client.FullName;
            TempData["ServiceName"] = type.Name;
            TempData["StylistName"] = stylist.User?.FullName ?? $"Stylist #{stylist.StylistID}";
            TempData["DateText"] = local.ToString("dddd, MMM d, yyyy");
            TempData["TimeText"] = local.ToString("h:mm tt");
            TempData["ConfirmId"] = $"A{appt.AppointmentID:D6}";

            return RedirectToAction(nameof(Success));
        }

        public IActionResult Success() => View();
        public IActionResult Unavailable() => View();

        private async Task LoadDropdowns()
        {
            var types = await _db.AppointmentTypes
                                 .Where(t => t.IsActive)
                                 .OrderBy(t => t.Name)
                                 .ToListAsync();
            ViewBag.ServiceList = new SelectList(types, "TypeID", "Name");

            // Show stylist display names (pull from linked Users)
            var stylists = await _db.Stylists
                                    .Include(s => s.User)
                                    .Where(s => s.IsActive)
                                    .OrderBy(s => s.User!.FullName)
                                    .Select(s => new { s.StylistID, Name = s.User!.FullName })
                                    .ToListAsync();
            ViewBag.StylistList = new SelectList(stylists, "StylistID", "Name");
        }
    }
}
