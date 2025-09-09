using Microsoft.EntityFrameworkCore;
using OrderFood_SW.Helper;
using OrderFood_SW.Models;

namespace OrderFood_SW.Repositories
{
    public class UserRepository
    {
        private readonly DatabaseHelperEF _db;
        public UserRepository(DatabaseHelperEF db) => _db = db;

        public async Task<(List<Users> Users, int TotalRows)> GetPagedAsync(string keyword, int page, int pageSize)
        {
            var query = _db.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(c => c.FullName.Contains(keyword));

            int totalRows = await query.CountAsync();
            var users = await query.OrderBy(c => c.UserId)
                                   .Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();

            return (users, totalRows);
        }

        public Task<Users?> GetByIdAsync(int id) =>
            _db.Users.FirstOrDefaultAsync(u => u.UserId == id);

        public async Task AddAsync(Users user)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Users user)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Users user)
        {
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
        }

        public Task<bool> ExistsAsync(int id) =>
            _db.Users.AnyAsync(u => u.UserId == id);
    }
}
