using Microsoft.AspNetCore.Mvc;
using OrderFood_SW.Helper;
using OrderFood_SW.Services;
using OrderFood_SW.Models;

namespace OrderFood_SW.Controllers
{
    [AuthorizeRole("Admin", "Staff")]
    public class NotificationController : Controller
    {
        private const int PageSize = 10;
        private readonly NotificationService _service;

        public NotificationController(NotificationService service)
        {
            _service = service;
        }

        // Trang chính - hiển thị danh sách thông báo
        public IActionResult Index(int page = 1)
        {
            var role = HttpContext.Session.GetString("Role");
            List<Notification> notifications;

            if (role == "Admin")
            {
                // Admin xem tất cả thông báo
                notifications = _service.GetNotificationsForAdmin();
            }
            else
            {
                // Staff chỉ xem thông báo của mình
                var userId = HttpContext.Session.GetInt32("UserId");
                notifications = _service.GetNotificationsBySender(userId?.ToString() ?? "");
            }

            var (pagedNotifications, totalPages) = _service.GetPagedNotifications(page, PageSize);
            
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Role = role;

            return View(pagedNotifications);
        }

        // Tạo thông báo mới (chỉ Staff)
        [AuthorizeRole("Staff")]
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var fullName = HttpContext.Session.GetString("FullName");

            var notification = new Notification
            {
                SenderId = userId?.ToString() ?? "",
                SenderName = fullName ?? "",
                Priority = "Normal",
                Type = "General"
            };

            return View(notification);
        }

        [HttpPost]
        [AuthorizeRole("Staff")]
        public IActionResult Create(Notification notification)
        {
            try
            {
                // Lấy thông tin user từ session trước khi validate
                var userId = HttpContext.Session.GetInt32("UserId");
                var fullName = HttpContext.Session.GetString("FullName");

                if (userId == null || string.IsNullOrEmpty(fullName))
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng. Vui lòng đăng nhập lại.";
                    return View(notification);
                }

                // Set SenderId và SenderName từ session
                notification.SenderId = userId.ToString();
                notification.SenderName = fullName;

                // Validate model sau khi set SenderId và SenderName
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
                    return View(notification);
                }

                var result = _service.CreateNotification(
                    notification.Title,
                    notification.Content,
                    userId.ToString(),
                    fullName,
                    notification.Priority,
                    notification.Type
                );

                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                    return View(notification);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi hệ thống: {ex.Message}";
                return View(notification);
            }
        }

        // Chỉnh sửa thông báo (chỉ Staff và chỉ thông báo của mình)
        [AuthorizeRole("Staff")]
        public IActionResult Edit(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var notification = _service.GetNotificationById(id);

            if (notification == null || notification.SenderId != userId?.ToString())
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông báo hoặc bạn không có quyền chỉnh sửa.";
                return RedirectToAction("Index");
            }

            return View(notification);
        }

        [HttpPost]
        [AuthorizeRole("Staff")]
        public IActionResult Edit(int id, Notification notification)
        {
            if (!ModelState.IsValid)
            {
                return View(notification);
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            var existingNotification = _service.GetNotificationById(id);

            if (existingNotification == null || existingNotification.SenderId != userId?.ToString())
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông báo hoặc bạn không có quyền chỉnh sửa.";
                return RedirectToAction("Index");
            }

            var result = _service.UpdateNotification(
                id,
                notification.Title,
                notification.Content,
                notification.Priority,
                notification.Type
            );

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("Index");
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
                return View(notification);
            }
        }

        // Xóa thông báo (chỉ Staff và chỉ thông báo của mình)
        [AuthorizeRole("Staff")]
        public IActionResult Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var notification = _service.GetNotificationById(id);

            if (notification == null || notification.SenderId != userId?.ToString())
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông báo hoặc bạn không có quyền xóa.";
                return RedirectToAction("Index");
            }

            return View(notification);
        }

        [HttpPost]
        [AuthorizeRole("Staff")]
        public IActionResult DeleteConfirmed(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var notification = _service.GetNotificationById(id);

            if (notification == null || notification.SenderId != userId?.ToString())
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông báo hoặc bạn không có quyền xóa.";
                return RedirectToAction("Index");
            }

            var result = _service.DeleteNotification(id);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction("Index");
        }

        // Chi tiết thông báo
        public IActionResult Details(int id)
        {
            var notification = _service.GetNotificationById(id);
            
            if (notification == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông báo.";
                return RedirectToAction("Index");
            }

            // Đánh dấu là đã đọc nếu chưa đọc
            if (!notification.IsRead)
            {
                _service.MarkAsRead(id);
            }

            return View(notification);
        }

        // Đánh dấu thông báo là đã đọc
        [HttpPost]
        public IActionResult MarkAsRead(int id)
        {
            var result = _service.MarkAsRead(id);
            return Json(new { success = result.Success, message = result.Message });
        }

        // Đánh dấu tất cả thông báo là đã đọc
        [HttpPost]
        public IActionResult MarkAllAsRead()
        {
            var result = _service.MarkAllAsRead();
            return Json(new { success = result.Success, message = result.Message });
        }

        // API để lấy số lượng thông báo chưa đọc (cho badge)
        [HttpGet]
        public IActionResult GetUnreadCount()
        {
            var count = _service.GetUnreadNotificationCount();
            return Json(new { count = count });
        }

        // API để lấy danh sách thông báo chưa đọc (cho dropdown)
        [HttpGet]
        public IActionResult GetUnreadNotifications()
        {
            var notifications = _service.GetUnreadNotifications();
            return Json(notifications);
        }

        // Lấy thông báo gần đây
        [HttpGet]
        public IActionResult GetRecentNotifications(int count = 5)
        {
            var notifications = _service.GetRecentNotifications(count);
            return Json(notifications);
        }

        // Test action để kiểm tra hệ thống
        [HttpGet]
        public IActionResult Test()
        {
            try
            {
                var testResult = _service.CreateNotification(
                    "Test Notification",
                    "This is a test notification to check if the system is working.",
                    "test-user",
                    "Test User",
                    "Normal",
                    "Info"
                );

                return Json(new { 
                    success = testResult.Success, 
                    message = testResult.Message,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = ex.Message,
                    timestamp = DateTime.Now
                });
            }
        }
    }
}
