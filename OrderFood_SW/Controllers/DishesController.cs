using Microsoft.AspNetCore.Mvc;
using OrderFood_SW.Models;
using OrderFood_SW.Services;
using OrderFood_SW.Helper;

namespace OrderFood_SW.Controllers
{
    [AuthorizeRole("Admin")]
    public class DishesController : Controller
    {
        private readonly DishService _service;

        public DishesController(DishService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index(string keyword = "", int page = 1)
        {
            int pageSize = 8;
            var (dishes, totalRows) = await _service.GetPagedAsync(keyword, page, pageSize);

            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRows / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.Keyword = keyword;

            return View(dishes);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Dish dish, IFormFile? ImageFile)
        {
            var imageName = await _service.SaveImageAsync(ImageFile);
            if (imageName != null) dish.ImageUrl = imageName;

            ModelState.Remove("ImageFile");
            ModelState.Remove("OrderDetails");

            if (!ModelState.IsValid) return View(dish);

            await _service.AddAsync(dish);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var dish = await _service.GetByIdAsync(id);
            if (dish == null) return NotFound();
            return View(dish);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Dish dish, IFormFile? ImageFile, string OldImageUrl)
        {
            if (id != dish.DishId) return NotFound();

            var imageName = await _service.SaveImageAsync(ImageFile);

            if (!string.IsNullOrEmpty(imageName))
            {
                // xóa ảnh cũ nếu có ảnh mới
                if (!string.IsNullOrEmpty(OldImageUrl) && OldImageUrl != "nophoto1.png")
                {
                    _service.DeleteImage(OldImageUrl);
                }
                dish.ImageUrl = imageName;
            }
            else
            {
                dish.ImageUrl = OldImageUrl;
            }

            ModelState.Remove("ImageFile");
            ModelState.Remove("OrderDetails");

            if (!ModelState.IsValid) return View(dish);

            await _service.UpdateAsync(dish);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var dish = await _service.GetByIdAsync(id);
            if (dish == null) return NotFound();
            return View(dish);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var dish = await _service.GetByIdAsync(id);
            if (dish == null) return NotFound();
            return View(dish);
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
