using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SIMS.Controllers
{
    [Authorize]
    public class AccountMenuController : Controller
    {
        [HttpGet]
        public IActionResult Analytics()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Settings()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Help()
        {
            return View();
        }
    }
}