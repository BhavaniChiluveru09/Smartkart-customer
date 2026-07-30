using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartKart.Models
{
    [Table("Cart")]
    public class Cart
    {
        [Key]
        public long Id { get; set; }

        public string UserEmail { get; set; } = "";

        public long ProductId { get; set; }

        public int Quantity { get; set; }
    }
}