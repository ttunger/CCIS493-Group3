using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaircutBookingSystem.Data
{
    [Table("Users")]
    public class User
    {
        [Key] public int UserID { get; set; }
        [Required, MaxLength(100)] public string FullName { get; set; } = "";
        [Required, MaxLength(255)] public string Email { get; set; } = "";
        [Required, MaxLength(255)] public string PasswordHash { get; set; } = "BOOKING_PORTAL_PLACEHOLDER";
        [Required, MaxLength(20)] public string Role { get; set; } = "Client";
        [MaxLength(30)] public string? Phone { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
    }

    [Table("Stylists")]
    public class Stylist
    {
        [Key] public int StylistID { get; set; }
        public int UserID { get; set; }
        public string? Bio { get; set; }
        public string? Specialty { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UserID))] public User? User { get; set; }
        public ICollection<Appointment>? Appointments { get; set; }
    }

    [Table("AppointmentTypes")]
    public class AppointmentType
    {
        [Key] public int TypeID { get; set; }
        [Required, MaxLength(100)] public string Name { get; set; } = "";
        [MaxLength(300)] public string? Description { get; set; }
        [Required] public int DurationMinutes { get; set; }
        [Required] public decimal Price { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }

    [Table("Appointments")]
    public class Appointment
    {
        [Key] public int AppointmentID { get; set; }
        [Required] public int ClientUserID { get; set; }
        [Required] public int StylistID { get; set; }
        [Required] public int TypeID { get; set; }
        [Required] public DateTime StartDateTimeUtc { get; set; }
        public DateTime? EndDateTimeUtc { get; set; }
        [Required, MaxLength(20)] public string Status { get; set; } = "Pending";
        [MaxLength(500)] public string? ClientNote { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(ClientUserID))] public User? Client { get; set; }
        [ForeignKey(nameof(StylistID))] public Stylist? Stylist { get; set; }
        [ForeignKey(nameof(TypeID))] public AppointmentType? Type { get; set; }
    }

    [Table("StylistNotes")]
    public class StylistNote
    {
        [Key] public int NoteID { get; set; }
        public int AppointmentID { get; set; }
        public int StylistID { get; set; }
        [Required] public string NotesText { get; set; } = "";
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(AppointmentID))] public Appointment? Appointment { get; set; }
        [ForeignKey(nameof(StylistID))] public Stylist? Stylist { get; set; }
    }
}
