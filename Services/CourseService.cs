using SIMS.DatabaseContext.Entities;
using SIMS.Interfaces;

namespace SIMS.Services
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;

        public CourseService(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public async Task<IEnumerable<Course>> GetAllAsync()
        {
            return await _courseRepository.GetAllAsync();
        }

        public async Task<Course?> GetByIdAsync(int id)
        {
            return await _courseRepository.GetByIdAsync(id);
        }

        public async Task AddAsync(Course course)
        {
            await _courseRepository.AddAsync(course);
        }

        public async Task UpdateAsync(Course course)
        {
            await _courseRepository.UpdateAsync(course);
        }

        public async Task DeleteAsync(int id)
        {
            await _courseRepository.DeleteAsync(id);
        }
    }
}