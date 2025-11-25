using System.ComponentModel.DataAnnotations;

namespace HaircutBookingSystem.Models
{
    public class BookingRequest
    {
        [Required, Display(Name = "Service")]
        public int TypeID { get; set; }

        [Required, Display(Name = "Stylist")]
        public int StylistID { get; set; }

        [Required, DataType(DataType.Date), Display(Name = "Preferred Date")]
        public DateTime? Date { get; set; }

        [Required, DataType(DataType.Time), Display(Name = "Preferred Time")]
        public TimeSpan? Time { get; set; }

        [Required, StringLength(60), Display(Name = "Full Name")]
        public string FullName { get; set; } = "";

        [Required, EmailAddress, Display(Name = "Email Address")]
        public string Email { get; set; } = "";

        [Phone, Display(Name = "Phone (optional)")]
        public string? Phone { get; set; }

        [StringLength(500), Display(Name = "Notes (optional)")]
        public string? Note { get; set; }
    }
}
