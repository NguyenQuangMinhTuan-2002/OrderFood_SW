using Microsoft.EntityFrameworkCore;
using OrderFood_SW.Models;
using OrderFood_SW.Repositories;
using OrderFood_SW.ViewModels;

namespace OrderFood_SW.Services
{
    public class StatisticService
    {
        private readonly StatisticRepository _repo;

        public StatisticService(StatisticRepository repo)
        {
            _repo = repo;
        }

        public RevenueViewModel GetRevenue(string period)
        {
            var now = DateTime.Now;
            DateTime startDate, endDate;

            switch (period.ToLower())
            {
                case "week":
                    int diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
                    startDate = now.Date.AddDays(-diff);
                    endDate = startDate.AddDays(7);
                    break;
                case "month":
                    startDate = new DateTime(now.Year, now.Month, 1);
                    endDate = startDate.AddMonths(1);
                    break;
                case "year":
                    startDate = new DateTime(now.Year, 1, 1);
                    endDate = startDate.AddYears(1);
                    break;
                default:
                    startDate = now.Date;
                    endDate = startDate.AddDays(1);
                    break;
            }

            var orders = _repo.GetOrders()
                .Where(o => o.OrderTime >= startDate && o.OrderTime < endDate && o.OrderStatus == 2)
                .ToList();

            if (!orders.Any())
                return new RevenueViewModel { Period = period };

            var totalRevenue = orders.Sum(o => o.OrderDetails.Sum(od => od.Quantity * od.Dish.DishPrice));
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

            return new RevenueViewModel
            {
                Period = period,
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                AvgOrderValue = avgOrderValue,
                TopDishes = topDishes,
                ShiftRevenue = shiftRevenue
            };
        }

        public async Task<OrderStatisticViewModel> GetOrderStatistic(int? month, int? year)
        {
            var selectedMonth = month ?? DateTime.Now.Month;
            var selectedYear = year ?? DateTime.Now.Year;

            var startDate = new DateTime(selectedYear, selectedMonth, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var orders = _repo.GetOrders()
                .Where(o => o.OrderTime >= startDate && o.OrderTime <= endDate);

            var approvedCount = orders.Count(o => o.OrderStatus == 2);
            var cancelledCount = orders.Count(o => o.OrderStatus == -1);
            var pendingCount = orders.Count(o => o.OrderStatus == 1);

            var weeklyData = await orders
                .GroupBy(o => (o.OrderTime.Day - 1) / 7)
                .Select(g => new
                {
                    Week = g.Key,
                    Approved = g.Count(o => o.OrderStatus == 2),
                    Cancelled = g.Count(o => o.OrderStatus == -1),
                    Pending = g.Count(o => o.OrderStatus == 1)
                })
                .ToListAsync();

            return new OrderStatisticViewModel
            {
                Approved = approvedCount,
                Cancelled = cancelledCount,
                Pending = pendingCount,
                WeeklyApproved = Enumerable.Range(0, 4).Select(w => weeklyData.FirstOrDefault(x => x.Week == w)?.Approved ?? 0).ToList(),
                WeeklyCancelled = Enumerable.Range(0, 4).Select(w => weeklyData.FirstOrDefault(x => x.Week == w)?.Cancelled ?? 0).ToList(),
                WeeklyPending = Enumerable.Range(0, 4).Select(w => weeklyData.FirstOrDefault(x => x.Week == w)?.Pending ?? 0).ToList()
            };
        }
    }
}
