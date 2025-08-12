using Microsoft.AspNetCore.Mvc;
using OrderFood_SW.Helper;
using System.Security.Cryptography;
using System.Text;

namespace OrderFood_SW.Controllers
{
    public class AccountController : Controller
    {
        private readonly DatabaseHelperEF _db;

        public AccountController(DatabaseHelperEF db)
        {
            _db = db;
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please input all info";
                return View();
            }

            // Hash mật khẩu nhập vào
            string passwordHash = HashPassword(password);

            // Kiểm tra trong CSDL
            var user = _db.Users.FirstOrDefault(u => u.Username == username && u.PasswordHash == passwordHash && u.IsActive);

            if (user != null && (user.Role.Equals("Admin") || user.Role.Equals("Staff")))
            {
                // Lưu session
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("FullName", user.FullName);
                HttpContext.Session.SetString("Role", user.Role);

                return RedirectToAction("Index", "Home"); // Hoặc Dashboard
            }else if (user != null && user.Role.Equals("Customer"))
            {
                // Lưu session
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("FullName", user.FullName);
                HttpContext.Session.SetString("Role", user.Role);

                return RedirectToAction("Index", "CustomerOrder"); // Hoặc Dashboard
            }
                ViewBag.Error = "User name or password are incorrect";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }
    }
}
