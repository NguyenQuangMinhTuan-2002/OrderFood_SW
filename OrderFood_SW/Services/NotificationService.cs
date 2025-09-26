using OrderFood_SW.Models;
using OrderFood_SW.Repositories;

namespace OrderFood_SW.Services
{
    public class NotificationService
    {
        private readonly NotificationRepository _repo;

        public NotificationService(NotificationRepository repo)
        {
            _repo = repo;
        }

        public List<Notification> GetAllNotifications()
        {
            return _repo.GetAllNotifications();
        }

        public List<Notification> GetNotificationsForAdmin()
        {
            return _repo.GetNotificationsForAdmin();
        }

        public List<Notification> GetNotificationsBySender(string senderId)
        {
            return _repo.GetNotificationsBySender(senderId);
        }

        public Notification? GetNotificationById(int id)
        {
            return _repo.GetNotificationById(id);
        }

        public int GetUnreadNotificationCount()
        {
            return _repo.GetUnreadNotificationCount();
        }

        public List<Notification> GetUnreadNotifications()
        {
            return _repo.GetUnreadNotifications();
        }

        public (bool Success, string Message) CreateNotification(string title, string content, string senderId, string senderName, string priority = "Normal", string type = "General")
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(title))
                    return (false, "Tiêu đề không được để trống.");
                
                if (string.IsNullOrWhiteSpace(content))
                    return (false, "Nội dung không được để trống.");
                
                if (string.IsNullOrWhiteSpace(senderId))
                    return (false, "SenderId không được để trống.");
                
                if (string.IsNullOrWhiteSpace(senderName))
                    return (false, "SenderName không được để trống.");

                var notification = new Notification
                {
                    Title = title.Trim(),
                    Content = content.Trim(),
                    SenderId = senderId.Trim(),
                    SenderName = senderName.Trim(),
                    Priority = priority ?? "Normal",
                    Type = type ?? "General",
                    CreatedDate = DateTime.Now,
                    IsRead = false,
                    IsActive = true
                };

                _repo.AddNotification(notification);
                _repo.SaveChanges();

                return (true, "Thông báo đã được tạo thành công.");
            }
            catch (Exception ex)
            {
                // Log the full exception for debugging
                System.Diagnostics.Debug.WriteLine($"Error creating notification: {ex}");
                return (false, $"Lỗi khi tạo thông báo: {ex.Message}");
            }
        }

        public (bool Success, string Message) UpdateNotification(int id, string title, string content, string priority = "Normal", string type = "General")
        {
            try
            {
                var notification = _repo.GetNotificationById(id);
                if (notification == null)
                    return (false, "Không tìm thấy thông báo.");

                notification.Title = title;
                notification.Content = content;
                notification.Priority = priority;
                notification.Type = type;
                notification.UpdatedDate = DateTime.Now;

                _repo.UpdateNotification(notification);
                _repo.SaveChanges();

                return (true, "Thông báo đã được cập nhật thành công.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi cập nhật thông báo: {ex.Message}");
            }
        }

        public (bool Success, string Message) DeleteNotification(int id)
        {
            try
            {
                var notification = _repo.GetNotificationById(id);
                if (notification == null)
                    return (false, "Không tìm thấy thông báo.");

                _repo.DeleteNotification(id);
                _repo.SaveChanges();

                return (true, "Thông báo đã được xóa thành công.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi xóa thông báo: {ex.Message}");
            }
        }

        public (bool Success, string Message) MarkAsRead(int id)
        {
            try
            {
                var notification = _repo.GetNotificationById(id);
                if (notification == null)
                    return (false, "Không tìm thấy thông báo.");

                _repo.MarkAsRead(id);
                _repo.SaveChanges();

                return (true, "Thông báo đã được đánh dấu là đã đọc.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi đánh dấu thông báo: {ex.Message}");
            }
        }

        public (bool Success, string Message) MarkAllAsRead()
        {
            try
            {
                _repo.MarkAllAsRead();
                _repo.SaveChanges();

                return (true, "Tất cả thông báo đã được đánh dấu là đã đọc.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi đánh dấu tất cả thông báo: {ex.Message}");
            }
        }

        public (List<Notification> Notifications, int TotalPages) GetPagedNotifications(int page, int pageSize)
        {
            var notifications = _repo.GetPagedNotifications(page, pageSize);
            int totalCount = _repo.GetTotalNotificationCount();
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return (notifications, totalPages);
        }

        public int GetTotalNotificationCount()
        {
            return _repo.GetTotalNotificationCount();
        }

        public List<Notification> GetRecentNotifications(int count = 5)
        {
            return _repo.GetAllNotifications().Take(count).ToList();
        }

        public List<Notification> GetNotificationsByPriority(string priority)
        {
            return _repo.GetAllNotifications()
                       .Where(n => n.Priority == priority)
                       .ToList();
        }

        public List<Notification> GetNotificationsByType(string type)
        {
            return _repo.GetAllNotifications()
                       .Where(n => n.Type == type)
                       .ToList();
        }
    }
}
