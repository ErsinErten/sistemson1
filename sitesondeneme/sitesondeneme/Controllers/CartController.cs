using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using sitesondeneme.Models;
using System.Linq;

namespace sitesondeneme.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            string username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Account", "Account");
            }

            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
            {
                return NotFound();
            }

            var cartItem = _context.CartItems
                            .FirstOrDefault(c => c.ProductId == productId && c.Username == username);

            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    ProductId = productId,
                    Username = username,
                    Quantity = quantity
                };
                _context.CartItems.Add(cartItem);
            }
            else
            {
                cartItem.Quantity += quantity;
                _context.CartItems.Update(cartItem);
            }

            _context.SaveChanges();

            return RedirectToAction("Cart", "Cart");
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int cartItemId)
        {
            string username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Account", "Account");
            }

            var cartItem = _context.CartItems
                            .FirstOrDefault(c => c.Id == cartItemId && c.Username == username);

            if (cartItem == null)
            {
                TempData["Message"] = "Silinecek ürün bulunamadı.";
                return RedirectToAction("Cart");
            }

            _context.CartItems.Remove(cartItem);
            _context.SaveChanges();

            TempData["Message"] = "Ürün sepetten kaldırıldı.";
            return RedirectToAction("Cart");
        }
        public IActionResult Cart()
        {
            var username = HttpContext.Session.GetString("Username");
            ViewData["Username"] = username;

            var isAdminStr = HttpContext.Session.GetString("IsAdmin");
            bool isAdmin = !string.IsNullOrEmpty(isAdminStr) && bool.Parse(isAdminStr);
            ViewData["IsAdmin"] = isAdmin;

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Account", "Account");
            }


            var cartItems = _context.CartItems
                .Where(c => c.Username == username)
                .Include(c => c.Product)
                .ToList();

            return View(cartItems);
        }


    }
}