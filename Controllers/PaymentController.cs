using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JustLearnIT.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JustLearnIT.Controllers
{
    public class PaymentController : Controller
    {
        [RoleAuthFilter("ADMIN,USER")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
