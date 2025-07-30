using OrderFood_SW.Models;
using System.ComponentModel.DataAnnotations;

public class OrderDetailViewModel
    //Cấu trúc nối 2 bảng order và order details (Object A cho 1-1 và List<B> cho 1-N)
{
    public Order Order { get; set; }
    public List<DetailsWithDish> OrderDetails { get; set; }
}

public class DetailsWithDish
{
    [Key]
    public int DishId { get; set; }
    public string ImageUrl { get; set; }
    public string DishName { get; set; }
    public int Quantity { get; set; }
    public decimal DishPrice { get; set; }
    public int DishStatus { get; set; } // 0: Đang chờ, 1: Đã hoàn thành
    public int OrderId { get; set; }
    public decimal Total => DishPrice * Quantity;
}
