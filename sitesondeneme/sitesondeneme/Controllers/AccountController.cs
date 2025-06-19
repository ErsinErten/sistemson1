using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using sitesondeneme.Models;
using System.Linq;

namespace sitesondeneme.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Account()
        {
            var username = HttpContext.Session.GetString("Username");
            ViewData["Username"] = username;

            var isAdminStr = HttpContext.Session.GetString("IsAdmin");
            bool isAdmin = !string.IsNullOrEmpty(isAdminStr) && bool.Parse(isAdminStr);
            ViewData["IsAdmin"] = isAdmin;

            return View();
        }


        [HttpPost]
        public IActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                _context.Users.Add(user);
                _context.SaveChanges();
                return RedirectToAction("Index", "Home");

            }

            return View("account");
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Kullanıcı adı veya şifre yanlış.");
                ViewData["ShowLogin"] = true;
                return View("account");
            }

            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("IsAdmin", user.IsAdmin.ToString());
            HttpContext.Session.SetString("IsSeller", user.IsSeller.ToString());
            HttpContext.Session.SetString("UserId", user.Id.ToString());

            TempData["LoginMessage"] = "Giriş başarılı!";
            return RedirectToAction("Index", "Home");
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Remove("Username");
            HttpContext.Session.Remove("IsAdmin");
            HttpContext.Session.Remove("IsSeller"); 
            HttpContext.Session.Remove("UserId");

            TempData["LogoutMessage"] = "Başarıyla çıkış yaptınız.";
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfileImage(int id, IFormFile profileImage)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (profileImage != null && profileImage.Length > 0)
            {
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsDir))
                    Directory.CreateDirectory(uploadsDir);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(profileImage.FileName);
                var filePath = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(stream);
                }

                user.ProfileImagePath = "/uploads/" + fileName;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("EditProfile");
        }

    }
}