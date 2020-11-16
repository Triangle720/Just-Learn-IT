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

        public enum IndexMessage
        {
            None,
            IncorrectLogin,
            AccountCreated,
            LoginTaken
        }

        public AccessController(DatabaseContext context)
        {
            _context = context;
        }

        [Route("Access/{message?}")]
        [AllowAnonymous]
        public IActionResult Index(IndexMessage message = IndexMessage.None)
        {
            switch (message)
            {
                case IndexMessage.IncorrectLogin:
                    ViewBag.LoginErr = "Wrong username or password";
                    break;
                case IndexMessage.AccountCreated:
                    ViewBag.RegisterMessage = "Accout successfully created!";
                    break;
                case IndexMessage.LoginTaken:
                    ViewBag.RegisterMessage = "Login already taken";
                    break;
                default:
                    ViewBag.LoginErr = ViewBag.RegisterMessage = string.Empty;
                    break;
            }

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
            
            return RedirectToAction("Index", new { message = IndexMessage.IncorrectLogin });
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

                return RedirectToAction("Index", new { message = IndexMessage.AccountCreated });
            }

            return RedirectToAction("Index", new { message = IndexMessage.LoginTaken });
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
