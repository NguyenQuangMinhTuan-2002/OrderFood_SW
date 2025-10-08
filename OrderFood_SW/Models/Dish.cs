using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderFood_SW.Models
{
    public class Dish
    {
        [Key]
        public int DishId { get; set; }
        public string DishName { get; set; }
        public string DishDescription { get; set; }
        public decimal DishPrice { get; set; }
        public string? ImageUrl { get; set; }
        public int CategoryId { get; set; }
        public bool IsAvailable { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Tax rate must be a positive number")]
        public decimal? TaxRate { get; set; } = 0.1m;

        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
