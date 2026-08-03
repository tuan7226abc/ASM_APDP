using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS.DatabaseContext;
using SIMS.DatabaseContext.Entities;
using SIMS.Models;

namespace SIMS.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherAttendanceController : Controller
    {
        private readonly SimsDbContext _context;

        public TeacherAttendanceController(SimsDbContext context)
        {
            _context = context;
        }

        // Danh sách lịch dạy của Teacher đang đăng nhập
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var teacher = await GetCurrentTeacherAsync();

            if (teacher == null)
            {
                TempData["Error"] =
                    "Your teacher account has not been linked to a teacher profile.";

                return View(new List<Schedule>());
            }

            var schedules = await _context.Schedules
                .Include(s => s.Course)
                .Where(s => s.TeacherId == teacher.Id)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            return View(schedules);
        }

        // Hiện danh sách sinh viên để điểm danh
        [HttpGet]
        public async Task<IActionResult> Take(
            int scheduleId,
            DateTime? date)
        {
            var teacher = await GetCurrentTeacherAsync();

            if (teacher == null)
            {
                return Forbid();
            }

            var schedule = await _context.Schedules
                .Include(s => s.Course)
                .Include(s => s.Teacher)
                .FirstOrDefaultAsync(s =>
                    s.Id == scheduleId &&
                    s.TeacherId == teacher.Id);

            if (schedule == null)
            {
                return NotFound();
            }

            DateTime attendanceDate = date?.Date ?? DateTime.Today;

            var students = await (
    from cs in _context.CourseStudents
    join student in _context.Students
        on cs.StudentId equals student.Id
    where cs.CourseId == schedule.CourseId
    orderby student.FullName
    select student
).ToListAsync();

            var existingAttendances = await _context.Attendances
                .Where(a =>
                    a.ScheduleId == scheduleId &&
                    a.AttendanceDate == attendanceDate)
                .ToDictionaryAsync(
                    a => a.StudentId,
                    a => a);

            var viewModel = new AttendanceViewModel
            {
                ScheduleId = schedule.Id,
                CourseName = schedule.Course?.CourseName ?? "N/A",
                TeacherName = schedule.Teacher?.FullName ?? "N/A",
                Room = schedule.Room,
                AttendanceDate = attendanceDate
            };

            foreach (var student in students)
            {
                existingAttendances.TryGetValue(
                    student.Id,
                    out Attendance? attendance);

                viewModel.Students.Add(
                    new AttendanceStudentViewModel
                    {
                        StudentId = student.Id,
                        StudentCode = student.StudentCode,
                        FullName = student.FullName,
                        Status = attendance?.Status ?? "Present",
                        Note = attendance?.Note
                    });
            }

            return View(viewModel);
        }

        // Lưu điểm danh
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Take(
            AttendanceViewModel viewModel)
        {
            var teacher = await GetCurrentTeacherAsync();

            if (teacher == null)
            {
                return Forbid();
            }

            var schedule = await _context.Schedules
                .Include(s => s.Course)
                .Include(s => s.Teacher)
                .FirstOrDefaultAsync(s =>
                    s.Id == viewModel.ScheduleId &&
                    s.TeacherId == teacher.Id);

            if (schedule == null)
            {
                return NotFound();
            }

            string[] validStatuses =
            {
                "Present",
                "Absent",
                "Late",
                "Excused"
            };

            if (viewModel.AttendanceDate == default)
            {
                ModelState.AddModelError(
                    nameof(viewModel.AttendanceDate),
                    "Attendance date is required.");
            }

            foreach (var studentItem in viewModel.Students)
            {
                if (!validStatuses.Contains(studentItem.Status))
                {
                    ModelState.AddModelError(
                        string.Empty,
                        $"Invalid attendance status for {studentItem.FullName}.");
                }
            }

            if (!ModelState.IsValid)
            {
                viewModel.CourseName =
                    schedule.Course?.CourseName ?? "N/A";

                viewModel.TeacherName =
                    schedule.Teacher?.FullName ?? "N/A";

                viewModel.Room = schedule.Room;

                return View(viewModel);
            }

            DateTime attendanceDate =
                viewModel.AttendanceDate.Date;

            foreach (var studentItem in viewModel.Students)
            {
                bool belongsToCourse =
                    await _context.CourseStudents.AnyAsync(cs =>
                        cs.CourseId == schedule.CourseId &&
                        cs.StudentId == studentItem.StudentId);

                if (!belongsToCourse)
                {
                    continue;
                }

                var attendance =
                    await _context.Attendances.FirstOrDefaultAsync(a =>
                        a.ScheduleId == schedule.Id &&
                        a.StudentId == studentItem.StudentId &&
                        a.AttendanceDate == attendanceDate);

                if (attendance == null)
                {
                    attendance = new Attendance
                    {
                        ScheduleId = schedule.Id,
                        StudentId = studentItem.StudentId,
                        AttendanceDate = attendanceDate,
                        Status = studentItem.Status,
                        Note = studentItem.Note?.Trim(),
                        CreatedAt = DateTime.Now
                    };

                    _context.Attendances.Add(attendance);
                }
                else
                {
                    attendance.Status = studentItem.Status;
                    attendance.Note = studentItem.Note?.Trim();
                    attendance.UpdatedAt = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Attendance saved successfully.";

            return RedirectToAction(
                nameof(Take),
                new
                {
                    scheduleId = viewModel.ScheduleId,
                    date = attendanceDate.ToString("yyyy-MM-dd")
                });
        }

        private async Task<Teacher?> GetCurrentTeacherAsync()
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

            return await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == user.Id);
        }
    }
}