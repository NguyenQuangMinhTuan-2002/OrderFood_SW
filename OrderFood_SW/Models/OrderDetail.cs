using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderFood_SW.Models
{
    public class OrderDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderDetailId { get; set; }

        public int OrderId { get; set; }
        public int DishId { get; set; }
        public int Quantity { get; set; }
        public int DishStatus { get; set; } // 0: Đang chờ, 1: Đã hoàn thành
        public string? Note { get; set; }
        public decimal? TaxRate { get; set; }
        public decimal? TaxAmount { get; set; }

        // Navigation properties
        public Order? Order { get; set; }
        public Dish? Dish { get; set; }
    }
}
