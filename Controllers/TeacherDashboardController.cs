using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS.DatabaseContext;
using SIMS.DatabaseContext.Entities;
using SIMS.Models;

namespace SIMS.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherDashboardController : Controller
    {
        private readonly SimsDbContext _context;

        public TeacherDashboardController(
            SimsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int? currentUserId =
                await GetCurrentUserIdAsync();

            if (!currentUserId.HasValue)
            {
                await HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);

                return RedirectToAction(
                    "Index",
                    "Login");
            }

            var teacher = await _context.Teachers
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    t => t.UserId == currentUserId.Value);

            if (teacher == null)
            {
                var emptyModel =
                    new TeacherDashboardViewModel
                    {
                        HasTeacherProfile = false,

                        TeacherName =
                            User.Identity?.Name
                            ?? "Teacher",

                        TeacherCode =
                            string.Empty,

                        TeacherId = 0,

                        CourseCount = 0,

                        StudentCount = 0,

                        TodayClassCount = 0,

                        AssignmentCount = 0,

                        TodaySchedules =
                            new List<Schedule>()
                    };

                return View(emptyModel);
            }

            var teacherSchedules = await _context.Schedules
                .AsNoTracking()
                .Include(s => s.Course)
                .Where(s => s.TeacherId == teacher.Id)
                .ToListAsync();

            var courseIds = teacherSchedules
                .Select(s => s.CourseId)
                .Distinct()
                .ToList();

            int studentCount = 0;

            if (courseIds.Count > 0)
            {
                studentCount =
                    await _context.CourseStudents
                        .AsNoTracking()
                        .Where(cs =>
                            courseIds.Contains(cs.CourseId))
                        .Select(cs => cs.StudentId)
                        .Distinct()
                        .CountAsync();
            }

            DateTime today = DateTime.Today;

            // Nếu database lưu:
            // Monday = 1
            // Tuesday = 2
            // ...
            // Sunday = 7
            int todayNumber =
                today.DayOfWeek == System.DayOfWeek.Sunday
                    ? 7
                    : (int)today.DayOfWeek;

            var todaySchedules = teacherSchedules
                .Where(s =>
                    s.DayOfWeek == todayNumber
                    &&
                    (
                        !s.StartDate.HasValue
                        ||
                        s.StartDate.Value.Date <= today
                    )
                    &&
                    (
                        !s.EndDate.HasValue
                        ||
                        s.EndDate.Value.Date >= today
                    ))
                .OrderBy(s => s.StartTime)
                .ToList();

            int assignmentCount = 0;

            if (courseIds.Count > 0)
            {
                assignmentCount =
                    await _context.Assignments
                        .AsNoTracking()
                        .CountAsync(a =>
                            courseIds.Contains(a.CourseId));
            }

            var model =
                new TeacherDashboardViewModel
                {
                    HasTeacherProfile = true,

                    TeacherId = teacher.Id,

                    TeacherName =
                        teacher.FullName
                        ?? User.Identity?.Name
                        ?? "Teacher",

                    TeacherCode =
                        teacher.TeacherCode
                        ?? string.Empty,

                    CourseCount =
                        courseIds.Count,

                    StudentCount =
                        studentCount,

                    TodayClassCount =
                        todaySchedules.Count,

                    AssignmentCount =
                        assignmentCount,

                    TodaySchedules =
                        todaySchedules
                };

            return View(model);
        }

        // ==========================
        // Lấy User ID hiện tại
        // ==========================
        private async Task<int?> GetCurrentUserIdAsync()
        {
            string? claimUserId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (int.TryParse(
                claimUserId,
                out int parsedUserId))
            {
                return parsedUserId;
            }

            // Dự phòng cho cookie cũ thiếu NameIdentifier
            string? username =
                User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            var currentUser =
                await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        u => u.Username == username);

            return currentUser?.Id;
        }

        // ==========================
        // Kiểm tra claims tạm thời
        // ==========================
        [HttpGet]
        public IActionResult CheckClaims()
        {
            var claims = User.Claims
                .Select(c => new
                {
                    Type = c.Type,
                    Value = c.Value
                })
                .ToList();

            return Json(claims);
        }
    }
}