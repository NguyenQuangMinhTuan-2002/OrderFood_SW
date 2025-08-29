using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderFood_SW.Helper;
using OrderFood_SW.Models;

namespace OrderFood_SW.Controllers
{
    [AllowAnonymous] // cho phép khách dùng QR mà không cần login
    public class GuestController : Controller
    {
        private readonly DatabaseHelperEF _db;

        public GuestController(DatabaseHelperEF db)
        {
            _db = db;
        }

        // http://localhost:7000/Guest/QRCheck?tableId=1
        public IActionResult QRCheck(int tableId)
        {
            var table = _db.Tables.FirstOrDefault(t => t.TableId == tableId);
            if (table == null)
            {
                TempData["Error"] = "Không tìm thấy bàn này.";
                return RedirectToAction("Index", "Home");
            }

            // Set session cho guest
            HttpContext.Session.SetInt32("CurrentTableId", table.TableId);
            HttpContext.Session.SetInt32("TableId", table.TableId);
            HttpContext.Session.SetString("Role", "Customer");

            if (!HttpContext.Session.GetInt32("UserId").HasValue)
                HttpContext.Session.SetInt32("UserId", 1); // Guest mặc định

            var currentUserId = HttpContext.Session.GetInt32("UserId");

            // Nếu bàn có order mở
            if (table.CurrentOrderId.HasValue)
            {
                var order = _db.Orders.FirstOrDefault(o => o.OrderId == table.CurrentOrderId.Value);

                if (order != null && order.OrderStatus == 1)
                {
                    if (currentUserId == 1) // Guest đang quét QR
                    {
                        if (order.UserId == 1)
                        {
                            // Cho phép Guest tiếp tục xem order cũ của mình
                            HttpContext.Session.SetInt32("CurrentOrderId", order.OrderId);
                            return RedirectToAction("OrderDetails", "Customer", new { orderId = order.OrderId });
                        }
                        else
                        {
                            // Đơn này đã thuộc về người dùng thật -> chặn Guest
                            return RedirectToAction("AccessDenied", "Account");
                        }
                    }
                }

                // Order đã đóng/hủy → reset bàn
                table.Status = "Available";
                table.CurrentOrderId = null;
                _db.SaveChanges();
            }

            // Nếu tới đây → chưa có order -> Guest được tạo đơn mới
            HttpContext.Session.Remove("CurrentOrderId");
            HttpContext.Session.Remove("Cart");
            return RedirectToAction("CreateOrder", "CustomerOrder", new { tableId = table.TableId });
        }
    }
}
