using faspark_be.Database;
using faspark_be.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace faspark_be.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecordController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public RecordController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [Authorize(Roles = "petugas")]
        [HttpGet]
        public async Task<IActionResult> GetAllRecords()
        {
            var records = await _context.Records
                .OrderByDescending(r => r.Tanggal_Lapor)
                .Select(record => new
                {
                    record.Id,
                    NamaPelapor = record.Nama_Pelapor,
                    NoHpPelapor = record.No_HP,
                    JenisBarang = record.Jenis_Barang,
                    Area = record.Area_Kehilangan,
                    Deskripsi = record.Description,
                    FotoUrl = $"{Request.Scheme}://{Request.Host}/uploads{record.ImageUrl}",
                    TanggalLapor = record.Tanggal_Lapor
                })
                .ToListAsync();

            return Ok(records);
        }

        [Authorize(Roles = "petugas")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRecordById(int id)
        {
            var record = await _context.Records.FindAsync(id);
            if (record == null)
                return NotFound(new { message = "Laporan tidak ditemukan." });

            return Ok(new
            {
                record.Id,
                NamaPelapor = record.Nama_Pelapor,
                NoHpPelapor = record.No_HP,
                JenisBarang = record.Jenis_Barang,
                Area = record.Area_Kehilangan,
                Deskripsi = record.Description,
                FotoUrl = $"{Request.Scheme}://{Request.Host}/uploads{record.ImageUrl}",
                TanggalLapor = record.Tanggal_Lapor
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateRecord([FromForm] RecordCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.Foto == null || dto.Foto.Length == 0)
                return BadRequest(new { message = "Gambar wajib diunggah." });

            try
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string fileName = Guid.NewGuid() + Path.GetExtension(dto.Foto.FileName);
                string fullPath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(fullPath, FileMode.Create))
                {
                    await dto.Foto.CopyToAsync(fileStream);
                }

                var newRecord = new Record
                {
                    Nama_Pelapor = dto.Nama_Pelapor,
                    No_HP = dto.No_HP,
                    Jenis_Barang = dto.Jenis_Barang,
                    Area_Kehilangan = dto.Area_Kehilangan,
                    Description = dto.Description,
                    ImageUrl = "/" + fileName,
                    Tanggal_Lapor = DateTime.UtcNow
                };

                _context.Records.Add(newRecord);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Laporan berhasil dikirim.",
                    id = newRecord.Id,
                    fotoUrl = $"{Request.Scheme}://{Request.Host}/uploads{newRecord.ImageUrl}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Terjadi kesalahan saat menyimpan laporan.",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        [Authorize(Roles = "petugas")]
        [HttpPut("{id}")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> UpdateRecord(int id, [FromForm] RecordUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var record = await _context.Records.FindAsync(id);
            if (record == null)
                return NotFound(new { message = "Laporan tidak ditemukan." });

            try
            {
                // Jika ada foto baru, proses upload dan hapus file lama
                if (dto.Foto != null && dto.Foto.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    // Hapus file lama
                    if (!string.IsNullOrEmpty(record.ImageUrl))
                    {
                        var oldFilePath = Path.Combine(_env.WebRootPath, "uploads", record.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                            System.IO.File.Delete(oldFilePath);
                    }

                    string fileName = Guid.NewGuid() + Path.GetExtension(dto.Foto.FileName);
                    string fullPath = Path.Combine(uploadsFolder, fileName);

                    using (var fileStream = new FileStream(fullPath, FileMode.Create))
                    {
                        await dto.Foto.CopyToAsync(fileStream);
                    }

                    record.ImageUrl = "/" + fileName;
                }
                else if (dto.Foto != null && dto.Foto.Length == 0)
                {
                    return BadRequest(new { message = "Gambar tidak boleh kosong jika diunggah." });
                }

                record.Nama_Pelapor = dto.Nama_Pelapor;
                record.No_HP = dto.No_HP;
                record.Jenis_Barang = dto.Jenis_Barang;
                record.Area_Kehilangan = dto.Area_Kehilangan;
                record.Description = dto.Description;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Laporan berhasil diperbarui." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Terjadi kesalahan saat memperbarui laporan.",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        [Authorize(Roles = "petugas")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRecord(int id)
        {
            var record = await _context.Records.FindAsync(id);
            if (record == null)
                return NotFound(new { message = "Data tidak ditemukan." });

            // Hapus file gambar jika ada
            if (!string.IsNullOrEmpty(record.ImageUrl))
            {
                var filePath = Path.Combine(_env.WebRootPath, "uploads", record.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Records.Remove(record);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Data berhasil dihapus." });
        }
    }

    public class RecordCreateDto
    {
        [Required]
        public string Nama_Pelapor { get; set; }

        [Required]
        public string No_HP { get; set; }

        [Required]
        public string Jenis_Barang { get; set; }

        public string? Description { get; set; }

        [Required]
        public string Area_Kehilangan { get; set; }

        [Required]
        public IFormFile Foto { get; set; }
    }

    public class RecordUpdateDto
    {
        [Required]
        public string Nama_Pelapor { get; set; }

        [Required]
        public string No_HP { get; set; }

        [Required]
        public string Jenis_Barang { get; set; }

        public string? Description { get; set; }

        [Required]
        public string Area_Kehilangan { get; set; }

        // Foto opsional, boleh null kalau tidak ingin ganti gambar
        public IFormFile? Foto { get; set; }
    }
}
