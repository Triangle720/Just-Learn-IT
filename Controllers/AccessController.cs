using System;
using System.Linq;
using System.Threading.Tasks;
using JustLearnIT.Data;
using JustLearnIT.Models;
using JustLearnIT.Security;
using JustLearnIT.Services;
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
            NotVerified,
            ShortPass
        }

        public AccessController(DatabaseContext context)
        {
            _context = context;
        }

        [Route("Access/{message?}")]
        [AllowAnonymous]
        public IActionResult Index(IndexMessage message = IndexMessage.None)
        {
            /* It needs to be changed (looks a bit ugly and work not good at all) */
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
                case IndexMessage.ShortPass:
                    ViewBag.RegisterMessage2 = "Password too short. 6+ chars";
                    break;
                default:
                    ViewBag.LoginErr = ViewBag.RegisterMessage = ViewBag.RegisterMessage2 = ViewBag.EmailErr = string.Empty;
                    break;
            }

            return View();
        }

        #region Register & email verification

        [AllowAnonymous]
        public async Task<IActionResult> Register([Bind("Login, Email")] UserModel user, string passwordString)
        {
            if (_context.Users.Where(u => u.Login == user.Login).Any())
                return RedirectToAction("Index", new { message = IndexMessage.LoginTaken });

            else if (_context.Users.Where(u => u.Email == InputManager.ParseEmail(user.Email)).Any())
                return RedirectToAction("Index", new { message = IndexMessage.EmailTaken });
            
            else if (string.IsNullOrEmpty(passwordString) || passwordString.Length < 6) 
                return RedirectToAction("Index", new { message = IndexMessage.ShortPass });

            var temp = user;
            temp.Id = Guid.NewGuid().ToString();
            temp.Password = await InputManager.EncryptPassword(passwordString, temp.Id, _context);
            temp.Email = InputManager.ParseEmail(temp.Email);
            temp.AccountCreationTime = DateTime.Now;
            temp.Role = Role.USER;
            temp.VerificationCode = new VerificationCodeModel
            {
                UserModelId = temp.Id,
                RadnomUriCode = await EmailService.SendEmail(temp.Email, temp.Login, EmailType.Email_Verification)
            };

            await _context.AddAsync(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { message = IndexMessage.AccountCreated });
        }

        [Route("Access/Verify/{key?}")]
        public async Task<IActionResult> Verify(string key = "")
        {
            var user = await _context.Users.Where(u => u.VerificationCode.RadnomUriCode == key).FirstOrDefaultAsync();

            if (user != null)
            {
                user.IsVerified = true;
                _context.Entry(user).State = EntityState.Modified;
                _context.Remove(user.VerificationCode);
                await _context.SaveChangesAsync();
                HttpContext.Session.SetString("Verification", "Success");
            }

            return View();
        }

        #endregion

        #region Login & two factor auth (if device is not trusted yet)
        [AllowAnonymous]
        public async Task<IActionResult> Login(UserModel user, string passwordString)
        {
            var temp = await _context.Users.Where(u => u.Login == user.Login).FirstOrDefaultAsync();

            if (temp != null && !string.IsNullOrEmpty(passwordString))
            {
                if (await InputManager.CheckPassword(passwordString, temp.Id, temp.Password, _context))
                {
                    if (!temp.IsVerified) return RedirectToAction("Index", new { message = IndexMessage.NotVerified });

                    HttpContext.Session.SetString("LoggingIn", temp.Id); // Set statement string <- Save 'temp' to cache memory instead or smth like that
                    return RedirectToAction("CheckDevice", "Access"); // check device
                }
            }

            return RedirectToAction("Index", new { message = IndexMessage.IncorrectLogin });
        }

        // JS checks if user has 'special' string inside localStorage, if not : send email -> go to OTPassword, else : AcceptLogin
        public IActionResult CheckDevice()
        {
            var userId = HttpContext.Session.GetString("LoggingIn");

            if (userId != null)
            {
                var user = _context.Users.Where(u => u.Id == userId).First();
                if (_context.Users.Contains(user)) return View("CheckDevice");
            }

            return RedirectToAction("Index", "Home");
        }

        // So far the method sends the password only once.The password has no expiration time for now.
        public async Task<IActionResult> SendLoginPassword()
        {
            var userId = HttpContext.Session.GetString("LoggingIn");
            if (userId != null)
            {
                var user = await _context.Users.Where(u => u.Id == userId).FirstAsync();

                if (user.OneTimePass == null)
                {
                    var pass = new OneTimePassword
                    {
                        UserModelId = user.Id,
                        Value = await EmailService.SendEmail(user.Email, user.Login, EmailType.Login_Verification)
                    };
                    user.OneTimePass = pass;

                    _context.Entry(user).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction("OTPassword", "Access");
            }
            return RedirectToAction("Index", "Home");
        }

        // Get generated password from user
        public IActionResult OTPassword()
        {
            var userId = HttpContext.Session.GetString("LoggingIn");

            // only when user is during login
            if (userId != null && _context.OneTimePasswords.Where(p => p.UserModelId == userId).Any())
            {
                return View("OTPassword");
            }

            return RedirectToAction("Index", "Home");
        }

        // check if generated password match with input
        public async Task<IActionResult> CheckTrustedDevicePassword(string code)
        {
            var userId = HttpContext.Session.GetString("LoggingIn");

            if (userId != null)
            {
                var user = await _context.Users.Where(u => u.Id == userId).FirstAsync();

                if (code == user.OneTimePass.Value)
                {
                    _context.Remove(user.OneTimePass);
                    await _context.SaveChangesAsync();
                    return await AcceptLogin();
                }

                ViewBag.Error = "Wrong code!";
                return View("OTPassword");
            }
            else return RedirectToAction("Index", "Home");
        }

        // FINALLY: User is logged in
        public async Task<IActionResult> AcceptLogin()
        {
            var userId = HttpContext.Session.GetString("LoggingIn");

            if(userId != null)
            {
                var user = await _context.Users.Where(u => u.Id == userId).FirstAsync();
                await AuthService.SetJWT(user, HttpContext);
                HttpContext.Session.Remove("LoggingIn");
            }

            return RedirectToAction("Index", "Home");
        }
        #endregion

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
