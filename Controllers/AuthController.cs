using Microsoft.AspNetCore.Mvc;

namespace SIMS.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
