using SIMS.DatabaseContext.Entities;

namespace SIMS.Models
{
    public class TeacherDashboardViewModel
    {
        public string TeacherName { get; set; } = string.Empty;

        public int TotalCourses { get; set; }

        public int TotalStudents { get; set; }

        public int TodayClasses { get; set; }

        public List<Schedule> TodaySchedules { get; set; } = new();
    }
}