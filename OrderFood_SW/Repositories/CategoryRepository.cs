using Microsoft.EntityFrameworkCore;
using OrderFood_SW.Helper;
using OrderFood_SW.Models;

namespace OrderFood_SW.Repositories
{
    public class CategoryRepository
    {
        private readonly DatabaseHelperEF _db;

        public CategoryRepository(DatabaseHelperEF db)
        {
            _db = db;
        }

        public async Task<int> CountAsync(string keyword = "")
        {
            var query = _db.Categories.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(c =>
                    c.CategoryName.Contains(keyword) ||
                    c.CategoryDescription.Contains(keyword));
            }
            return await query.CountAsync();
        }

        public async Task<List<Category>> GetPagedAsync(string keyword, int skip, int pageSize)
        {
            var query = _db.Categories.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(c =>
                    c.CategoryName.Contains(keyword) ||
                    c.CategoryDescription.Contains(keyword));
            }

            return await query
                .OrderBy(c => c.CategoryId)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _db.Categories.FindAsync(id);
        }

        public async Task AddAsync(Category category)
        {
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Category category)
        {
            _db.Update(category);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Category category)
        {
            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();
        }
    }
}
