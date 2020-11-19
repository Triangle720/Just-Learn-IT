using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using JustLearnIT.Data;
using JustLearnIT.Models;
using JustLearnIT.Security;
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
            LoginTaken,
            EmailTaken,
            NotVerified
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
                case IndexMessage.NotVerified:
                    ViewBag.LoginErr = "Check your email & verify account";
                    break;
                case IndexMessage.AccountCreated:
                    ViewBag.RegisterMessage = "Accout successfully created!";
                    break;
                case IndexMessage.LoginTaken:
                    ViewBag.RegisterMessage = "Login already taken";
                    break;
                case IndexMessage.EmailTaken:
                    ViewBag.EmailErr = "Email assigned to another account";
                    break;
                default:
                    ViewBag.LoginErr = ViewBag.RegisterMessage = string.Empty;
                    break;
            }

            return View();
        }

        [Route("Access/Verify/{key?}")]
        public async Task<IActionResult> Verify(string key = "")
        {
            if (key != string.Empty)
            {
                var user = await _context.Users.Where(u => u.VerificationCode.RadnomUriCode == key).FirstOrDefaultAsync();

                if (user != null)
                {
                    user.IsVerified = true;
                    _context.Entry(user).State = EntityState.Modified;
                    _context.Remove(user.VerificationCode);
                    await _context.SaveChangesAsync();

                    ViewBag.Message = "Account verified successfully!";
                    return View(true);
                }
            }

            ViewBag.Message = "Are you lost? :)";
            return View(false);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Login([Bind("Login")] UserModel user, string passwordString)
        {
            var temp = await _context.Users.Where(u => u.Login == user.Login.ToLower()).FirstOrDefaultAsync();

            if (temp != null)
            {
                if (await InputManager.CheckPassword(passwordString, temp.Id, temp.Password, _context))
                {
                    if (!temp.IsVerified) return RedirectToAction("Index", new { message = IndexMessage.NotVerified }); ;

                    var token = await AuthService.AssignToken(temp);

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
        public async Task<IActionResult> Register([Bind("Login, Email")] UserModel user, string passwordString)
        {
            if (await _context.Users.Where(u => u.Login == user.Login).AnyAsync())
                return RedirectToAction("Index", new { message = IndexMessage.LoginTaken });

            else if (await _context.Users.Where(u => u.Email == user.Email).AnyAsync())
                return RedirectToAction("Index", new { message = IndexMessage.EmailTaken });

            var temp = user;
            temp.Id = Guid.NewGuid().ToString();
            temp.Login = temp.Login.ToLower();
            temp.Password = await InputManager.EncryptPassword(passwordString, temp.Id, _context);
            temp.Email = InputManager.ParseEmail(temp.Email);

            temp.VerificationCode = new VerificationCodeModel
            {
                UserModelId = temp.Id,
                RadnomUriCode = await AuthService.SendEmail(temp.Email, temp.Login, EmailType.Email_Verification)
            };

            await _context.AddAsync(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { message = IndexMessage.AccountCreated });
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
