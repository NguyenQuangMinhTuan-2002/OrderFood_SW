using OrderFood_SW.Models;
using System.ComponentModel.DataAnnotations;

public class OrderDetailViewModel
    //Cấu trúc nối 2 bảng order và order details (Object A cho 1-1 và List<B> cho 1-N)
{
    public Order Order { get; set; } = new Order();
    public List<DetailsWithDish> OrderDetails { get; set; } = new List<DetailsWithDish>();
}

public class DetailsWithDish
{
    public int OrderDetailId { get; set; }
    public int DishId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string DishName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal TaxRate { get; set; } = 0.1m; // Default tax rate of 10%
    public decimal DishPrice { get; set; }
    public int DishStatus { get; set; } // 0: Đang chờ, 1: Đã hoàn thành
    public int OrderId { get; set; }
    public string Note { get; set; } = string.Empty;
    public decimal Total => DishPrice * Quantity + (DishPrice * Quantity * TaxRate);
}
