using Microsoft.EntityFrameworkCore;
using OrderFood_SW.Helper;
using OrderFood_SW.Models;

namespace OrderFood_SW.Repositories
{
    public class StatisticRepository
    {
        private readonly DatabaseHelperEF _db;

        public StatisticRepository(DatabaseHelperEF db)
        {
            _db = db;
        }

        public IQueryable<Order> GetOrders()
        {
            return _db.Orders
                      .Include(o => o.OrderDetails)
                      .ThenInclude(od => od.Dish);
        }
    }
}
