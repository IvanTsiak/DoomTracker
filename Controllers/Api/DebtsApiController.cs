using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoomTracker;
using DoomTracker.Models;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using DoomTracker.Services;
using Microsoft.AspNetCore.Authorization;

namespace DoomTracker.Controllers_Api
{
    [Route("api/debts")]
    [ApiController]
    [Authorize]
    public class DebtsApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IFileService _fileService;

        public DebtsApiController(AppDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        // GET: api/DebtsApi
        [HttpGet]
        public async Task<ActionResult<object>> GetDebts([FromQuery] int skip = 0, [FromQuery] int limit = 10)
        {
            var totalCount = await _context.Debts.CountAsync();
            
            var debts = await _context.Debts
                .Include(d => d.Category)
                .OrderByDescending(d => d.CreatedAt)
                .Skip(skip)
                .Take(limit)
                .ToListAsync();

            string? nextLink = null;
            if (skip + limit < totalCount)
            {
                var nextSkip = skip + limit;
                var request = HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}{request.Path}";
                nextLink = $"{baseUrl}?skip={nextSkip}&limit={limit}";
            }

            return Ok(new
            {
                totalCount = totalCount,
                skip = skip,
                limit = limit,
                nextLink = nextLink,
                data = debts
            });
        }

        // GET: api/DebtsApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Debt>> GetDebt(int id)
        {
            var debt = await _context.Debts.FindAsync(id);

            if (debt == null)
            {
                return NotFound();
            }

            return debt;
        }

        // PUT: api/DebtsApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDebt(int id, Debt debt)
        {
            if (id != debt.Id)
            {
                return BadRequest();
            }

            _context.Entry(debt).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DebtExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/DebtsApi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Debt>> PostDebt([FromForm] Debt debt, IFormFile? uploadedFile)
        {
            debt.UserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (uploadedFile != null)
            {
                debt.ProofFilePath = await _fileService.UploadFileAsync(uploadedFile);
            }
            
            _context.Debts.Add(debt);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction("GetDebt", new { id = debt.Id }, debt);
        }

        // DELETE: api/DebtsApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDebt(int id)
        {
            var debt = await _context.Debts.FindAsync(id);
            if (debt == null)
            {
                return NotFound();
            }

            _context.Debts.Remove(debt);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DebtExists(int id)
        {
            return _context.Debts.Any(e => e.Id == id);
        }
    }
}
