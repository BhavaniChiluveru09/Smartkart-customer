using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartKart.Models
{
    [Table("Product")]
    public class Product
    {
        [Key]
        [Column("Id")]
        public long Id { get; set; }

        [Column("Name")]
        public string Name { get; set; } = "";

        [Column("Description")]
        public string? Description { get; set; } = "";

        [Column("Price")]
        public int Price { get; set; }

        [Column("Image")]
        public string Image { get; set; } = "";

        [Column("Stock")]
        public int Stock { get; set; }

        [Column("Category")]
        public string Category { get; set; } = "";

        public string? SubCategory { get; set; }
    }
}