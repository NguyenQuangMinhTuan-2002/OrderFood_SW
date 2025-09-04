namespace OrderFood_SW.ViewModels
{
    public class RevenueViewModel
    {
        public string Period { get; set; } = "day";
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AvgOrderValue { get; set; }
        public List<TopDishViewModel> TopDishes { get; set; } = new();
        public Dictionary<string, decimal> ShiftRevenue { get; set; } = new();
    }

    public class TopDishViewModel
    {
        public string DishName { get; set; }
        public int Quantity { get; set; }
        public decimal Revenue { get; set; }
    }


}
