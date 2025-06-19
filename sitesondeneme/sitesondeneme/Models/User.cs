namespace sitesondeneme.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsSeller { get; set; }
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public string? ProfileImagePath { get; set; }


    }

}
