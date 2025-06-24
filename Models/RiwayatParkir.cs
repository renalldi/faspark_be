using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace faspark_be.Models
{
    [Table("Riwayat_Parkir")]
    public class RiwayatParkir
    {
        [Key]
        [Column("id_riwayat")]
        public int Id_Riwayat { get; set; }

        [Required]
        [Column("id_user")]
        public int Id_User { get; set; }

        [Required]
        [Column("id_area")]
        public int Id_Area { get; set; }

        [Required]
        [Column("waktu_masuk")]
        public DateTime Waktu_Masuk { get; set; }

        [Column("waktu_keluar")]
        public DateTime? Waktu_Keluar { get; set; }

        [Required]
        [Column("status_riwayat")]
        public string Status_Riwayat { get; set; }

        // Tambahkan relasi ke Area
        [ForeignKey("Id_Area")]
        public AreaParkir Area_Parkir { get; set; }

        // relasi ke User
        [ForeignKey("Id_User")]
        public User User { get; set; }
    }
}
