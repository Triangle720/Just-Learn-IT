using Microsoft.AspNetCore.Mvc;
using JustLearnIT.Data;
using Microsoft.AspNetCore.Authorization;
using JustLearnIT.Security;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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

        [Route("Home/Profile/{msg?}")]
        [RoleAuthFilter("ADMIN,USER")]
        public IActionResult Profile(string msg)
        {
            if (!string.IsNullOrEmpty(msg)) ViewBag.Message = msg;

            var user = _context.Users.Where(u => u.Id == AuthService.GetJWTAudience(HttpContext.Session.GetString("TOKEN")))
                                     .FirstOrDefault();
            return View(user);
        }

        [RoleAuthFilter("ADMIN,USER")]
        public IActionResult Courses()
        {
            var subscription = _context.Users.Where(u => u.Id == AuthService.GetJWTAudience(HttpContext.Session.GetString("TOKEN")))
                                             .FirstOrDefault()
                                             .Subscription;
            return View(subscription);
        }


        [RoleAuthFilter("ADMIN,USER")]
        public async Task<IActionResult> ChangePassword(string id, string oldP, string newP)
        {
            string message = string.Empty;

            var user = await _context.Users.Where(u => u.Id == id).FirstOrDefaultAsync();

            // TODO: Make it beautiful later
            if(oldP == null || newP == null || oldP.Length < 6 || newP.Length < 6) message = "Enter 6+ characters";
            else if (!await InputManager.CheckPassword(oldP, user.Id, user.Password, _context)) message = "Wrong password";
            else if (oldP.Equals(newP)) message = "New password must be different";
            else
            {
                var key = await InputManager.EncryptPassword(newP, user.Id, _context);
                user.Password = key;
                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                message = "Password successfully changed";
            }

            return RedirectToAction("Profile", new { msg = message });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
