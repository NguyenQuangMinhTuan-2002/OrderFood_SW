using System.ComponentModel.DataAnnotations;

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
    }
}
