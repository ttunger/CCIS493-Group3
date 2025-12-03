using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HaircutBookingSystem.Models
{
    public class BookingRequest : IValidatableObject
    {
        [Required(ErrorMessage = "Please choose a service.")]
        [Display(Name = "Service")]
        [Range(1, int.MaxValue, ErrorMessage = "Please choose a valid service.")]
        public int ServiceId { get; set; }

        // double check for "stylistid"
        [Required(ErrorMessage = "Please choose a stylist.")]
        [Display(Name = "Preferred Stylist")]
        [Range(1, int.MaxValue, ErrorMessage = "Please choose a valid stylist.")]
        public int StylistId { get; set; }

        [Required(ErrorMessage = "Please choose a date.")]
        [DataType(DataType.Date)]
        [Display(Name = "Preferred Date")]
        public DateTime? Date { get; set; }

        [Required(ErrorMessage = "Please choose a time.")]
        [DataType(DataType.Time)]
        [Display(Name = "Preferred Time")]
        public TimeSpan? Time { get; set; }

        [Required(ErrorMessage = "Please enter your full name.")]
        [StringLength(60, MinimumLength = 2,
            ErrorMessage = "Full name must be between 2 and 60 characters.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [Display(Name = "Phone (optional)")]
        public string? Phone { get; set; }

        
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            
            if (Date.HasValue && Time.HasValue)
            {
                var bookingDateTime = Date.Value.Date + Time.Value;

                
                if (bookingDateTime <= DateTime.Now)
                {
                    yield return new ValidationResult(
                        "The selected date and time must be in the future.",
                        new[] { nameof(Date), nameof(Time) });
                }

                
                var opening = new TimeSpan(9, 0, 0);  
                var closing = new TimeSpan(17, 0, 0); 

                if (Time.Value < opening || Time.Value > closing)
                {
                    yield return new ValidationResult(
                        "Please choose a time during opening hours (9:00 AM – 5:00 PM).",
                        new[] { nameof(Time) });
                }
            }
        }
    }
}
