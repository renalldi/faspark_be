using System.ComponentModel.DataAnnotations;

namespace faspark_be.Models
{
    public class Report
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string PlatMotor { get; set; } = string.Empty;

        [Required]
        public string NamaMotor { get; set; } = string.Empty;

        [Required]
        public string Spot { get; set; } = string.Empty;

        public string Deskripsi { get; set; } = string.Empty;
        public string GambarPath { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
