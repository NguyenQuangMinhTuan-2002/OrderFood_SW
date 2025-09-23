using Microsoft.EntityFrameworkCore;
using OrderFood_SW.Helper;
using OrderFood_SW.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace OrderFood_SW.Repositories
{
    public class CustomerOrderRepository
    {
        private readonly DatabaseHelperEF _db;
        public CustomerOrderRepository(DatabaseHelperEF db)
        {
            _db = db;
        }


        public Task<List<Table>> GetAllTablesAsync() => _db.Tables.ToListAsync();
        public Task<Table?> GetTableByIdAsync(int tableId) => _db.Tables.FirstOrDefaultAsync(t => t.TableId == tableId);


        public Task<Order?> GetOrderWithDetailsAsync(int orderId) => _db.Orders
        .Include(o => o.OrderDetails)
        .FirstOrDefaultAsync(o => o.OrderId == orderId);


        public Task<bool> OrderHasDishAsync(int orderId, int dishId) => _db.OrderDetails
        .AnyAsync(od => od.OrderId == orderId && od.DishId == dishId);


        public Task<Dish?> GetDishByIdAsync(int dishId) => _db.Dishes.FirstOrDefaultAsync(d => d.DishId == dishId);


        public Task<List<Dish>> GetDishesAsync(int? categoryId)
        {
            var q = _db.Dishes.AsQueryable();
            if (categoryId.HasValue && categoryId.Value != 0)
                q = q.Where(d => d.CategoryId == categoryId.Value);


            return q.OrderBy(d => d.CategoryId).ToListAsync();
        }


        public Task<List<Category>> GetCategoriesAsync() => _db.Categories.OrderBy(c => c.CategoryName).ToListAsync();


        public void AddOrder(Order order) => _db.Orders.Add(order);
        public void AddOrderDetail(OrderDetail detail)
        {
            var newDetail = new OrderDetail
            {
                OrderId = detail.OrderId,
                DishId = detail.DishId,
                Quantity = detail.Quantity,
                Note = detail.Note,
                DishStatus = detail.DishStatus
            };

            _db.OrderDetails.Add(newDetail);
        }

        public void AddOrderDetails(IEnumerable<OrderDetail> details) => _db.OrderDetails.AddRange(details);


        public void UpdateTable(Table table) => _db.Tables.Update(table);


        public Task SaveChangesAsync() => _db.SaveChangesAsync();
    }
}