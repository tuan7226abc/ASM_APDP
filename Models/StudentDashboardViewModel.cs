using SIMS.DatabaseContext.Entities;

namespace SIMS.Models
{
    public class StudentDashboardViewModel
    {
        public string StudentName { get; set; } = string.Empty;

        public int TotalCourses { get; set; }

        public int TodayClasses { get; set; }

        public int TotalGrades { get; set; }

        public List<Schedule> TodaySchedules { get; set; } = new();
    }
}