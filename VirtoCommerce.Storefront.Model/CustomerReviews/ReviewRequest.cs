namespace VirtoCommerce.Storefront.Model.CustomerReviews
{
    public class ReviewRequest
    {
        public string Content { get; set; }
        public int Rating { get; set; }
        public string ProductId { get; set; }
    }
}
