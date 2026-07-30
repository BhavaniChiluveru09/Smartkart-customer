using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartKart.Models
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        public long Id { get; set; }

        public string UserEmail { get; set; } = "";

        public long ProductId { get; set; }

        public int Quantity { get; set; }

        public string Address { get; set; } = "";

        public DateTime OrderDate { get; set; }


        // ✅ New custom order id
        public string OrderCode { get; set; } = string.Empty;

        public string OrderStatus { get; set; } = "Created";
        public bool IsCancelled { get; set; } = false;
        public bool IsRefunded { get; set; } = false;


    }
}