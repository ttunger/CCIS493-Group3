using Microsoft.EntityFrameworkCore;

namespace HaircutBookingSystem.Data
{
    public class SalonContext : DbContext
    {
        public SalonContext(DbContextOptions<SalonContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Stylist> Stylists => Set<Stylist>();
        public DbSet<AppointmentType> AppointmentTypes => Set<AppointmentType>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<StylistNote> StylistNotes => Set<StylistNote>();
    }
}
