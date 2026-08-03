using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS.DatabaseContext;
using SIMS.DatabaseContext.Entities;

namespace SIMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly SimsDbContext _context;

        public UserController(SimsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string keyword = search.Trim();

                query = query.Where(u =>
                    u.Username.Contains(keyword) ||
                    u.Email.Contains(keyword) ||
                    u.Role.Contains(keyword));
            }

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            ViewBag.Search = search;

            return View(users);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new User
            {
                Role = "Student",
                Status = 1,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            user.Username = user.Username?.Trim() ?? string.Empty;
            user.Email = user.Email?.Trim() ?? string.Empty;
            user.Phone = user.Phone?.Trim();
            user.Address = user.Address?.Trim();

            if (string.IsNullOrWhiteSpace(user.Username))
            {
                ModelState.AddModelError(
                    nameof(user.Username),
                    "Username is required.");
            }

            if (string.IsNullOrWhiteSpace(user.Password))
            {
                ModelState.AddModelError(
                    nameof(user.Password),
                    "Password is required.");
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                ModelState.AddModelError(
                    nameof(user.Email),
                    "Email is required.");
            }

            bool usernameExists = await _context.Users
                .AnyAsync(u => u.Username == user.Username);

            if (usernameExists)
            {
                ModelState.AddModelError(
                    nameof(user.Username),
                    "Username already exists.");
            }

            bool emailExists = await _context.Users
                .AnyAsync(u => u.Email == user.Email);

            if (emailExists)
            {
                ModelState.AddModelError(
                    nameof(user.Email),
                    "Email already exists.");
            }

            string[] validRoles =
            {
        "Admin",
        "Teacher",
        "Student"
    };

            if (!validRoles.Contains(user.Role))
            {
                ModelState.AddModelError(
                    nameof(user.Role),
                    "Invalid role.");
            }

            if (!ModelState.IsValid)
            {
                return View(user);
            }

            user.Status = 1;
            user.CreatedAt = DateTime.Now;
            user.UpdatedAt = DateTime.Now;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "User created successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
public async Task<IActionResult> Edit(int? id)
{
    if (id == null)
    {
        return NotFound();
    }

    var user = await _context.Users.FindAsync(id);

    if (user == null)
    {
        return NotFound();
    }

    return View(user);
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, User user)
{
    if (id != user.Id)
    {
        return NotFound();
    }

    var existingUser = await _context.Users.FindAsync(id);

    if (existingUser == null)
    {
        return NotFound();
    }

    user.Username = user.Username?.Trim() ?? string.Empty;
    user.Email = user.Email?.Trim() ?? string.Empty;
    user.Phone = user.Phone?.Trim();
    user.Address = user.Address?.Trim();

    bool usernameExists = await _context.Users.AnyAsync(u =>
        u.Username == user.Username &&
        u.Id != user.Id);

    if (usernameExists)
    {
        ModelState.AddModelError(
            nameof(user.Username),
            "Username already exists.");
    }

    bool emailExists = await _context.Users.AnyAsync(u =>
        u.Email == user.Email &&
        u.Id != user.Id);

    if (emailExists)
    {
        ModelState.AddModelError(
            nameof(user.Email),
            "Email already exists.");
    }

    string[] validRoles =
    {
        "Admin",
        "Teacher",
        "Student"
    };

    if (!validRoles.Contains(user.Role))
    {
        ModelState.AddModelError(
            nameof(user.Role),
            "Invalid role.");
    }

    // Không bắt nhập lại Password khi Edit
    ModelState.Remove(nameof(user.Password));

    if (!ModelState.IsValid)
    {
        return View(user);
    }

    existingUser.Username = user.Username;
    existingUser.Email = user.Email;
    existingUser.Phone = user.Phone;
    existingUser.Address = user.Address;
    existingUser.Role = user.Role;
    existingUser.UpdatedAt = DateTime.Now;

    await _context.SaveChangesAsync();

    TempData["Success"] = "User updated successfully.";

    return RedirectToAction(nameof(Index));
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ToggleStatus(int id)
{
    var user = await _context.Users.FindAsync(id);

    if (user == null)
    {
        return NotFound();
    }

    string? currentUsername = User.Identity?.Name;

    if (user.Username == currentUsername)
    {
        TempData["Error"] =
            "You cannot lock your own account.";

        return RedirectToAction(nameof(Index));
    }

    user.Status = user.Status == 1
        ? (byte)0
        : (byte)1;

    user.UpdatedAt = DateTime.Now;

    await _context.SaveChangesAsync();

    TempData["Success"] = user.Status == 1
        ? "User unlocked successfully."
        : "User locked successfully.";

    return RedirectToAction(nameof(Index));
}

[HttpGet]
public async Task<IActionResult> ResetPassword(int? id)
{
    if (id == null)
    {
        return NotFound();
    }

    var user = await _context.Users.FindAsync(id);

    if (user == null)
    {
        return NotFound();
    }

    return View(user);
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ResetPassword(
    int id,
    string newPassword,
    string confirmPassword)
{
    var user = await _context.Users.FindAsync(id);

    if (user == null)
    {
        return NotFound();
    }

    if (string.IsNullOrWhiteSpace(newPassword))
    {
        ViewBag.Error = "New password is required.";
        return View(user);
    }

    if (newPassword.Length < 6)
    {
        ViewBag.Error =
            "Password must contain at least 6 characters.";

        return View(user);
    }

    if (newPassword != confirmPassword)
    {
        ViewBag.Error =
            "Password confirmation does not match.";

        return View(user);
    }

    user.Password = newPassword;
    user.UpdatedAt = DateTime.Now;

    await _context.SaveChangesAsync();

    TempData["Success"] =
        "Password reset successfully.";

    return RedirectToAction(nameof(Index));
}
    }
}