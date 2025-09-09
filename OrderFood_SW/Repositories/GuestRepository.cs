using OrderFood_SW.Helper;
using OrderFood_SW.Models;

namespace OrderFood_SW.Repositories
{
    public class GuestRepository
    {
        private readonly DatabaseHelperEF _db;

        public GuestRepository(DatabaseHelperEF db)
        {
            _db = db;
        }

        public Table? GetTableById(int tableId)
        {
            return _db.Tables.FirstOrDefault(t => t.TableId == tableId);
        }

        public Order? GetOrderById(int orderId)
        {
            return _db.Orders.FirstOrDefault(o => o.OrderId == orderId);
        }

        public Dish? GetDishById(int dishId)
        {
            return _db.Dishes.FirstOrDefault(o => o.DishId == dishId);
        }

        public void UpdateTable(Table table)
        {
            _db.Tables.Update(table);
            _db.SaveChanges();
        }
    }
}
