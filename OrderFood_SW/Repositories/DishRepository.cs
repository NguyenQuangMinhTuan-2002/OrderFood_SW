using Microsoft.EntityFrameworkCore;
using OrderFood_SW.Helper;
using OrderFood_SW.Models;

namespace OrderFood_SW.Repositories
{
    public class DishRepository
    {
        private readonly DatabaseHelperEF _db;

        public DishRepository(DatabaseHelperEF db)
        {
            _db = db;
        }

        public async Task<(List<Dish>, int totalRows)> GetPagedAsync(string keyword, int page, int pageSize)
        {
            var query = _db.Dishes.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(d =>
                    d.DishName.Contains(keyword) ||
                    d.DishDescription.Contains(keyword) ||
                    d.DishPrice.ToString().Contains(keyword));
            }

            int totalRows = await query.CountAsync();

            var dishes = await query
                .OrderBy(d => d.DishId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (dishes, totalRows);
        }

        public async Task<Dish?> GetByIdAsync(int id)
        {
            return await _db.Dishes.FindAsync(id);
        }

        public async Task AddAsync(Dish dish)
        {
            _db.Dishes.Add(dish);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Dish dish)
        {
            _db.Dishes.Update(dish);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Dish dish)
        {
            _db.Dishes.Remove(dish);
            await _db.SaveChangesAsync();
        }
    }
}
