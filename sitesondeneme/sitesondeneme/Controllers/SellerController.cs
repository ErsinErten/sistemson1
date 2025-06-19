using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sitesondeneme.Models;

public class SellerController : Controller
{
    private readonly AppDbContext _context;

    public SellerController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Profile(int id)
    {
        var username = HttpContext.Session.GetString("Username");
        ViewData["Username"] = username;

        var seller = _context.Users
            .Include(u => u.Products)
            .FirstOrDefault(u => u.Id == id);

        if (seller == null)
        {
            return NotFound();
        }

        return View(seller);
    }
}

