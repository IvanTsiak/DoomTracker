using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using DoomTracker.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace DoomTracker.Controllers
{
    [Authorize]
    public class DebtsController : Controller
    {
        private readonly AppDbContext _context;

        public DebtsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Debts
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Debts.Include(d => d.Category).Include(d => d.User);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Debts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var debt = await _context.Debts
                .Include(d => d.Category)
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (debt == null)
            {
                return NotFound();
            }

            return View(debt);
        }

        // GET: Debts/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email");
            return View();
        }

        // POST: Debts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,CategoryId,Title,Description,Deadline,Status,CreatedAt")] Debt debt, IFormFile? uploadedFile)
        {
            debt.UserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            if (ModelState.IsValid)
            {
                debt.Deadline = debt.Deadline.ToUniversalTime();
                
                if (uploadedFile != null && uploadedFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    Directory.CreateDirectory(uploadsFolder);
                    
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + uploadedFile.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadedFile.CopyToAsync(stream);
                    }
                    
                    debt.ProofFilePath = "/uploads/" + uniqueFileName;
                }
                
                _context.Add(debt);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", debt.CategoryId);
            return View(debt);
        }

        // GET: Debts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var debt = await _context.Debts.FindAsync(id);
            if (debt == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", debt.CategoryId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", debt.UserId);
            return View(debt);
        }

        // POST: Debts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,CategoryId,Title,Description,Deadline,Status,ProofFilePath,CreatedAt")] Debt debt)
        {
            if (id != debt.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                debt.Deadline = debt.Deadline.ToUniversalTime();
                
                try
                {
                    _context.Update(debt);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DebtExists(debt.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", debt.CategoryId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", debt.UserId);
            return View(debt);
        }

        // GET: Debts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var debt = await _context.Debts
                .Include(d => d.Category)
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (debt == null)
            {
                return NotFound();
            }

            return View(debt);
        }

        // POST: Debts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var debt = await _context.Debts.FindAsync(id);
            if (debt != null)
            {
                _context.Debts.Remove(debt);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DebtExists(int id)
        {
            return _context.Debts.Any(e => e.Id == id);
        }
    }
}
