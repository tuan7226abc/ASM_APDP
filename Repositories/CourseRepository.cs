using Microsoft.EntityFrameworkCore;
using SIMS.DatabaseContext;
using SIMS.DatabaseContext.Entities;
using SIMS.Interfaces;

namespace SIMS.Repositories
{
    public class CourseRepository : ICourseRepository
    {
        private readonly SimsDbContext _context;

        public CourseRepository(SimsDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Course>> GetAllAsync()
        {
            return await _context.Courses.OrderByDescending(c => c.Id).ToListAsync();
        }

        public async Task<Course?> GetByIdAsync(int id)
        {
            return await _context.Courses.FindAsync(id);
        }

        public async Task AddAsync(Course course)
        {
            course.CreatedAt = DateTime.Now;
            course.UpdatedAt = DateTime.Now;
            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Course course)
        {
            course.UpdatedAt = DateTime.Now;
            _context.Courses.Update(course);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }
        }
    }
}