using System.ComponentModel.DataAnnotations;

namespace OrderFood_SW.Models
{
    public class Table
    {
        [Key]
        public int TableId { get; set; }
        public int TableNumber { get; set; }
        public string QRCode { get; set; } = "n/a";
        public string Status { get; set; } = "n/a";
        public string Description { get; set; } = "n/a";

        // Temp field.
        public int? CurrentOrderId { get; set; }

        // Navigation property
        public virtual ICollection<Order>? Orders { get; set; }
    }
}
