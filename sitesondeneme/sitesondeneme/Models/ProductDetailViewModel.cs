namespace sitesondeneme.Models;
using sitesondeneme.Models;

public class ProductDetailViewModel
{
    public Product Product { get; set; } = null!;
    public List<Comment> Comments { get; set; } = new List<Comment>();

    public double AverageRating { get; set; } 
    public int CommentCount { get; set; }   
    public List<Product> RelatedProducts { get; set; }
}
