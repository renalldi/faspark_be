using faspark_be.Database;
using faspark_be.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace faspark_be.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportController(AppDbContext context)
        {
            _context = context;
        }

        public class ReportInputModel
        {
            public string PlatMotor { get; set; } = "";
            public string NamaMotor { get; set; } = "";
            public string Spot { get; set; } = "";
            public string Deskripsi { get; set; } = "";
            public IFormFile? Gambar { get; set; }
        }

        [HttpPost]
        [Authorize(Roles = "petugas")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> CreateReport([FromForm] ReportInputModel input)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                string? fileName = null;

                if (input.Gambar != null && input.Gambar.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    var ext = Path.GetExtension(input.Gambar.FileName).ToLower();

                    if (!allowedExtensions.Contains(ext))
                    {
                        return BadRequest(new { message = "Format file tidak didukung. Gunakan .jpg, .jpeg, atau .png." });
                    }

                    fileName = $"{Guid.NewGuid()}{ext}";
                    var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    var filePath = Path.Combine(uploadsDir, fileName);
                    Directory.CreateDirectory(uploadsDir);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await input.Gambar.CopyToAsync(stream);
                    }
                }
                else if (input.Gambar != null && input.Gambar.Length == 0)
                {
                    return BadRequest(new { message = "Gambar tidak boleh kosong." });
                }

                var report = new Report
                {
                    PlatMotor = input.PlatMotor,
                    NamaMotor = input.NamaMotor,
                    Spot = input.Spot,
                    Deskripsi = input.Deskripsi,
                    GambarPath = fileName ?? ""
                };

                _context.Reports.Add(report);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Laporan berhasil dikirim.",
                    id = report.Id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Terjadi kesalahan saat mengirim laporan.", error = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "petugas")]
        public IActionResult GetReports()
        {
            return Ok(_context.Reports.ToList());
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "petugas")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> UpdateReport(int id, [FromForm] ReportInputModel input)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var report = await _context.Reports.FindAsync(id);
            if (report == null)
            {
                return NotFound(new { message = "Laporan tidak ditemukan." });
            }

            try
            {
                if (input.Gambar != null && input.Gambar.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    var ext = Path.GetExtension(input.Gambar.FileName).ToLower();

                    if (!allowedExtensions.Contains(ext))
                    {
                        return BadRequest(new { message = "Format file tidak didukung. Gunakan .jpg, .jpeg, atau .png." });
                    }

                    // Hapus file lama jika ada
                    if (!string.IsNullOrEmpty(report.GambarPath))
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", report.GambarPath);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    var filePath = Path.Combine(uploadsDir, fileName);
                    Directory.CreateDirectory(uploadsDir);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await input.Gambar.CopyToAsync(stream);
                    }

                    report.GambarPath = fileName;
                }
                else if (input.Gambar != null && input.Gambar.Length == 0)
                {
                    return BadRequest(new { message = "Gambar tidak boleh kosong." });
                }

                report.PlatMotor = input.PlatMotor;
                report.NamaMotor = input.NamaMotor;
                report.Spot = input.Spot;
                report.Deskripsi = input.Deskripsi;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Laporan berhasil diperbarui." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Terjadi kesalahan saat memperbarui laporan.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "petugas")]
        public async Task<IActionResult> DeleteReport(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
            {
                return NotFound(new { message = "Laporan tidak ditemukan." });
            }

            try
            {
                // Hapus file gambar jika ada
                if (!string.IsNullOrEmpty(report.GambarPath))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", report.GambarPath);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Reports.Remove(report);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Laporan berhasil dihapus." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Terjadi kesalahan saat menghapus laporan.", error = ex.Message });
            }
        }
    }
}
