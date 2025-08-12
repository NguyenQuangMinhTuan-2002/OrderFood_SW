using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderFood_SW.Helper;
using OrderFood_SW.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OrderFood_SW.Controllers
{
    [AuthorizeRole("Admin", "Staff")]
    public class DishesController : Controller
    {
        private readonly DatabaseHelperEF _db;

        public DishesController(DatabaseHelperEF db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(string keyword = "", int page = 1)
        {
            int pageSize = 8;
            int offset = (page - 1) * pageSize;

            var query = _db.Dishes.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(d => d.DishName.Contains(keyword) ||
                                         d.DishDescription.Contains(keyword) ||
                                         d.DishPrice.ToString().Contains(keyword)
                );
            }
            int totalRows = await query.CountAsync();
            var dishes = await query.OrderBy(d => d.DishId)
                                    .Skip(offset)
                                    .Take(pageSize)
                                    .ToListAsync();
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRows / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.Keyword = keyword;

            return View(dishes);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Dish dish, IFormFile ImageFile)
        {
            if (ImageFile != null && ImageFile.Length > 0)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                Directory.CreateDirectory(uploadFolder);
                string filePath = Path.Combine(uploadFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                dish.ImageUrl = fileName;
            }
            // Remove the ImageFile from ModelState to prevent validation errors
            ModelState.Remove("ImageFile");
            ModelState.Remove("OrderDetails");

            if (!ModelState.IsValid)
            {
                return View(dish);
            }

            _db.Dishes.Add(dish);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var dish = await _db.Dishes.FindAsync(id);
            if (dish == null) return NotFound();
            return View(dish);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Dish dish, IFormFile ImageFile, string OldImageUrl)
        {
            if (ImageFile != null && ImageFile.Length > 0)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                Directory.CreateDirectory(uploadFolder);
                string filePath = Path.Combine(uploadFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                dish.ImageUrl = fileName;
            }
            else
            {
                dish.ImageUrl = OldImageUrl;
            }

            // Remove the ImageFile from ModelState to prevent validation errors
            ModelState.Remove("ImageFile");
            ModelState.Remove("OrderDetails");

            if (!ModelState.IsValid)
                return View(dish);

            _db.Dishes.Update(dish);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var dish = await _db.Dishes.FindAsync(id);
            if (dish == null) return NotFound();
            return View(dish);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var dish = await _db.Dishes.FindAsync(id);
            if (dish == null) return NotFound();
            return View(dish);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dish = await _db.Dishes.FindAsync(id);
            if (dish == null) return NotFound();

            _db.Dishes.Remove(dish);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
