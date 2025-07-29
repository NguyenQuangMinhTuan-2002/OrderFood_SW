using System.ComponentModel.DataAnnotations;

namespace OrderFood_SW.Models
{
    public class Table
    {
        [Key]
        public int TableId { get; set; }
        public int TableNumber { get; set; }
        public string QRCode { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
    }
}
