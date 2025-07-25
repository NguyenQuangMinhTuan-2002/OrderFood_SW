namespace OrderFood_SW.ViewModels
{
    public class OrderCartItem
    {
        public int DishId { get; set; }
        public string DishName { get; set; }
        public float Price { get; set; }
        public int Quantity { get; set; }
        public float Total => Price * Quantity;
    }
}
