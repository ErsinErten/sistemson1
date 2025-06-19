using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sitesondeneme.Models;

namespace sitesondeneme.Controllers
{
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;

        public OrderController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Orders()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Account", "Account"); 

            ViewData["Username"] = username;

            var isAdminStr = HttpContext.Session.GetString("IsAdmin");
            bool isAdmin = !string.IsNullOrEmpty(isAdminStr) && bool.Parse(isAdminStr);
            ViewData["IsAdmin"] = isAdmin;

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
                return RedirectToAction("Account", "Account");

            var orders = _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }
        public IActionResult Checkout()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CompletePurchase()
        {
            HttpContext.Session.Remove("Cart");

            return View();
        }
        [HttpPost]
        public IActionResult CompleteOrder()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Account");

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
                return RedirectToAction("Account");

            var cartItems = _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.Username == username)
                .ToList();

            if (!cartItems.Any())
                return RedirectToAction("Cart");

            var totalAmount = cartItems.Sum(i => i.Product.Price * i.Quantity);

            var order = new Order
            {
                UserId = user.Id,
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount,
                Items = cartItems.Select(item => new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                }).ToList()
            };

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cartItems);
            _context.SaveChanges();

            return Json(new { success = true });
        }

    }
}
