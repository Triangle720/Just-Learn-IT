using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using JustLearnIT.Models;
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
            //var user = _context.Users.Where(u => u.Login == "test").FirstOrDefault();
            var user = _context.Users.Where(u => u.Login == HttpContext.Session.GetString("LOGIN")).FirstOrDefault();
            return View(user);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
