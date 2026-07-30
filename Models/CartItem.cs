namespace SmartKart.Models
{
    public class CartItem
    {
        public long CartId { get; set; }

        public long ProductId { get; set; }

        public string Name { get; set; } = "";

        public int Price { get; set; }

        public string Image { get; set; } = "";

        public int Quantity { get; set; } = 1;
        public int Stock { get; set; }

        public int Total
        {
            get
            {
                return Price * Quantity;
            }
        }
    }
}