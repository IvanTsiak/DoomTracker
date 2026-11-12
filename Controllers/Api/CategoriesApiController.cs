using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoomTracker;
using DoomTracker.Models;

namespace DoomTracker.Controllers_Api
{
    [Route("api/categories")]
    [ApiController]
    public class CategoriesApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriesApiController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/CategoriesApi
        [HttpGet]
        public async Task<ActionResult<object>> GetCategories([FromQuery] int skip = 0, [FromQuery] int limit = 10)
        {
            var totalCount = await _context.Categories.CountAsync();
            
            var categories = await _context.Categories
                .OrderBy(c => c.Id)
                .Skip(skip)
                .Take(limit)
                .ToListAsync();
            
            string? nextLink = null;
            if (skip + limit < totalCount)
            {
                var nextSkip = skip + limit;
                var request = HttpContext.Request;
                var baseUri = $"{request.Scheme}://{request.Host}{request.PathBase}{request.Path}";
                nextLink = $"{baseUri}?skip={skip}&limit={limit}";
            }

            return Ok(new
            {
                totalCount = totalCount,
                skip = skip,
                limit = limit,
                nextLink = nextLink,
                data = categories
            });
        }

        // GET: api/CategoriesApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }

        // PUT: api/CategoriesApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, Category category)
        {
            if (id != category.Id)
            {
                return BadRequest();
            }

            _context.Entry(category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
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

        // POST: api/CategoriesApi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Category>> PostCategory(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCategory", new { id = category.Id }, category);
        }

        // DELETE: api/CategoriesApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchCategories([FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 3)
            {
                return Ok(new { results = new List<object>() });
            }
            
            var categories = await _context.Categories
                .Where(c => c.Name.ToLower().Contains(term.ToLower()))
                .OrderBy(c => c.Name)
                .Take(50)
                .Select(c => new 
                { 
                    id = c.Id, 
                    text = c.Name 
                })
                .ToListAsync();

            return Ok(new { results = categories });
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
        
    }
}
