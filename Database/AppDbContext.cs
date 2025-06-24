using faspark_be.Models;
using Microsoft.EntityFrameworkCore;

namespace faspark_be.Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Record> Records { get; set; }
        public DbSet<RiwayatParkir> Riwayat_Parkir { get; set; }
        public DbSet<AreaParkir> Area_Parkir { get; set; }

    }
}
