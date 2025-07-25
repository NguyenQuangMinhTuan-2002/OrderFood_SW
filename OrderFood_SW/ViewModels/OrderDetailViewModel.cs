using OrderFood_SW.Models;

public class OrderDetailViewModel
{
    public Order Order { get; set; }
    public List<OrderDetailsWithDish> OrderDetails { get; set; }
}

public class OrderDetailsWithDish
{
    public int DishId { get; set; }
    public string DishName { get; set; }
    public int Quantity { get; set; }
    public decimal DishPrice { get; set; }
    public decimal Total => DishPrice * Quantity;
}
