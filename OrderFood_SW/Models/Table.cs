namespace OrderFood_SW.Models
{
    public class Table
    {
        public int TableId { get; set; }
        public int TableNumber { get; set; }
        public string QRCode { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
    }
}
