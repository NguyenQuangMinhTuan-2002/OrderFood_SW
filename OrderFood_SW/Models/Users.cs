using System.ComponentModel.DataAnnotations;

namespace OrderFood_SW.Models
{
    public class Users
    {
        [Key]
        public int UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public Boolean IsActive { get; set; }
    }
}
