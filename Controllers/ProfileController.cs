using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS.DatabaseContext;
using SIMS.DatabaseContext.Entities;
using SIMS.Models;

namespace SIMS.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly SimsDbContext _context;

        public ProfileController(SimsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var model = new ProfileViewModel
            {
                UserId = user.Id,
                Username = user.Username,
                Role = user.Role,
                FullName = user.Username,

                // Chỉ giữ các dòng này nếu User.cs có các thuộc tính tương ứng
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address
            };

            if (user.Role.Equals(
                "Student",
                StringComparison.OrdinalIgnoreCase))
            {
                var student = await _context.Students
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.UserId == user.Id);

                if (student == null)
                {
                    ViewBag.Error =
                        "Your account has not been linked to a student profile.";
                }
                else
                {
                    model.FullName = student.FullName;
                    model.Email = student.Email;
                    model.Phone = student.Phone;
                    model.Address = student.Address;
                    model.DateOfBirth = student.DateOfBirth;
                    model.Gender = student.Gender;
                    model.StudentCode = student.StudentCode;
                }
            }
            else if (user.Role.Equals(
                "Teacher",
                StringComparison.OrdinalIgnoreCase))
            {
                var teacher = await _context.Teachers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.UserId == user.Id);

                if (teacher == null)
                {
                    ViewBag.Error =
                        "Your account has not been linked to a teacher profile.";
                }
                else
                {
                    model.FullName = teacher.FullName;
                    model.Email = teacher.Email;
                    model.Phone = teacher.Phone;
                    model.TeacherCode = teacher.TeacherCode;
                    model.Department = teacher.Department;
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProfileViewModel model)
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // Không cho người dùng tự sửa Username, Role, UserId
            model.UserId = user.Id;
            model.Username = user.Username;
            model.Role = user.Role;

            ModelState.Remove(nameof(ProfileViewModel.UserId));
            ModelState.Remove(nameof(ProfileViewModel.Username));
            ModelState.Remove(nameof(ProfileViewModel.Role));

            if (!ModelState.IsValid)
            {
                await RestoreReadOnlyFieldsAsync(model, user.Id);
                return View(model);
            }

            if (user.Role.Equals(
                "Admin",
                StringComparison.OrdinalIgnoreCase))
            {
                user.Email = model.Email?.Trim();
                user.Phone = model.Phone?.Trim();
                user.Address = model.Address?.Trim();
                user.UpdatedAt = DateTime.Now;
            }
            else if (user.Role.Equals(
                "Student",
                StringComparison.OrdinalIgnoreCase))
            {
                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.UserId == user.Id);

                if (student == null)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Your account has not been linked to a student profile.");

                    return View(model);
                }

                student.FullName = model.FullName.Trim();
                student.Email = model.Email?.Trim();
                student.Phone = model.Phone?.Trim();
                student.Address = model.Address?.Trim();
                student.DateOfBirth = model.DateOfBirth;
                student.Gender = model.Gender?.Trim();
                student.UpdatedAt = DateTime.Now;

                user.Email = model.Email?.Trim();
                user.Phone = model.Phone?.Trim();
                user.Address = model.Address?.Trim();
                user.UpdatedAt = DateTime.Now;
            }
            else if (user.Role.Equals(
                "Teacher",
                StringComparison.OrdinalIgnoreCase))
            {
                var teacher = await _context.Teachers
                    .FirstOrDefaultAsync(t => t.UserId == user.Id);

                if (teacher == null)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Your account has not been linked to a teacher profile.");

                    return View(model);
                }

                teacher.FullName = model.FullName.Trim();
                teacher.Email = model.Email?.Trim() ?? string.Empty;
                teacher.Phone = model.Phone?.Trim();
                teacher.Department = model.Department?.Trim();

                user.Email = model.Email?.Trim();
                user.Phone = model.Phone?.Trim();
                user.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Profile updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        private async Task<User?> GetCurrentUserAsync()
        {
            string? username = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            return await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Username == username);
        }

        private async Task RestoreReadOnlyFieldsAsync(
            ProfileViewModel model,
            int userId)
        {
            if (model.Role.Equals(
                "Student",
                StringComparison.OrdinalIgnoreCase))
            {
                var student = await _context.Students
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.UserId == userId);

                model.StudentCode = student?.StudentCode;
            }
            else if (model.Role.Equals(
                "Teacher",
                StringComparison.OrdinalIgnoreCase))
            {
                var teacher = await _context.Teachers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.UserId == userId);

                model.TeacherCode = teacher?.TeacherCode;
            }
        }
    }
}