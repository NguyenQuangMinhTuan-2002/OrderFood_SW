namespace OrderFood_SW.Models
{
    public class Dish
    {
        public int DishId { get; set; }
        public string DishName { get; set; }
        public string DishDescription { get; set; }
        public float DishPrice { get; set; }
        public string? ImageUrl { get; set; }
        public int CategoryId { get; set; }
        public bool IsAvailable { get; set; }
    }
}
