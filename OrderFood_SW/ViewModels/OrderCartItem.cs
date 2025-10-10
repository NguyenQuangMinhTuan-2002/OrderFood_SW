namespace OrderFood_SW.ViewModels
{
    public class OrderCartItem
    {
        public string CartItemId { get; set; } = Guid.NewGuid().ToString(); // Unique identifier for the cart item

        public int DishId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string DishName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal TaxRate { get; set; } = 0.1m; // Default tax rate of 10%
        public string Note { get; set; } = string.Empty;
        public decimal Total => Price * Quantity;
    }
}
