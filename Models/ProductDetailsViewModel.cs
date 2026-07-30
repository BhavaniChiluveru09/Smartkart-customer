namespace SmartKart.Models
{
    public class ProductDetailsViewModel
    {
        public Product Product { get; set; }
        public string RecommendationTitle { get; set; } = "";

        public List<Product> RecommendedProducts { get; set; } = new List<Product>();
    }
}