using faspark_be.Database;
using faspark_be.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace faspark_be.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParkirController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ParkirController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/parkir/aktif
        [HttpGet("aktif")]
        [Authorize(Roles = "petugas,mahasiswa,dosen")]
        public async Task<IActionResult> GetParkirAktif()
        {
            var data = await _context.Riwayat_Parkir
                .Where(r => r.Status_Riwayat == "Parkir" && r.Waktu_Keluar == null)
                .Join(_context.Users,
                      riwayat => riwayat.Id_User,
                      user => user.Id,
                      (riwayat, user) => new
                      {
                          riwayat.Id_Riwayat,
                          Username = user.Username,
                          riwayat.Id_Area,
                          riwayat.Waktu_Masuk
                      })
                .Join(_context.Area_Parkir,
                      joined => joined.Id_Area,
                      area => area.Id_Area,
                      (joined, area) => new
                      {
                          joined.Id_Riwayat,
                          joined.Username,
                          NamaArea = area.Nama_Area,
                          joined.Waktu_Masuk
                      })
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("aktif-by-user/{id}")]
        [Authorize]
        public IActionResult GetParkirAktifByUser(int id)
        {
            var riwayat = _context.Riwayat_Parkir
                .Include(r => r.Area_Parkir)
                .FirstOrDefault(r =>
                    r.Id_User == id &&
                    r.Status_Riwayat == "Parkir" &&
                    r.Waktu_Keluar == null);

            if (riwayat == null)
                return Ok(new { isParked = false });

            return Ok(new
            {
                isParked = true,
                riwayat.Id_Riwayat,
                riwayat.Id_Area,
                riwayat.Area_Parkir.Nama_Area,
                riwayat.Waktu_Masuk
            });
        }

        // GET: api/parkir/area-status
        [HttpGet("area-status")]
        [AllowAnonymous]
        public IActionResult GetAreaWithPersentase()
        {
            var result = _context.Area_Parkir
                .Select(area => new ParkirOutputDTO
                {
                    Id_Area = area.Id_Area,
                    Nama_Area = area.Nama_Area,
                    Kapasitas_Area = area.Kapasitas_Area,
                    Terisi = _context.Riwayat_Parkir.Count(r =>
                        r.Id_Area == area.Id_Area &&
                        r.Status_Riwayat == "Parkir" &&
                        r.Waktu_Keluar == null),
                    Persen = Math.Round(
                        (double)_context.Riwayat_Parkir.Count(r =>
                            r.Id_Area == area.Id_Area &&
                            r.Status_Riwayat == "Parkir" &&
                            r.Waktu_Keluar == null)
                        / area.Kapasitas_Area * 100, 1)
                });

            return Ok(result);
        }

        // GET: api/parkir/status/{userId}
        [HttpGet("status/{userId}")]
        [Authorize]
        public IActionResult GetUserParkirStatus(int userId)
        {
            var parkirAktif = _context.Riwayat_Parkir
                .Where(r => r.Id_User == userId && r.Status_Riwayat == "Parkir" && r.Waktu_Keluar == null)
                .OrderByDescending(r => r.Waktu_Masuk)
                .FirstOrDefault();

            if (parkirAktif != null)
            {
                return Ok(new
                {
                    active = true,
                    areaId = parkirAktif.Id_Area,
                    riwayatId = parkirAktif.Id_Riwayat
                });
            }
            else
            {
                return Ok(new { active = false });
            }
        }

        // POST: api/parkir
        [HttpPost]
        [Authorize(Roles = "petugas,mahasiswa,dosen")]
        public async Task<IActionResult> ParkirMasuk([FromBody] ParkirInputDTO input)
        {
            Console.WriteLine($"[SERVER] Request masuk: UserId={input.Id_User}, AreaId={input.Id_Area}");

            var alreadyParked = _context.Riwayat_Parkir.Any(r =>
                r.Id_User == input.Id_User &&
                r.Status_Riwayat == "Parkir" &&
                r.Waktu_Keluar == null);

            if (alreadyParked)
            {
                Console.WriteLine("[SERVER] Sudah parkir di tempat lain");
                return BadRequest(new { message = "Kamu sudah parkir di area lain." });
            }

            var riwayat = new RiwayatParkir
            {
                Id_User = input.Id_User,
                Id_Area = input.Id_Area,
                Waktu_Masuk = DateTime.UtcNow,
                Status_Riwayat = "Parkir"
            };

            _context.Riwayat_Parkir.Add(riwayat);
            await _context.SaveChangesAsync();

            Console.WriteLine($"[SERVER] Parkir berhasil. Id Riwayat: {riwayat.Id_Riwayat}");

            return Ok(new { message = "Berhasil parkir", id = riwayat.Id_Riwayat });
        }


        // PUT: api/parkir/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "petugas,mahasiswa,dosen")]
        public async Task<IActionResult> KeluarParkir(int id)
        {
            var riwayat = await _context.Riwayat_Parkir.FindAsync(id);
            if (riwayat == null || riwayat.Waktu_Keluar != null)
                return NotFound(new { message = "Data tidak ditemukan atau sudah keluar" });

            riwayat.Waktu_Keluar = DateTime.UtcNow;
            riwayat.Status_Riwayat = "Keluar";

            await _context.SaveChangesAsync();
            return Ok(new { message = "Berhasil keluar parkir" });
        }

    }

    public class ParkirInputDTO
    {
        public int Id_User { get; set; }
        public int Id_Area { get; set; }
    }

    public class ParkirOutputDTO
    {
        public int Id_Area { get; set; }
        public string Nama_Area { get; set; }
        public int Kapasitas_Area { get; set; }
        public int Terisi { get; set; }
        public double Persen { get; set; }
    }
}
