using Microsoft.EntityFrameworkCore;
using OrderFood_SW.Helper;
using OrderFood_SW.Models;
using OrderFood_SW.ViewModels;

namespace OrderFood_SW.Repositories
{
    public class CustomerRepository
    {
        private readonly DatabaseHelperEF _db;

        public CustomerRepository(DatabaseHelperEF db)
        {
            _db = db;
        }

        public List<OrderHistoryViewModel> GetRecentOrdersByUser(int userId, int take = 4)
        {
            var orders = _db.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderTime)
                .Select(o => new OrderHistoryViewModel
                {
                    OrderId = o.OrderId,
                    OrderTime = o.OrderTime,
                    OrderStatus = o.OrderStatus.ToString(),
                    TotalAmount = o.TotalAmount,
                    Note = o.note ?? "n/a",
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
                .Take(take)
                .ToList();

            return orders;
        }

        public (List<OrderHistoryViewModel> Orders, int TotalOrders, int TotalPages)
            GetOrderHistory(int userId, string status, DateTime? fromDate, DateTime? toDate, int page, int pageSize)
        {
            var query = _db.Orders.Where(o => o.UserId == userId);

            // --- Lọc theo trạng thái ---
            if (!string.IsNullOrEmpty(status))
            {
                if (status == "completed")
                    query = query.Where(o => o.OrderStatus == 2);
                else if (status == "cancelled")
                    query = query.Where(o => o.OrderStatus == -1);
            }
            else
            {
                query = query.Where(o => o.OrderStatus == 2 || o.OrderStatus == -1);
            }

            // --- Lọc theo ngày ---
            if (fromDate.HasValue)
                query = query.Where(o => o.OrderTime >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(o => o.OrderTime <= toDate.Value);

            // Tổng số đơn
            var totalOrders = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalOrders / pageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            // Lấy dữ liệu
            var orders = query
                .OrderByDescending(o => o.OrderTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new OrderHistoryViewModel
                {
                    OrderId = o.OrderId,
                    OrderTime = o.OrderTime,
                    OrderStatus = o.OrderStatus.ToString(),
                    TotalAmount = o.TotalAmount,
                    Note = o.note ?? "n/a",
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
                                UnitPrice = d.DishPrice,
                                TaxRate = (decimal)d.TaxRate,
                                Note = od.Note ?? "n/a"
                            })
                        .ToList()
                })
                .ToList();

            return (orders, totalOrders, totalPages);
        }

        public List<OrderHistoryViewModel> GetProcessingOrders(int userId)
        {
            var orders = _db.Orders
                .Where(o => o.UserId == userId && o.OrderStatus == 1)
                .OrderByDescending(o => o.OrderTime)
                .Select(o => new OrderHistoryViewModel
                {
                    OrderId = o.OrderId,
                    OrderTime = o.OrderTime,
                    OrderStatus = o.OrderStatus.ToString(),
                    TotalAmount = o.TotalAmount,
                    Note = o.note ?? "n/a",
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
                                UnitPrice = d.DishPrice,
                                TaxRate = (decimal)d.TaxRate,
                                Note = od.Note ?? "n/a"
                            })
                        .ToList()
                })
                .ToList();

            return orders;
        }

        public Order? GetOrderWithDetails(int orderId)
        {
            return _db.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.OrderId == orderId);
        }

        public Table? GetTableById(int tableId)
        {
            return _db.Tables.FirstOrDefault(t => t.TableId == tableId);
        }

        public void UpdateOrder(Order order)
        {
            _db.Orders.Update(order);
        }

        public void UpdateTable(Table table)
        {
            _db.Tables.Update(table);
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _db.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<Table?> GetTableByIdAsync(int tableId)
        {
            return await _db.Tables.FirstOrDefaultAsync(t => t.TableId == tableId);
        }

        public async Task<List<DetailsWithDish>> GetOrderDetailsWithDishesAsync(int orderId)
        {
            return await _db.OrderDetails
            .Where(od => od.OrderId == orderId)
            .Join(_db.Dishes,od => od.DishId,d => d.DishId,(od, d) => new DetailsWithDish
                {
                    DishId = d.DishId,
                    ImageUrl = d.ImageUrl ?? "nophoto.png",
                    DishName = d.DishName,
                    Quantity = od.Quantity,
                    DishPrice = d.DishPrice,
                    DishStatus = od.DishStatus,
                    OrderId = od.OrderId,
                    TaxRate = (od.TaxRate ?? 0.1m),
                    Note = od.Note ?? ""
                })
            .ToListAsync();
        }

        public async Task<Users?> GetByIdAsync(int userId)
        {
            return await _db.Users.FindAsync(userId);
        }

        public async Task UpdateAsync(Users user)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _db.Users.AnyAsync(e => e.UserId == id);
        }
    }
}
