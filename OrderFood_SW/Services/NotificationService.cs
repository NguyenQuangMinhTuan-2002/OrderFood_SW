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

        public int GetUnreadNotificationCount(int userId)
        {
            return _repo.GetUnreadNotificationCount(userId);
        }

        public List<Notification> GetUnreadNotifications(int userId)
        {
            return _repo.GetUnreadNotifications(userId);
        }

        public (bool Success, string Message) CreateNotification(string title, string content, string senderId, string senderName, string priority = "Normal", string type = "General")
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(title))
                    return (false, "Title cannot be empty.");
                
                if (string.IsNullOrWhiteSpace(content))
                    return (false, "Content cannot be empty.");
                
                if (string.IsNullOrWhiteSpace(senderId))
                    return (false, "SenderId cannot be empty.");
                
                if (string.IsNullOrWhiteSpace(senderName))
                    return (false, "SenderName cannot be empty.");

                var notification = new Notification
                {
                    Title = title.Trim(),
                    Content = content.Trim(),
                    SenderId = senderId.Trim(),
                    SenderName = senderName.Trim(),
                    Priority = priority ?? "Normal",
                    Type = type ?? "General",
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };

                _repo.AddNotification(notification);
                _repo.SaveChanges();

                return (true, "Notification has been created !.");
            }
            catch (Exception ex)
            {
                // Log the full exception for debugging
                System.Diagnostics.Debug.WriteLine($"Error creating notification: {ex}");
                return (false, $"Error creating notification: {ex.Message}");
            }
        }

        public (bool Success, string Message) UpdateNotification(int id, string title, string content, string priority = "Normal", string type = "General")
        {
            try
            {
                var notification = _repo.GetNotificationById(id);
                if (notification == null)
                    return (false, "Notification not found.");

                notification.Title = title;
                notification.Content = content;
                notification.Priority = priority;
                notification.Type = type;
                notification.UpdatedDate = DateTime.Now;

                _repo.UpdateNotification(notification);
                _repo.SaveChanges();

                return (true, "Notification updated successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating notification: {ex.Message}");
            }
        }

        public (bool Success, string Message) DeleteNotification(int id)
        {
            try
            {
                var notification = _repo.GetNotificationById(id);
                if (notification == null)
                    return (false, "Notification not found.");

                _repo.DeleteNotification(id);
                _repo.SaveChanges();

                return (true, "Notification deleted successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting notification: {ex.Message}");
            }
        }

        public (bool Success, string Message) MarkAsRead(int notificationId, int userId)
        {
            try
            {
                var notification = _repo.GetNotificationById(notificationId);
                if (notification == null)
                    return (false, "Notification not found.");

                _repo.MarkAsRead(notificationId, userId);
                _repo.SaveChanges();

                return (true, "Notification marked as read.");
            }
            catch (Exception ex)
            {
                return (false, $"Error marking notification: {ex.Message}");
            }
        }


        public (bool Success, string Message) MarkAllAsRead(int userId)
        {
            try
            {
                _repo.MarkAllAsRead(userId);
                _repo.SaveChanges();

                return (true, "All notifications marked as read.");
            }
            catch (Exception ex)
            {
                return (false, $"Error marking all notifications: {ex.Message}");
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

        public bool IsNotificationReadByUser(int notificationId, int userId)
        {
            return _repo.IsNotificationReadByUser(notificationId, userId);
        }
    }
}
