using System.ComponentModel.DataAnnotations;

namespace OrderFood_SW.Models
{
    public class Account
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string role { get; set; } = string.Empty;
    }
}
