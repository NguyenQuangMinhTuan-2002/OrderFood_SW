using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderFood_SW.Models
{
    public class NotificationReads
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int NotificationId { get; set; }

        public int? UserId { get; set; }
        public DateTime? ReadDate { get; set; }

        [ForeignKey(nameof(NotificationId))]
        public Notification Notification { get; set; } = null!;
    }
}
