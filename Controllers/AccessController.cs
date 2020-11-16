using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using JustLearnIT.Data;
using JustLearnIT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JustLearnIT.Controllers
{
    public class AccessController : Controller
    {
        private readonly DatabaseContext _context;

        public AccessController(DatabaseContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Login([Bind("Login, Password")] UserModel user)
        {
            var temp = await _context.Users.Where(u => u.Login == user.Login).FirstOrDefaultAsync();

            if (temp != null)
            {
                if (user.Password == temp.Password)
                {
                    var token = await Security.AuthService.AssignToken(temp);

                    JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                    string tokenString = tokenHandler.WriteToken(token);
                    JwtSecurityToken jwtToken = tokenHandler.ReadJwtToken(tokenString);

                    HttpContext.Session.SetString("LOGIN", jwtToken.Audiences.ToArray()[0]);
                    HttpContext.Session.SetString("TOKEN", tokenString);
                    HttpContext.Session.SetString("ROLE", jwtToken.Claims.First(x => x.Type.ToString().Equals(ClaimTypes.Role)).Value);

                    return RedirectToAction("Index", "Home");
                }
            }
            
            // incorrect login or password
            return View("Index");
        }

        [AllowAnonymous]
        public async Task<IActionResult> Register([Bind("Login, Password")] UserModel user)
        {
            if (await _context.Users.Where(u => u.Login == user.Login).FirstOrDefaultAsync() == null)
            {
                var temp = user;
                temp.Id = Guid.NewGuid().ToString();
                await _context.AddAsync(user);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            // login already taken
            return View("Index");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
