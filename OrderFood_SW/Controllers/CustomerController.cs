using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderFood_SW.Helper;
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

        public IActionResult OrderHistory()
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
                .ToList();

            return View(orders);
        }

        [HttpPost]
        public IActionResult CustomerCancelOrder(int orderId)
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
    }
}
