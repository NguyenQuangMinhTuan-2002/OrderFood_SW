using Microsoft.EntityFrameworkCore;
using OrderFood_SW.Helper;
using OrderFood_SW.Models;

namespace OrderFood_SW.Repositories
{
    public class AccountRepository
    {
        private readonly DatabaseHelperEF _db;
        public AccountRepository(DatabaseHelperEF db) => _db = db;

        public async Task<Users?> GetUserAsync(string username, string passwordHash)
        {
            return await _db.Users
                .FirstOrDefaultAsync(u => u.Username == username
                                       && u.PasswordHash == passwordHash
                                       && u.IsActive);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _db.Users.AnyAsync(u => u.Username == username);
        }
    }
}
