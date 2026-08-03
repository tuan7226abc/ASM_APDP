using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS.DatabaseContext;
using SIMS.DatabaseContext.Entities;
using SIMS.Models;

namespace SIMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentAttendanceController : Controller
    {
        private readonly SimsDbContext _context;

        public StudentAttendanceController(SimsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var student = await GetCurrentStudentAsync();

            if (student == null)
            {
                ViewBag.Error =
                    "Your account has not been linked to a student profile.";

                return View(new StudentAttendanceViewModel());
            }

            var attendanceItems = await _context.Attendances
                .Include(a => a.Schedule)
                    .ThenInclude(s => s!.Course)
                .Include(a => a.Schedule)
                    .ThenInclude(s => s!.Teacher)
                .Where(a => a.StudentId == student.Id)
                .OrderByDescending(a => a.AttendanceDate)
                .Select(a => new StudentAttendanceItemViewModel
                {
                    AttendanceDate = a.AttendanceDate,

                    CourseName =
                        a.Schedule != null &&
                        a.Schedule.Course != null
                            ? a.Schedule.Course.CourseName
                            : "N/A",

                    TeacherName =
                        a.Schedule != null &&
                        a.Schedule.Teacher != null
                            ? a.Schedule.Teacher.FullName
                            : "N/A",

                    Room =
                        a.Schedule != null
                            ? a.Schedule.Room
                            : "N/A",

                    Status = a.Status,

                    Note = a.Note
                })
                .ToListAsync();

            int totalSessions = attendanceItems.Count;

            int presentCount = attendanceItems.Count(a =>
                a.Status == "Present");

            int absentCount = attendanceItems.Count(a =>
                a.Status == "Absent");

            int lateCount = attendanceItems.Count(a =>
                a.Status == "Late");

            int excusedCount = attendanceItems.Count(a =>
                a.Status == "Excused");

            decimal attendanceRate = totalSessions == 0
                ? 0
                : Math.Round(
                    (decimal)(presentCount + lateCount) /
                    totalSessions * 100,
                    2);

            var viewModel = new StudentAttendanceViewModel
            {
                TotalSessions = totalSessions,
                PresentCount = presentCount,
                AbsentCount = absentCount,
                LateCount = lateCount,
                ExcusedCount = excusedCount,
                AttendanceRate = attendanceRate,
                Attendances = attendanceItems
            };

            ViewBag.StudentName = student.FullName;

            return View(viewModel);
        }

        private async Task<Student?> GetCurrentStudentAsync()
        {
            string? username = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return null;
            }

            return await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == user.Id);
        }
    }
}