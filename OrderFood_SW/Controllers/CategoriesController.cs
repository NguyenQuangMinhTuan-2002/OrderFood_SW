using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderFood_SW.Helper;
using OrderFood_SW.Models;

namespace OrderFood_SW.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly DatabaseHelperEF _db;

        public CategoriesController(DatabaseHelperEF db)
        {
            _db = db;
        }

        // GET: /Categories
        public async Task<IActionResult> Index(string keyword = "", int page = 1)
        {
            int pageSize = 8;
            int skip = (page - 1) * pageSize;

            var query = _db.Categories.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(c =>
                    c.CategoryName.Contains(keyword) ||
                    c.CategoryDescription.Contains(keyword));
            }

            int totalRows = await query.CountAsync();
            var categories = await query
                .OrderBy(c => c.CategoryId)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRows / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.Keyword = keyword;

            return View(categories);
        }

        // GET: /Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (!ModelState.IsValid)
                return View(category);

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Categories/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        // POST: /Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.CategoryId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(category);

            _db.Update(category);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Categories/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        // GET: /Categories/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        // POST: /Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category != null)
            {
                _db.Categories.Remove(category);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
