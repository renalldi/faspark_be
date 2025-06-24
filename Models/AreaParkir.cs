using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace faspark_be.Models
{
    [Table("Area_Parkir")]
    public class AreaParkir
    {
        [Key]
        [Column("id_area")]
        public int Id_Area { get; set; }

        [Required]
        [Column("nama_area")]
        public string Nama_Area { get; set; }

        [Required]
        [Column("kapasitas_area")]
        public int Kapasitas_Area { get; set; }

        [Required]
        [Column("status_area")]
        public string Status_Area { get; set; }
        public ICollection<RiwayatParkir> Riwayat_Parkir { get; set; }
    }
}
