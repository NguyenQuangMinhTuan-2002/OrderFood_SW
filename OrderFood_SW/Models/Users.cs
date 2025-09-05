using System.ComponentModel.DataAnnotations;

namespace OrderFood_SW.Models
{
    public class Users
    {
        [Key]
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ImageAvat { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public Boolean IsActive { get; set; }
    }
}
