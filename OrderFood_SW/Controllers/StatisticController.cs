using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderFood_SW.Helper;
using OrderFood_SW.ViewModels;

namespace OrderFood_SW.Controllers
{
    [AuthorizeRole("Admin", "Staff")]
    public class StatisticController : Controller
    {
        private readonly DatabaseHelperEF _db;

        public StatisticController(DatabaseHelperEF db)
        {
            _db = db;
        }

        public IActionResult Revenue(string period = "day")
        {
            var now = DateTime.Now;
            DateTime startDate, endDate;

            switch (period.ToLower())
            {
                case "week":
                    int diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
                    startDate = now.Date.AddDays(-diff); // Thứ 2 tuần hiện tại
                    endDate = startDate.AddDays(7);      // hết Chủ nhật
                    break;
                case "month":
                    startDate = new DateTime(now.Year, now.Month, 1);
                    endDate = startDate.AddMonths(1);
                    break;
                case "year":
                    startDate = new DateTime(now.Year, 1, 1);   // 01/01 năm nay
                    endDate = startDate.AddYears(1);            // 01/01 năm sau
                    break;
                default: // day
                    startDate = now.Date;
                    endDate = startDate.AddDays(1);
                    break;
            }

            var orders = _db.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Dish)
                .Where(o => o.OrderTime >= startDate && o.OrderTime < endDate && o.OrderStatus == 2) // chỉ tính đơn đã duyệt)
                .ToList();

            if (!orders.Any())
            {
                return View(new RevenueViewModel { Period = period });
            }

            var totalRevenue = orders.Sum(o =>
                o.OrderDetails.Sum(od => od.Quantity * od.Dish.DishPrice));

            var totalOrders = orders.Count;
            var avgOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            var topDishes = orders
                .SelectMany(o => o.OrderDetails)
                .GroupBy(od => od.Dish.DishName)
                .Select(g => new TopDishViewModel
                {
                    DishName = g.Key,
                    Quantity = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.Quantity * x.Dish.DishPrice)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(5)
                .ToList();

            var shiftRevenue = new Dictionary<string, decimal>
            {
                { "Morning", orders.Where(o => o.OrderTime.Hour >= 6 && o.OrderTime.Hour < 12)
                                   .Sum(o => o.OrderDetails.Sum(od => od.Quantity * od.Dish.DishPrice)) },
                { "Noon", orders.Where(o => o.OrderTime.Hour >= 12 && o.OrderTime.Hour < 18)
                                .Sum(o => o.OrderDetails.Sum(od => od.Quantity * od.Dish.DishPrice)) },
                { "Evening", orders.Where(o => o.OrderTime.Hour >= 18 && o.OrderTime.Hour < 24)
                                   .Sum(o => o.OrderDetails.Sum(od => od.Quantity * od.Dish.DishPrice)) }
            };

            var model = new RevenueViewModel
            {
                Period = period,
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                AvgOrderValue = avgOrderValue,
                TopDishes = topDishes,
                ShiftRevenue = shiftRevenue
            };

            return View(model);
        }

        public async Task<IActionResult> Order(int? month, int? year)
        {
            var selectedMonth = month ?? DateTime.Now.Month;
            var selectedYear = year ?? DateTime.Now.Year;

            ViewBag.month = month;
            ViewBag.year = year;

            // ngày đầu và cuối của tháng được chọn
            var startDate = new DateTime(selectedYear, selectedMonth, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            // Lấy dữ liệu tổng trong tháng
            var approvedCount = await _db.Orders.CountAsync(o => o.OrderStatus == 2 && o.OrderTime >= startDate && o.OrderTime <= endDate);
            var cancelledCount = await _db.Orders.CountAsync(o => o.OrderStatus == -1 && o.OrderTime >= startDate && o.OrderTime <= endDate);
            var pendingCount = await _db.Orders.CountAsync(o => o.OrderStatus == 1 && o.OrderTime >= startDate && o.OrderTime <= endDate);

            // Gom theo tuần trong tháng
            var weeklyData = await _db.Orders
                .Where(o => o.OrderTime >= startDate && o.OrderTime <= endDate)
                .GroupBy(o => (o.OrderTime.Day - 1) / 7) // chia ngày ra theo tuần (0 = week1, 1 = week2…)
                .Select(g => new
                {
                    Week = g.Key,
                    Approved = g.Count(o => o.OrderStatus == 2),
                    Cancelled = g.Count(o => o.OrderStatus == -1),
                    Pending = g.Count(o => o.OrderStatus == 1)
                })
                .ToListAsync();

            var model = new OrderStatisticViewModel
            {
                Approved = approvedCount,
                Cancelled = cancelledCount,
                Pending = pendingCount,
                WeeklyApproved = Enumerable.Range(0, 4).Select(w => weeklyData.FirstOrDefault(x => x.Week == w)?.Approved ?? 0).ToList(),
                WeeklyCancelled = Enumerable.Range(0, 4).Select(w => weeklyData.FirstOrDefault(x => x.Week == w)?.Cancelled ?? 0).ToList(),
                WeeklyPending = Enumerable.Range(0, 4).Select(w => weeklyData.FirstOrDefault(x => x.Week == w)?.Pending ?? 0).ToList()
            };

            return View(model);
        }


        public IActionResult Employee()
        {
            return View();
        }
    }
}
