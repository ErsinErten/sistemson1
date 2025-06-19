using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using sitesondeneme.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using System.Threading.Tasks;


namespace sitesondeneme.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var username = HttpContext.Session.GetString("Username");
            ViewData["Username"] = username;

            var isAdminStr = HttpContext.Session.GetString("IsAdmin");
            bool isAdmin = !string.IsNullOrEmpty(isAdminStr) && bool.Parse(isAdminStr);
            ViewData["IsAdmin"] = isAdmin;

            return View();
        }

        public IActionResult iletisim()
        {
            var username = HttpContext.Session.GetString("Username");
            ViewData["Username"] = username;

            var isAdminStr = HttpContext.Session.GetString("IsAdmin");
            bool isAdmin = !string.IsNullOrEmpty(isAdminStr) && bool.Parse(isAdminStr);
            ViewData["IsAdmin"] = isAdmin;

            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
    
    }
}
