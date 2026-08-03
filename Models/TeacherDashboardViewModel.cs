using SIMS.DatabaseContext.Entities;

namespace SIMS.Models
{
    public class TeacherDashboardViewModel
    {
        public bool HasTeacherProfile { get; set; }

        public int TeacherId { get; set; }

        public string TeacherName { get; set; }
            = string.Empty;

        public string TeacherCode { get; set; }
            = string.Empty;

        public int CourseCount { get; set; }

        public int StudentCount { get; set; }

        public int TodayClassCount { get; set; }

        public int AssignmentCount { get; set; }

        public List<Schedule> TodaySchedules { get; set; }
            = new();
    }
}