using OrderFood_SW.Models;

namespace OrderFood_SW.ViewModels
{
    public class OrderPageModel
    {
        public string SearchKeyword { get; set; }
        public int SelectedCategoryId { get; set; } = 0;

        public List<Dish> FoundDishes { get; set; } = new List<Dish>();
        public List<Table> FoundTables { get; set; } = new List<Table>();
        public List<Order> FoundOrders { get; set; } = new List<Order>();
        public List<Category> DishCategories { get; set; } = new List<Category>();
        public List<OrderCartItem> CartItems { get; set; } = new List<OrderCartItem>();
    }
}
