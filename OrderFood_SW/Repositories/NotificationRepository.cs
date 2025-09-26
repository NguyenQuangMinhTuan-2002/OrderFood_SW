using Microsoft.EntityFrameworkCore;
using OrderFood_SW.Helper;
using OrderFood_SW.Models;

namespace OrderFood_SW.Repositories
{
    public class NotificationRepository
    {
        private readonly DatabaseHelperEF _db;

        public NotificationRepository(DatabaseHelperEF db)
        {
            _db = db;
        }

        public List<Notification> GetAllNotifications()
        {
            return _db.Notifications
                      .Where(n => n.IsActive)
                      .OrderByDescending(n => n.CreatedDate)
                      .ToList();
        }

        public List<Notification> GetNotificationsForAdmin()
        {
            return _db.Notifications
                      .Where(n => n.IsActive)
                      .OrderByDescending(n => n.CreatedDate)
                      .ToList();
        }

        public List<Notification> GetNotificationsBySender(string senderId)
        {
            return _db.Notifications
                      .Where(n => n.SenderId == senderId && n.IsActive)
                      .OrderByDescending(n => n.CreatedDate)
                      .ToList();
        }

        public Notification? GetNotificationById(int id)
        {
            return _db.Notifications.FirstOrDefault(n => n.Id == id);
        }

        public int GetUnreadNotificationCount()
        {
            return _db.Notifications.Count(n => !n.IsRead && n.IsActive);
        }

        public List<Notification> GetUnreadNotifications()
        {
            return _db.Notifications
                      .Where(n => !n.IsRead && n.IsActive)
                      .OrderByDescending(n => n.CreatedDate)
                      .ToList();
        }

        public void AddNotification(Notification notification)
        {
            try
            {
                _db.Notifications.Add(notification);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding notification to context: {ex}");
                throw;
            }
        }

        public void UpdateNotification(Notification notification)
        {
            notification.UpdatedDate = DateTime.Now;
            _db.Notifications.Update(notification);
        }

        public void DeleteNotification(int id)
        {
            var notification = _db.Notifications.FirstOrDefault(n => n.Id == id);
            if (notification != null)
            {
                notification.IsActive = false;
                _db.Notifications.Update(notification);
            }
        }

        public void MarkAsRead(int notificationId, int userId)
        {
            // Kiểm tra xem đã đọc chưa
            var alreadyRead = _db.NotificationReads
                .FirstOrDefault(r => r.NotificationId == notificationId && r.UserId == userId);

            if (alreadyRead == null)
            {
                // Thêm vào bảng NotificationReads
                var read = new NotificationReads
                {
                    NotificationId = notificationId,
                    UserId = userId,
                    ReadDate = DateTime.Now
                };
                _db.NotificationReads.Add(read);

                // Cập nhật trạng thái IsRead trong bảng Notifications
                var notification = _db.Notifications.FirstOrDefault(n => n.Id == notificationId);
                if (notification != null)
                {
                    notification.IsRead = true;
                    notification.UpdatedDate = DateTime.Now;
                    _db.Notifications.Update(notification);
                }
            }
        }


        public void MarkAllAsRead()
        {
            var unreadNotifications = _db.Notifications.Where(n => !n.IsRead && n.IsActive);
            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }
            _db.Notifications.UpdateRange(unreadNotifications);
        }

        public List<Notification> GetPagedNotifications(int page, int pageSize)
        {
            return _db.Notifications
                      .Where(n => n.IsActive)
                      .OrderByDescending(n => n.CreatedDate)
                      .Skip((page - 1) * pageSize)
                      .Take(pageSize)
                      .ToList();
        }

        public int GetTotalNotificationCount()
        {
            return _db.Notifications.Count(n => n.IsActive);
        }

        public void SaveChanges()
        {
            try
            {
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving changes: {ex}");
                throw;
            }
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
