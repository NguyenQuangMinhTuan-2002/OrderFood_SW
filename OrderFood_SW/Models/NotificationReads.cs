using System.ComponentModel.DataAnnotations;

namespace OrderFood_SW.Models
{
    public class NotificationReads
    {
        [Key]
        public int Id { get; set; }
        public int NotificationId { get; set; }
        public int UserId { get; set; }
        public DateTime ReadDate { get; set; } = DateTime.Now;

        public Notification Notification { get; set; } = null!;
    }
}
