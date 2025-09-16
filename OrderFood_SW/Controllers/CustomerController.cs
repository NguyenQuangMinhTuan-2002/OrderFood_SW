using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderFood_SW.Models;
using OrderFood_SW.Services;
using OrderFood_SW.ViewModels;

namespace OrderFood_SW.Controllers
{
    [AllowAnonymous]
    public class CustomerController : Controller
    {
        private readonly CustomerService _service;

        public CustomerController(CustomerService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            int? userIdSession = HttpContext.Session.GetInt32("UserId");
            if (userIdSession == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập để xem đơn hàng.";
                return RedirectToAction("Login", "Account");
            }

            int userId = userIdSession.Value;
            var orders = _service.GetRecentOrders(userId);

            return View(orders);
        }

        public IActionResult OrderHistory(string status = "", DateTime? fromDate = null, DateTime? toDate = null, int page = 1)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 1;
            const int pageSize = 10;

            var (orders, totalOrders, totalPages) = _service.GetOrderHistory(userId, status, fromDate, toDate, page, pageSize);

            // Pagination info
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.HasPreviousPage = page > 1;
            ViewBag.HasNextPage = page < totalPages;
            ViewBag.TotalOrders = totalOrders;

            // Giữ lại filter để bind ra view
            ViewBag.Status = status;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

            return View(orders);
        }

        public IActionResult OrderProcessing()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 1;
            var orders = _service.GetProcessingOrders(userId);
            return View(orders);
        }


        [HttpPost]
        public IActionResult CancelOrder(int orderId)
        {
            var result = _service.CancelOrder(orderId);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("OrderHistory", new { orderId });
            }

            TempData["Success"] = result.Message;
            return RedirectToAction("CreateOrder", "CustomerOrder", new { tableId = result.TableId });
        }

        public async Task<IActionResult> OrderDetails(int orderId)
        {
            var result = await _service.GetOrderDetailsAsync(orderId);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("OrderHistory");
            }

            ViewBag.TableNumber = result.TableNumber;
            return View(result.Data);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var vm = await _service.GetUserForEditAsync(id.Value);
            if (vm == null) return NotFound();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditUserViewModel vm, IFormFile? ImageFile, string OldImageUrl)
        {
            if (id != vm.UserId) return NotFound();

            var imageName = await _service.SaveImageAsync(ImageFile);

            if (!string.IsNullOrEmpty(imageName))
            {
                // xóa ảnh cũ nếu có ảnh mới
                if (!string.IsNullOrEmpty(OldImageUrl) && OldImageUrl != "nophoto1.png")
                {
                    _service.DeleteImage(OldImageUrl);
                }
                vm.ImageAvat = imageName;
            }
            else
            {
                vm.ImageAvat = OldImageUrl;
            }
            ModelState.Remove("ImageFile");

            if (ModelState.IsValid)
            {
                var user = await _service.UpdateUserAsync(vm);
                if (user == null) return NotFound();

                // update session values
                HttpContext.Session.SetString("FullName", vm.FullName);
                HttpContext.Session.SetString("Email", vm.Email);

                return RedirectToAction(nameof(Index));
            }

            await _service.UpdateUserAsync(vm);
            return View(vm);
        }

        private async Task<bool> UsersExists(int id)
        {
            return await _service.UserExistsAsync(id);
        }
    }
}