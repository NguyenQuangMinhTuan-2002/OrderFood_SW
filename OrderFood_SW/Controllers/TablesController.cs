using Microsoft.AspNetCore.Mvc;
using OrderFood_SW.Helper;
using OrderFood_SW.Models;
using OrderFood_SW.Services;

namespace OrderFood_SW.Controllers
{
    [AuthorizeRole("Admin")]
    public class TablesController : Controller
    {
        private readonly TableService _service;

        public TablesController(TableService service)
        {
            _service = service;
        }

        // GET: /Tables
        public async Task<IActionResult> Index(string keyword = "", int page = 1)
        {
            int pageSize = 8;
            var (tables, totalRows) = await _service.GetPagedAsync(keyword, page, pageSize);

            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRows / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.Keyword = keyword;

            return View(tables);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Table table)
        {
            if (!ModelState.IsValid)
                return View(table);

            await _service.AddAsync(table);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var table = await _service.GetByIdAsync(id);
            if (table == null) return NotFound();
            return View(table);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Table table)
        {
            if (id != table.TableId) return NotFound();
            if (!ModelState.IsValid) return View(table);

            await _service.UpdateAsync(table);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var table = await _service.GetByIdAsync(id);
            if (table == null) return NotFound();
            return View(table);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var table = await _service.GetByIdAsync(id);
            if (table == null) return NotFound();
            return View(table);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
