using OrderFood_SW.Models;
using OrderFood_SW.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace OrderFood_SW.Services
{
    public class AccountService
    {
        private readonly AccountRepository _repo;
        private readonly UserRepository _userRepo;
        
        public AccountService(AccountRepository repo, UserRepository userRepo)
        {
            _repo = repo;
            _userRepo = userRepo;
        }

        public async Task<Users?> AuthenticateAsync(string username, string password)
        {
            var hash = HashPassword(password);
            return await _repo.GetUserAsync(username, hash);
        }

        public async Task<bool> RegisterCustomerAsync(string username, string password, string fullName, string email)
        {
            // Check if username already exists
            if (await _repo.UsernameExistsAsync(username))
                return false;

            var user = new Users
            {
                Username = username,
                PasswordHash = HashPassword(password),
                FullName = fullName,
                Email = email,
                Role = "Customer",
                IsActive = true,
                ImageAvat = "nophoto1.png"
            };

            await _userRepo.AddAsync(user);
            return true;
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }
}
