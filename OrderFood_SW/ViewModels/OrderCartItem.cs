namespace OrderFood_SW.ViewModels
{
    public class OrderCartItem
    {
        public int OrderDetailId { get; set; }

        public int DishId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string DishName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Note { get; set; } = string.Empty;
        public decimal Total => Price * Quantity;
    }
}
