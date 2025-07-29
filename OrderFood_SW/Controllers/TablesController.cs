using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderFood_SW.Helper;
using OrderFood_SW.Models;

// Updated EF

namespace OrderFood_SW.Controllers
{
    public class TablesController : Controller
    {
        private readonly DatabaseHelperEF _db;

        public TablesController(DatabaseHelperEF db)
        {
            _db = db;
        }

        // GET: /Tables
        public async Task<IActionResult> Index(string keyword = "", int page = 1)
        {
            int pageSize = 8;
            int skip = (page - 1) * pageSize;

            var query = _db.Tables.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(c =>
                    c.TableNumber.Equals(keyword) ||
                    c.Description.Contains(keyword));
            }

            int totalRows = await query.CountAsync();
            var Tables = await query
                .OrderBy(c => c.TableId)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRows / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.Keyword = keyword;

            return View(Tables);
        }

        // GET: /Tables/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Tables/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Table Table)
        {
            if (!ModelState.IsValid)
                return View(Table);

            _db.Tables.Add(Table);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Tables/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var Table = await _db.Tables.FindAsync(id);
            if (Table == null)
                return NotFound();

            return View(Table);
        }

        // POST: /Tables/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Table Table)
        {
            if (id != Table.TableId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(Table);

            _db.Update(Table);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Tables/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var Table = await _db.Tables.FindAsync(id);
            if (Table == null)
                return NotFound();

            return View(Table);
        }

        // GET: /Tables/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var Table = await _db.Tables.FindAsync(id);
            if (Table == null)
                return NotFound();

            return View(Table);
        }

        // POST: /Tables/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var Table = await _db.Tables.FindAsync(id);
            if (Table != null)
            {
                _db.Tables.Remove(Table);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
