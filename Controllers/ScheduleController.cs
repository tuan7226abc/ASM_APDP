using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SIMS.DatabaseContext;
using SIMS.DatabaseContext.Entities;

namespace SIMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ScheduleController : Controller
    {
        private readonly SimsDbContext _context;

        public ScheduleController(SimsDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var schedules = await _context.Schedules
                .Include(s => s.Course)
                .Include(s => s.Teacher)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            return View(schedules);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadDropDownLists();

            return View(new Schedule
            {
                DayOfWeek = 2,
                StartTime = new TimeSpan(8, 0, 0),
                EndTime = new TimeSpan(10, 0, 0),
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(3)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Schedule schedule)
        {
            ValidateSchedule(schedule);

            if (await HasConflict(schedule))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Teacher or room already has another schedule at this time.");
            }

            if (!ModelState.IsValid)
            {
                await LoadDropDownLists(
                    schedule.CourseId,
                    schedule.TeacherId);

                return View(schedule);
            }

            schedule.CreatedAt = DateTime.Now;

            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Schedule created successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schedule = await _context.Schedules.FindAsync(id);

            if (schedule == null)
            {
                return NotFound();
            }

            await LoadDropDownLists(
                schedule.CourseId,
                schedule.TeacherId);

            return View(schedule);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Schedule schedule)
        {
            if (id != schedule.Id)
            {
                return NotFound();
            }

            ValidateSchedule(schedule);

            if (await HasConflict(schedule, schedule.Id))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Teacher or room already has another schedule at this time.");
            }

            if (!ModelState.IsValid)
            {
                await LoadDropDownLists(
                    schedule.CourseId,
                    schedule.TeacherId);

                return View(schedule);
            }

            _context.Schedules.Update(schedule);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Schedule updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schedule = await _context.Schedules
                .Include(s => s.Course)
                .Include(s => s.Teacher)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (schedule == null)
            {
                return NotFound();
            }

            return View(schedule);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var schedule = await _context.Schedules.FindAsync(id);

            if (schedule != null)
            {
                _context.Schedules.Remove(schedule);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Schedule deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        private void ValidateSchedule(Schedule schedule)
        {
            if (schedule.EndTime <= schedule.StartTime)
            {
                ModelState.AddModelError(
                    nameof(schedule.EndTime),
                    "End time must be later than start time.");
            }

            if (schedule.StartDate.HasValue &&
                schedule.EndDate.HasValue &&
                schedule.EndDate < schedule.StartDate)
            {
                ModelState.AddModelError(
                    nameof(schedule.EndDate),
                    "End date must be later than start date.");
            }
        }

        private async Task<bool> HasConflict(
            Schedule schedule,
            int? ignoredId = null)
        {
            return await _context.Schedules.AnyAsync(s =>
                (!ignoredId.HasValue || s.Id != ignoredId.Value) &&
                s.DayOfWeek == schedule.DayOfWeek &&
                s.StartTime < schedule.EndTime &&
                schedule.StartTime < s.EndTime &&
                (
                    s.TeacherId == schedule.TeacherId ||
                    s.Room == schedule.Room
                ));
        }

        private async Task LoadDropDownLists(
            int? courseId = null,
            int? teacherId = null)
        {
            ViewBag.CourseId = new SelectList(
                await _context.Courses
                    .OrderBy(c => c.CourseName)
                    .ToListAsync(),
                "Id",
                "CourseName",
                courseId);

            ViewBag.TeacherId = new SelectList(
                await _context.Teachers
                    .OrderBy(t => t.FullName)
                    .ToListAsync(),
                "Id",
                "FullName",
                teacherId);
        }
    }
}