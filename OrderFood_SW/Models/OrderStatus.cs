using System.ComponentModel.DataAnnotations;

namespace OrderFood_SW.Models
{
    public class OrderStatus
    {
        [Key]
        public int Status { get; set; }
        public string StatusDescription { get; set; }
    }
}
