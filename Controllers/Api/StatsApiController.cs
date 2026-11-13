using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoomTracker.Controllers.Api;

[Route("api/stats")]
[ApiController]
[Authorize]
public class StatsApiController : ControllerBase
{
    private readonly AppDbContext _context;

    public StatsApiController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("status-chart")]
    public async Task<IActionResult> GetStatusStats()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        var stats = await _context.Debts
            .Where(d => d.UserId == userId)
            .GroupBy(d => d.Status)
            .Select(g => new
            {
                status = g.Key.ToString(),
                count = g.Count()
            })
            .ToListAsync();

        return Ok(stats);
    }
}