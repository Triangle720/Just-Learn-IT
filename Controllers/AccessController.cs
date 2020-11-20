using System;
using System.Collections.Generic;
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
                default:
                    ViewBag.LoginErr = ViewBag.RegisterMessage = string.Empty;
                    break;
            }

            return View();
        }

        #region Register & email verification

        [AllowAnonymous]
        public async Task<IActionResult> Register([Bind("Login, Email")] UserModel user, string passwordString)
        {
            if (await _context.Users.Where(u => u.Login == user.Login).AnyAsync())
                return RedirectToAction("Index", new { message = IndexMessage.LoginTaken });

            else if (await _context.Users.Where(u => u.Email == InputManager.ParseEmail(user.Email)).AnyAsync())
                return RedirectToAction("Index", new { message = IndexMessage.EmailTaken });

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

        #endregion

        #region Login & two factor auth (if device is not trusted yet)
        [AllowAnonymous]
        public async Task<IActionResult> Login(UserModel user, string passwordString)
        {
            var temp = await _context.Users.Where(u => u.Login == user.Login).FirstOrDefaultAsync();

            if (temp != null)
            {
                if (await InputManager.CheckPassword(passwordString, temp.Id, temp.Password, _context))
                {
                    if (!temp.IsVerified) return RedirectToAction("Index", new { message = IndexMessage.NotVerified });

                    HttpContext.Session.SetString("Statement", "Logging In"); // Set statement string
                    return RedirectToAction("CheckDevice", "Access", new { userId = temp.Id }); // check device
                }
            }

            return RedirectToAction("Index", new { message = IndexMessage.IncorrectLogin });
        }

        // JS checks if user has 'special' string inside localStorage, if not : send email -> go to OTPassword, else : AcceptLogin
        public IActionResult CheckDevice(string userId)
        {
            if (HttpContext.Session.GetString("Statement") == "Logging In")
            {
                var user = _context.Users.Where(u => u.Id == userId).First();
                if (_context.Users.Contains(user)) return View("CheckDevice", userId);
            }

            return RedirectToAction("Index", "Home");
        }

        // So far the method sends the password only once.The password has no expiration time for now.
        public async Task<IActionResult> SendLoginPassword(string userId)
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

            return RedirectToAction("OTPassword", "Access", new { userId = user.Id });
        }

        // Get generated password from user
        public IActionResult OTPassword(string userId)
        {
            // only when user is during login
            if (HttpContext.Session.GetString("Statement") == "Logging In" && _context.OneTimePasswords.Where(p => p.UserModelId == userId).Any())
            {
                return View("OTPassword", userId);
            }

            return RedirectToAction("Index", "Home");
        }

        // check if generated password match with input
        public async Task<IActionResult> CheckTrustedDevicePassword(Dictionary<string, string> parms)
        {
            if (HttpContext.Session.GetString("Statement") == "Logging In")
            {
                var user = await _context.Users.Where(u => u.Id == parms["userId"]).FirstAsync();

                if (parms["password"] == user.OneTimePass.Value)
                {
                    _context.Remove(user.OneTimePass);
                    await _context.SaveChangesAsync();
                    return await AcceptLogin(user.Id);
                }

                ViewBag.Error = "Wrong code!";
                return View("OTPassword", user.Id);
            }
            else return RedirectToAction("Index", "Home");
        }

        // FINALLY: User is logged in
        public async Task<IActionResult> AcceptLogin(string userId)
        {
            var user = await _context.Users.Where(u => u.Id == userId).FirstAsync();
            await AuthService.SetJWT(user, HttpContext);
            HttpContext.Session.Remove("Statement");
            var x = HttpContext.Session;
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
