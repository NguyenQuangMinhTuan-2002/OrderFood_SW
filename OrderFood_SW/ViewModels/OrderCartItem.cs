namespace OrderFood_SW.ViewModels
{
    public class OrderCartItem
    {
        public int DishId { get; set; }
        public string ImageUrl { get; set; }
        public string DishName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity;
    }
}
