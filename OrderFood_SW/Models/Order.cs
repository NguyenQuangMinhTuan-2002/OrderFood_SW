namespace OrderFood_SW.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int TableId { get; set; }
        public DateTime OrderTime { get; set; }
        public int OrderStatus { get; set; }
        public float TotalAmount { get; set; }
        public string note { get; set; }
    }
}
