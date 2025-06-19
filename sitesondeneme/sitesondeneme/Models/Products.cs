using System.Collections.Generic;

namespace sitesondeneme.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public string? ImagePath { get; set; }

        public required string Category { get; set; }

        public string? ImagePath2 { get; set; }

        public string? ImagePath3 { get; set; }

        public string? ImagePath4 { get; set; }
        public string? Description { get; set; }
        public int SellerId { get; set; }
        public User? Seller { get; set; }
        public string? Username { get; set; } 



        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
