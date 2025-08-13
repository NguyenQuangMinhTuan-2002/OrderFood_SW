namespace OrderFood_SW.ViewModels
{
    public class OrderHistoryViewModel
    {
        public int OrderId { get; set; }
        public DateTime OrderTime { get; set; }
        public string OrderStatus { get; set; }
        public decimal TotalAmount { get; set; }
        public string Note { get; set; }
        public List<OrderHistoryDetailViewModel> OrderDetails { get; set; }
    }

    public class OrderHistoryDetailViewModel
    {
        public string DishName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
