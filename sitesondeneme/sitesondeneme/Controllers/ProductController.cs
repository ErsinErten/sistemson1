using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using sitesondeneme.Models;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace sitesondeneme.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Products(string? category, int page = 1)
        {
            var username = HttpContext.Session.GetString("Username");
            ViewData["Username"] = username;

            var isAdminStr = HttpContext.Session.GetString("IsAdmin");
            bool isAdmin = !string.IsNullOrEmpty(isAdminStr) && bool.Parse(isAdminStr);
            ViewData["IsAdmin"] = isAdmin;

            int pageSize = 9;

            IQueryable<Product> productsQuery = _context.Products;

            if (!string.IsNullOrEmpty(category))
            {
                productsQuery = productsQuery.Where(p => p.Category == category);
                ViewData["CurrentCategory"] = category;
            }
            else
            {
                ViewData["CurrentCategory"] = "Tüm Ürünler";
            }

            int totalProducts = productsQuery.Count();


            List<Product> products = productsQuery
                                        .Skip((page - 1) * pageSize)
                                        .Take(pageSize)
                                        .ToList();

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalProducts / (double)pageSize);

            return View(products);
        }




        public IActionResult ProductDetails(int id)
        {
            var product = _context.Products
                            .Include(p => p.Seller)
                            .FirstOrDefault(p => p.Id == id);

            if (product == null)
                return NotFound();

            var comments = _context.Comments.Where(c => c.ProductId == id).ToList();

            double averageRating = 0;
            if (comments.Any())
                averageRating = comments.Average(c => c.Rating);

            var relatedProducts = _context.Products
                .Where(p => p.Category == product.Category && p.Id != id)
                .OrderByDescending(p => p.Id)
                .Take(4)
                .ToList();

            var viewModel = new ProductDetailViewModel
            {
                Product = product,
                Comments = comments,
                AverageRating = averageRating,
                CommentCount = comments.Count,
                RelatedProducts = relatedProducts
            };

            var username = HttpContext.Session.GetString("Username");
            ViewData["Username"] = username;

            return View(viewModel);
        }



        [HttpPost]
        public IActionResult AddComment(int productId, string text, int rating)
        {
            var username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Account", "Account");

            if (string.IsNullOrWhiteSpace(text))
            {
                TempData["CommentError"] = "Yorum boş olamaz.";
                return RedirectToAction("ProductDetails", new { id = productId });
            }

            var comment = new Comment
            {
                ProductId = productId,
                Text = text,
                Rating = rating,
                UserName = username,
                DatePosted = DateTime.Now
            };

            _context.Comments.Add(comment);
            _context.SaveChanges();

            return RedirectToAction("ProductDetails", new { id = productId });
        }

        [HttpGet]
        public IActionResult AddProduct()
        {
            var username = HttpContext.Session.GetString("Username");
            var userIdStr = HttpContext.Session.GetString("UserId");

            if (username == null || string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToAction("Index");
            }

            ViewData["Username"] = username;
            ViewBag.AllProducts = _context.Products.ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product,
            IFormFile MainImage, IFormFile Image1, IFormFile Image2, IFormFile Image3)
        {
            product.Username = User.Identity.Name;
            var username = HttpContext.Session.GetString("Username");
            var userIdStr = HttpContext.Session.GetString("UserId");

            if (username == null || string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToAction("Index");
            }

            int userId = int.Parse(userIdStr);

            product.SellerId = userId;

            async Task<string?> SaveFileAsync(IFormFile? file)
            {
                if (file != null && file.Length > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    return "/images/" + fileName;
                }
                return null;
            }

            var mainImagePath = await SaveFileAsync(MainImage);
            if (mainImagePath != null)
                product.ImagePath = mainImagePath;

            var imagePath1 = await SaveFileAsync(Image1);
            if (imagePath1 != null)
                product.ImagePath2 = imagePath1;

            var imagePath2 = await SaveFileAsync(Image2);
            if (imagePath2 != null)
                product.ImagePath3 = imagePath2;

            var imagePath3 = await SaveFileAsync(Image3);
            if (imagePath3 != null)
                product.ImagePath4 = imagePath3;

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("Products");
        }

        [HttpPost]
        public IActionResult DeleteProduct(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                TempData["Message"] = "Ürün bulunamadı.";
                return RedirectToAction("AddProduct");
            }

            _context.Products.Remove(product);
            _context.SaveChanges();

            TempData["Message"] = "Ürün başarıyla silindi.";
            return RedirectToAction("AddProduct");
        }
        [HttpGet]
        public IActionResult EditProduct(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        public IActionResult EditProduct(Product updatedProduct, IFormFile MainImage)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == updatedProduct.Id);
            if (product == null)
            {
                return NotFound();
            }

            product.Name = updatedProduct.Name;
            product.Price = updatedProduct.Price;
            product.Category = updatedProduct.Category;
            product.Description = updatedProduct.Description;

            if (MainImage != null && MainImage.Length > 0)
            {
                var imagePath = Path.Combine("wwwroot/images", MainImage.FileName);
                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    MainImage.CopyTo(stream);
                }

                product.ImagePath = "/images/" + MainImage.FileName;
            }

            _context.Products.Update(product);
            _context.SaveChanges();

            return RedirectToAction("AddProduct");
        }
        [HttpGet]
        public IActionResult SellerAddProduct()
        {
            var username = HttpContext.Session.GetString("Username");
            var userIdStr = HttpContext.Session.GetString("UserId");
            var isSellerStr = HttpContext.Session.GetString("IsSeller");

            if (username == null || string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToAction("Index", "Home");
            }

            bool isSeller = !string.IsNullOrEmpty(isSellerStr) && bool.Parse(isSellerStr);
            if (!isSeller)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            ViewData["Username"] = username;

            int userId = int.Parse(userIdStr);
            ViewBag.AllProducts = _context.Products.Where(p => p.SellerId == userId).ToList();

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> SellerAddProduct(Product product,
            IFormFile MainImage, IFormFile Image1, IFormFile Image2, IFormFile Image3)
        {
            var username = HttpContext.Session.GetString("Username");
            var userIdStr = HttpContext.Session.GetString("UserId");
            var isSellerStr = HttpContext.Session.GetString("IsSeller");

            if (username == null || string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToAction("Index", "Home");
            }

            bool isSeller = !string.IsNullOrEmpty(isSellerStr) && bool.Parse(isSellerStr);
            if (!isSeller)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            int userId = int.Parse(userIdStr);
            product.SellerId = userId;

            async Task<string?> SaveFileAsync(IFormFile? file)
            {
                if (file != null && file.Length > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    return "/images/" + fileName;
                }
                return null;
            }

            var mainImagePath = await SaveFileAsync(MainImage);
            if (mainImagePath != null)
                product.ImagePath = mainImagePath;

            var imagePath1 = await SaveFileAsync(Image1);
            if (imagePath1 != null)
                product.ImagePath2 = imagePath1;

            var imagePath2 = await SaveFileAsync(Image2);
            if (imagePath2 != null)
                product.ImagePath3 = imagePath2;

            var imagePath3 = await SaveFileAsync(Image3);
            if (imagePath3 != null)
                product.ImagePath4 = imagePath3;

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("Products");
        }
        [HttpPost]
        public IActionResult SellerDeleteProduct(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                TempData["Message"] = "Ürün bulunamadı.";
                return RedirectToAction("SellerAddProduct");
            }

            _context.Products.Remove(product);
            _context.SaveChanges();

            TempData["Message"] = "Ürün başarıyla silindi.";
            return RedirectToAction("SellerAddProduct");
        }

        [HttpPost]
        public IActionResult SellerEditProduct(Product updatedProduct, IFormFile MainImage)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == updatedProduct.Id);
            if (product == null)
            {
                return NotFound();
            }

            product.Name = updatedProduct.Name;
            product.Price = updatedProduct.Price;
            product.Category = updatedProduct.Category;
            product.Description = updatedProduct.Description;

            if (MainImage != null && MainImage.Length > 0)
            {
                var imagePath = Path.Combine("wwwroot/images", MainImage.FileName);
                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    MainImage.CopyTo(stream);
                }
                product.ImagePath = "/images/" + MainImage.FileName;
            }

            _context.Products.Update(product);
            _context.SaveChanges();

            return RedirectToAction("SellerAddProduct");
        }
        [HttpGet]
        public IActionResult SellerEditProduct(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            var username = HttpContext.Session.GetString("Username");
            ViewData["Username"] = username;

            return View(product); 
        }


    }
}
