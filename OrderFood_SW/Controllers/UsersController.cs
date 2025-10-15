using Microsoft.AspNetCore.Mvc;
using OrderFood_SW.Helper;
using OrderFood_SW.Models;
using OrderFood_SW.Services;

namespace OrderFood_SW.Controllers
{
    [AuthorizeRole("Admin")]
    public class UsersController : Controller
    {
        private readonly UserService _service;

        public UsersController(UserService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index(string keyword = "", int page = 1)
        {
            int pageSize = 8;
            var (users, totalRows) = await _service.GetPagedAsync(keyword, page, pageSize);

            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRows / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.Keyword = keyword;

            return View(users);
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _service.GetByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Users user)
        {
            if (user.ImageFile != null)
            {
                var fileName = await _service.SaveImageAsync(user.ImageFile);
                user.ImageAvat = fileName ?? "nophoto1.png";
            }

            if (!ModelState.IsValid)
            {
                return View(user);
            }

            await _service.AddAsync(user);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var user = await _service.GetByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Users user, IFormFile? ImageFile, string OldImageUrl)
        {
            if (id != user.UserId) return NotFound();

            var imageName = await _service.SaveImageAsync(ImageFile);

            if (!string.IsNullOrEmpty(imageName))
            {
                // xóa ảnh cũ nếu có ảnh mới
                if (!string.IsNullOrEmpty(OldImageUrl) && OldImageUrl != "nophoto1.png")
                {
                    _service.DeleteImage(OldImageUrl);
                }
                user.ImageAvat = imageName;
            }
            else
            {
                user.ImageAvat = OldImageUrl;
            }

            ModelState.Remove("ImageFile");

            if (!ModelState.IsValid)
            {
                return View(user);
            }

            await _service.UpdateAsync(user);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var user = await _service.GetByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _service.GetByIdAsync(id);
            if (user != null) await _service.DeleteAsync(user);
            return RedirectToAction(nameof(Index));
        }
    }
}
