using faspark_be.Database;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class ParkingReport : ControllerBase
{
    private readonly AppDbContext _context;

    public ParkingReport(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Record/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRecord(int id)
    {
        var record = await _context.Reports.FindAsync(id);

        if (record == null)
        {
            return NotFound(new { message = "Record not found" });
        }

        return Ok(record);
    }
}
