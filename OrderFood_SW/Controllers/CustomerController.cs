using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderFood_SW.Helper;
using OrderFood_SW.Models;
using OrderFood_SW.ViewModels;
using System.Linq;

namespace OrderFood_SW.Controllers
{
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
        public IActionResult OrderHistory()
        {
            int userIdStr = (int)HttpContext.Session.GetInt32("UserId") ;

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

            // Cập nhật trạng thái đơn hàng sang -1 (bị hủy)
            order.OrderStatus = -1;
            order.TotalAmount = 0;

            // Cập nhật trạng thái bàn về "Available"
            var table = _db.Tables.FirstOrDefault(t => t.TableId == order.TableId);
            if (table != null)
            {
                table.Status = "Available";
            }

            _db.SaveChanges();

            TempData["Success"] = "Đơn hàng đã được hủy (lưu trạng thái trong hệ thống).";
            return RedirectToAction("CreateOrder", "CustomerOrder", new { tableId = order.TableId });
        }

        [HttpPost]
        public async Task<IActionResult>ReOrder(int orderId)
        {
            // Lấy order cũ
            var oldOrder = await _db.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (oldOrder == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng cũ!";
                return RedirectToAction("OrderHistory");
            }

            // Lấy giỏ hàng hiện tại từ session
            var cart = HttpContext.Session.GetObject<List<OrderCartItem>>("Cart")
                       ?? new List<OrderCartItem>();

            // Thêm/cộng dồn món từ order cũ vào giỏ hàng
            foreach (var item in oldOrder.OrderDetails)
            {
                var dish = await _db.Dishes.FirstOrDefaultAsync(d => d.DishId == item.DishId);
                if (dish != null)
                {
                    var existingItem = cart.FirstOrDefault(c => c.DishId == dish.DishId);
                    if (existingItem != null)
                    {
                        // Nếu món đã có thì cộng thêm số lượng
                        existingItem.Quantity += item.Quantity;
                    }
                    else
                    {
                        // Nếu món chưa có thì thêm mới
                        cart.Add(new OrderCartItem
                        {
                            DishId = dish.DishId,
                            DishName = dish.DishName,
                            ImageUrl = dish.ImageUrl,
                            Price = dish.DishPrice,
                            Quantity = item.Quantity
                        });
                    }
                }
            }

            // Lưu lại giỏ hàng vào session
            HttpContext.Session.SetObject("Cart", cart);

            TempData["Success"] = $"Đã thêm {oldOrder.OrderDetails.Count} món từ đơn #{oldOrder.OrderId} vào giỏ hàng!";
            return RedirectToAction("Index", "CustomerCart");
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

            // Lấy tên bàn (join thủ công)
            var table = await _db.Tables
                .FirstOrDefaultAsync(t => t.TableId == order.TableId);

            ViewBag.TableNumber = table;

            // Lấy chi tiết order + join với Dish
            var orderDetails = await _db.OrderDetails
                .Where(od => od.OrderId == orderId)
                .Join(_db.Dishes,
                    od => od.DishId,
                    d => d.DishId,
                    (od, d) => new DetailsWithDish
                    {
                        DishId = d.DishId,
                        ImageUrl = d.ImageUrl,
                        DishName = d.DishName,
                        Quantity = od.Quantity,
                        DishPrice = d.DishPrice,
                        DishStatus = od.DishStatus, // 0: chưa phục vụ, 1: đã phục vụ
                        OrderId = od.OrderId
                    })
                .ToListAsync();

            var vm = new OrderDetailViewModel
            {
                Order = order,
                OrderDetails = orderDetails
            };

            return View(vm);
        }

    }
}
