using System.ComponentModel.DataAnnotations;

namespace OrderFood_SW.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }
        public int TableId { get; set; }
        public DateTime OrderTime { get; set; }
        public int OrderStatus { get; set; }
        public decimal TotalAmount { get; set; }
        public string note { get; set; }
        public int? UserId { get; set; } // Nullable to allow for orders without a user context

        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
