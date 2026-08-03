using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMS.Interfaces;
using SIMS.Models;
using System.Security.Claims;

namespace SIMS.Controllers
{
    public class LoginController : Controller
    {
        private readonly IUserService userService;
        public LoginController(IUserService service)
        {
            userService = service;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(); // tra ve 1 giao dien
        }
        [HttpPost]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // khong co loi gi
                string username = model.Username.ToString().Trim();
                string password = model.Password.ToString().Trim();
                var user = await userService.LoginUserAsync(username, password);
                if (user != null)
                {
                    // ma hoa thong tin nguoi dung va luu vao cookie
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Role, user.Role)
                    };
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(identity)
                    );

                    // chuyen vao DashboardController
                    return RedirectToAction("Index", "Dashboard");
                }
                ViewData["InvalidAccount"] = "Username or Password invalid";
            }

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }
            return RedirectToAction("Index", "Login");
        }
    }
}
