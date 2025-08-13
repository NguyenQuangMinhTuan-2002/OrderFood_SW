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
    }
}
