using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DoomTracker.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using DoomTracker.Services;
using Microsoft.ApplicationInsights;

namespace DoomTracker.Controllers
{
    [Authorize]
    public class DebtsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IFileService _fileService;
        private readonly TelemetryClient _telemetryClient;

        public DebtsController(AppDbContext context, IFileService fileService,  TelemetryClient telemetryClient)
        {
            _context = context;
            _fileService = fileService;
            _telemetryClient = telemetryClient;
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
                .Include(d => d.Logs.OrderByDescending(l => l.CreatedAt))
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
                
                if (uploadedFile != null)
                {
                    debt.ProofFilePath = await _fileService.UploadFileAsync(uploadedFile);
                }
                
                debt.Logs.Add(new DebtLog
                {
                    ActionType = DebtActionType.Created,
                    NewStatus = debt.Status,
                    LogMessage = "Debt successfully created"
                });
                
                _context.Add(debt);
                await _context.SaveChangesAsync();
                
                _telemetryClient.TrackEvent("DebtCreated", new Dictionary<string, string> 
                { 
                    { "DebtTitle", debt.Title },
                    { "CategoryId", debt.CategoryId.ToString() }
                });
                
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
                debt.CreatedAt = debt.CreatedAt.ToUniversalTime();
                
                try
                {
                    var originalDebt = await _context.Debts.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
                    if (originalDebt == null)
                    {
                        return NotFound();
                    }
                    
                    if (originalDebt.Status != debt.Status)
                    {
                        var log = new DebtLog
                        {
                            DebtId = debt.Id,
                            ActionType = DebtActionType.StatusChanged,
                            OldStatus = originalDebt.Status,
                            NewStatus = debt.Status,
                            LogMessage = $"Status changed from {originalDebt.Status} to {debt.Status}"
                        };
                        _context.DebtLogs.Add(log);
                    }
                    
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
