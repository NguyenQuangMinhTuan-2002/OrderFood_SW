using System.ComponentModel.DataAnnotations;

namespace OrderFood_SW.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;
        
        [Required]
        public string SenderId { get; set; } = string.Empty;
        
        [Required]
        public string SenderName { get; set; } = string.Empty;
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedDate { get; set; }
        
        public bool IsRead { get; set; } = false;
        
        public bool IsActive { get; set; } = true;
        
        public string Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent
        
        public string Type { get; set; } = "General"; // General, Warning, Info, Error
    }
}
