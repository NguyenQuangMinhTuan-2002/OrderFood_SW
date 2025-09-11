using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderFood_SW.Helper;
using OrderFood_SW.Models;
using OrderFood_SW.ViewModels;

namespace OrderFood_SW.Controllers
{
    [AllowAnonymous]
    public class CustomerController : Controller
    {
        private readonly DatabaseHelperEF _db;

        public CustomerController(DatabaseHelperEF db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            int userIdStr = (int)HttpContext.Session.GetInt32("UserId");

            int userId = userIdStr;

            // Lấy danh sách đơn hàng của user
            var orders = _db.Orders
                .Where(o => o.UserId == userId) // lọc theo khách hàng
                .OrderByDescending(o => o.OrderTime)
                .Select(o => new OrderHistoryViewModel
                {
                    OrderId = o.OrderId,
                    OrderTime = o.OrderTime,
                    OrderStatus = o.OrderStatus.ToString(),
                    TotalAmount = o.TotalAmount,
                    Note = o.note,
                    OrderDetails = _db.OrderDetails
                        .Where(od => od.OrderId == o.OrderId)
                        .Join(_db.Dishes,
                            od => od.DishId,
                            d => d.DishId,
                            (od, d) => new OrderHistoryDetailViewModel
                            {
                                DishName = d.DishName,
                                Quantity = od.Quantity,
                                UnitPrice = d.DishPrice
                            })
                        .ToList()
                })
                .Take(4)
                .ToList();

            return View(orders);
        }

        public IActionResult OrderHistory(string status = "", DateTime? fromDate = null, DateTime? toDate = null, int page = 1)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 1;
            const int pageSize = 10;

            // Query cơ bản
            var query = _db.Orders
                .Where(o => o.UserId == userId);

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
            int userIdStr = HttpContext.Session.GetInt32("UserId") ?? 1;

            int userId = userIdStr;

            // Lấy danh sách đơn hàng của user
            var orders = _db.Orders
                .Where(o => o.UserId == userId && o.OrderStatus == 1)
                .OrderByDescending(o => o.OrderTime)
                .Select(o => new OrderHistoryViewModel
                {
                    OrderId = o.OrderId,
                    OrderTime = o.OrderTime,
                    OrderStatus = o.OrderStatus.ToString(),
                    TotalAmount = o.TotalAmount,
                    Note = o.note,
                    OrderDetails = _db.OrderDetails
                        .Where(od => od.OrderId == o.OrderId)
                        .Join(_db.Dishes,
                            od => od.DishId,
                            d => d.DishId,
                            (od, d) => new OrderHistoryDetailViewModel
                            {
                                DishName = d.DishName,
                                ImageUrl = d.ImageUrl ?? "nophoto.png",
                                Quantity = od.Quantity,
                                UnitPrice = d.DishPrice
                            })
                        .ToList()
                })
                .ToList();

            return View(orders);
        }

        [HttpPost]
        public IActionResult CancelOrder(int orderId)
        {
            var order = _db.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null)
                return NotFound();

            // Kiểm tra nếu có món nào đã được phục vụ
            bool hasServed = order.OrderDetails.Any(d => d.DishStatus == 1);
            if (hasServed)
            {
                TempData["Error"] = "Không thể hủy đơn vì đã có món được phục vụ.";
                return RedirectToAction("OrderHistory", new { orderId });
            }

            _db.SaveChanges();

            TempData["Success"] = "Đơn hàng đã được hủy (lưu trạng thái trong hệ thống).";
            return RedirectToAction("CreateOrder", "CustomerOrder", new { tableId = order.TableId });
        }

        public async Task<IActionResult> OrderDetails(int orderId)
        {
            // Lấy order
            var order = await _db.Orders
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("OrderHistory");
            }

        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2")); // x2: hex format
                }
                return builder.ToString();
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            var vm = new EditUserViewModel
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditUserViewModel vm)
        {
            if (id != vm.UserId) return NotFound();

            if (ModelState.IsValid)
            {
                var user = await _db.Users.FindAsync(id);
                if (user == null) return NotFound();

                user.Username = vm.Username;
                user.FullName = vm.FullName;
                user.Email = vm.Email;
                user.Role = vm.Role;
                user.IsActive = vm.IsActive;

                HttpContext.Session.SetString("FullName", vm.FullName);
                HttpContext.Session.SetString("Email", vm.Email);

                return RedirectToAction(nameof(Index));
            }
            return View(vm);
        }

        private bool UsersExists(int id)
        {
            return _db.Users.Any(e => e.UserId == id);
        }
    }
}