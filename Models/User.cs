using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartKart.Models
{
    [Table("Customers")]
    public class User
    {
        [Key]
        [Column("Id")]
        public long Id { get; set; }

        [Column("Name")]
        public string Name { get; set; } = "";

        [Column("Email")]
        public string Email { get; set; } = "";

        [Column("Phone")]
        public string Phone { get; set; } = "";

        [Column("Password")]
        public string Password { get; set; } = "";
    }
}