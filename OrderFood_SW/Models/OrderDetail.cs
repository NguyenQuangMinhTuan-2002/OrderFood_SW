using System.ComponentModel.DataAnnotations;

namespace OrderFood_SW.Models
{
    public class OrderDetail
    {
        public int OrderId { get; set; }
        public int DishId { get; set; }
        public int Quantity { get; set; }
        public int DishStatus { get; set; } // 0: Đang chờ, 1: Đã hoàn thành

        // Navigation properties
        public Order Order { get; set; }
        public Dish Dish { get; set; }
    }
}
