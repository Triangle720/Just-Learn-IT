using Microsoft.AspNetCore.Mvc;
using JustLearnIT.Data;
using Microsoft.AspNetCore.Authorization;
using JustLearnIT.Security;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace JustLearnIT.Controllers
{
    public class HomeController : Controller
    {
        private readonly DatabaseContext _context;

        public HomeController(DatabaseContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [RoleAuthFilter("ADMIN,USER")]
        public IActionResult Profile()
        {
            var user = _context.Users.Where(u => u.Login == AuthService.GetJWTAudience(HttpContext.Session.GetString("TOKEN")))
                                     .FirstOrDefault();
            return View(user);
        }

        [RoleAuthFilter("ADMIN,USER")]
        public IActionResult Courses()
        {
            var subscription = _context.Users.Where(u => u.Login == AuthService.GetJWTAudience(HttpContext.Session.GetString("TOKEN")))
                                             .FirstOrDefault()
                                             .Subscription;
            return View(subscription);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
