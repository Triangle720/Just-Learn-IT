using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using JustLearnIT.Models;
using JustLearnIT.Data;
using Microsoft.AspNetCore.Authorization;

namespace JustLearnIT.Controllers
{
    public class HomeController : Controller
    {
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
