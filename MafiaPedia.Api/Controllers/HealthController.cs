using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MafiaPedia.Api.Data;

namespace MafiaPedia.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly MafiaDbContext _context;

    public HealthController(MafiaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        await _context.Database.ExecuteSqlRawAsync("SELECT 1");
        return Ok(new { status = "ok" });
    }
}
