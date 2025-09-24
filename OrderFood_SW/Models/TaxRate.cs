using System.ComponentModel.DataAnnotations;

namespace OrderFood_SW.Models
{
    public class TaxRate
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [Range(0, 1, ErrorMessage = "Tax rate must be between 0 and 1 (0% to 100%)")]
        public decimal Rate { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedDate { get; set; }
    }
}
