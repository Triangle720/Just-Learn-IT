using System.Linq;
using System.Threading.Tasks;
using JustLearnIT.Data;
using JustLearnIT.Security;
using JustLearnIT.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JustLearnIT.Controllers
{
    [RoleAuthFilter("ADMIN,USER")]
    [SubscriptionFilter]
    public class PaymentController : Controller
    {
        private readonly DatabaseContext _context;
        public PaymentController(DatabaseContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("Payment/PayU_Redirect/{amount}")]
        public async Task<IActionResult> PayU_Redirect(string amount)
        {
            if (!string.IsNullOrEmpty(amount))
            {
                var results = await PaymentService.CreateOrder(amount);

                // retry if no response
                if(string.IsNullOrEmpty(results))
                {
                    await PaymentService.GetAccessToken();
                    results = await PaymentService.CreateOrder(amount);
                }

                // if still no response
                if (string.IsNullOrEmpty(results)) return RedirectToAction("Error", "Payment");

                var separatedResults = results.Split(";");
                await _context.AddAsync(new Models.OrderModel
                {
                    Id = separatedResults[1],
                    UserModelId = AuthService.GetJWTAudience(HttpContext.Session.GetString("TOKEN"))
                });
                await _context.SaveChangesAsync();

                return Redirect(separatedResults[0]);
            }

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Result(int error = 0)
        {
            if(error != 0) return RedirectToAction("Error");
            var user = _context.Users.Where(u => u.Id == AuthService.GetJWTAudience(HttpContext.Session.GetString("TOKEN"))).FirstOrDefault();

            if (user.Orders == null) return RedirectToAction("Error"); // TODO: Show info about no orders


            /*
             * let's assume that we only accept the completed transaction and then delete all of them
             * TODO: Optimization
             */

            foreach (Models.OrderModel order in user.Orders)
            {
                var status = await PaymentService.IsOrderAccepted(order.Id);
                
                if(status)
                {
                    _context.RemoveRange(user.Orders);
                    user.Subscription = true;
                    _context.Entry(user).State = EntityState.Modified;
                    await _context.SaveChangesAsync();

                    HttpContext.Session.SetString("SUB", true.ToString());
                    return View(true);
                }

                _context.Remove(order);
            }

            return View(false);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
