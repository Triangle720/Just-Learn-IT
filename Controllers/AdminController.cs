using System.Linq;
using System.Threading.Tasks;
using JustLearnIT.Data;
using JustLearnIT.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JustLearnIT.Controllers
{
    [RoleAuthFilter("ADMIN")]
    public class AdminController : Controller
    {
        private readonly DatabaseContext _context;
        public AdminController(DatabaseContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var users = _context.Users.ToList();
            return View(users);
        }

        public IActionResult UserDelete(string id)
        {
            var user = _context.Users.Where(u => u.Id == id).FirstOrDefault();
            return View(user);
        }

        public async Task<IActionResult> Delete(string id)
        {
            var user = await _context.Users.Where(u => u.Id == id).FirstOrDefaultAsync();

            if(user != null)
            {
                if (user.VerificationCode != null) _context.Remove(user.VerificationCode);
                if (user.OneTimePass != null) _context.Remove(user.OneTimePass);
                if (user.Orders.Count() > 0) _context.RemoveRange(user.OneTimePass);
                _context.Remove(user.Salt);
                _context.Remove(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}
