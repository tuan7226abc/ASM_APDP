using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SIMS.Interfaces;
using SIMS.Models;

namespace SIMS.Controllers
{
    public class LoginController : Controller
    {
        private readonly IUserService _userService;

        public LoginController(IUserService userService)
        {
            _userService = userService;
        }

        // =========================
        // GET: /Login
        // =========================
        [HttpGet]
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectByRole();
            }

            return View(new LoginViewModel());
        }

        // =========================
        // POST: /Login
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Đổi AuthenticateAsync thành đúng tên hàm
            // đang có trong IUserService của bạn.
            var user = await _userService.LoginUserAsync(
    model.Username,
    model.Password);

            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid username or password.");

                return View(model);
            }

            if (user.Status != 1)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "This account has been locked.");

                return View(model);
            }

            string username = user.Username?.Trim() ?? string.Empty;
            string role = user.Role?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(role))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The account information is invalid.");

                return View(model);
            }

            // Mỗi Claim phải có đủ Type và Value.
            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    username),

                new Claim(
                    ClaimTypes.Role,
                    role)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            var properties = new AuthenticationProperties
            {
                IsPersistent = false,
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                properties);

            if (string.Equals(
                role,
                "Admin",
                StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            if (string.Equals(
                role,
                "Teacher",
                StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(
                    "Index",
                    "TeacherDashboard");
            }

            if (string.Equals(
                role,
                "Student",
                StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(
                    "Index",
                    "StudentDashboard");
            }

            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            ModelState.AddModelError(
                string.Empty,
                "This account role is not supported.");

            return View(model);
        }

        // =========================
        // Logout POST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(
                nameof(Index),
                "Login");
        }

        // Dùng nếu nút logout hiện tại đang là thẻ <a>
        [HttpGet]
        public async Task<IActionResult> SignOutAccount()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(
                nameof(Index),
                "Login");
        }

        private IActionResult RedirectByRole()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            if (User.IsInRole("Teacher"))
            {
                return RedirectToAction(
                    "Index",
                    "TeacherDashboard");
            }

            if (User.IsInRole("Student"))
            {
                return RedirectToAction(
                    "Index",
                    "StudentDashboard");
            }

            return RedirectToAction(
                "AccessDenied",
                "Auth");
        }
    }
}