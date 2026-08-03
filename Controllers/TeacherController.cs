using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS.DatabaseContext;
using SIMS.DatabaseContext.Entities;

namespace SIMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TeacherController : Controller
    {
        private readonly SimsDbContext _context;

        public TeacherController(SimsDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var teachers = await _context.Teachers
                .OrderBy(t => t.TeacherCode)
                .ToListAsync();

            return View(teachers);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Teacher teacher)
        {
            if (await _context.Teachers.AnyAsync(t =>
                    t.TeacherCode == teacher.TeacherCode))
            {
                ModelState.AddModelError(
                    nameof(teacher.TeacherCode),
                    "Teacher code already exists.");
            }

            if (await _context.Teachers.AnyAsync(t =>
                    t.Email == teacher.Email))
            {
                ModelState.AddModelError(
                    nameof(teacher.Email),
                    "Email already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(teacher);
            }

            teacher.CreatedDate = DateTime.Now;

            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Teacher created successfully.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teachers.FindAsync(id);

            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Teacher teacher)
        {
            if (id != teacher.Id)
            {
                return NotFound();
            }

            if (await _context.Teachers.AnyAsync(t =>
                    t.TeacherCode == teacher.TeacherCode &&
                    t.Id != teacher.Id))
            {
                ModelState.AddModelError(
                    nameof(teacher.TeacherCode),
                    "Teacher code already exists.");
            }

            if (await _context.Teachers.AnyAsync(t =>
                    t.Email == teacher.Email &&
                    t.Id != teacher.Id))
            {
                ModelState.AddModelError(
                    nameof(teacher.Email),
                    "Email already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(teacher);
            }

            try
            {
                _context.Teachers.Update(teacher);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Teacher updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                bool exists = await _context.Teachers
                    .AnyAsync(t => t.Id == teacher.Id);

                if (!exists)
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);

            if (teacher != null)
            {
                _context.Teachers.Remove(teacher);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Teacher deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}