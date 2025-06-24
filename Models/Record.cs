using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace faspark_be.Models
{
    public class Record
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nama_Pelapor { get; set; }

        [Required]
        public string No_HP { get; set; }

        [Required]
        public string Jenis_Barang { get; set; }

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public DateTime? Tanggal_Lapor { get; set; }

        [Required]
        public string Area_Kehilangan { get; set; }

        [NotMapped]
        public IFormFile Foto { get; set; }
    }
}
