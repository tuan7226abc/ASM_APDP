using Microsoft.EntityFrameworkCore;
using SIMS.DatabaseContext;
using SIMS.DatabaseContext.Entities;
using SIMS.Interfaces;

namespace SIMS.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly SimsDbContext _context;

        public StudentRepository(SimsDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Student>> GetAllAsync()
        {
            return await _context.Students.ToListAsync();
        }

        public async Task<Student?> GetByIdAsync(int id)
        {
            return await _context.Students.FindAsync(id);
        }

        public async Task AddAsync(Student student)
        {
            await _context.Students.AddAsync(student);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Student student)
        {
            _context.Students.Update(student);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
            }
        }

    }
}