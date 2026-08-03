namespace SIMS.Models
{
    public class AdminDashboardViewModel
    {
        public int StudentCount { get; set; }

        public int TeacherCount { get; set; }

        public int CourseCount { get; set; }

        public int UserCount { get; set; }

        public int ScheduleCount { get; set; }

        public string CurrentUsername { get; set; } = string.Empty;

        public List<string> ChartLabels { get; set; } = new();

        public List<int> StudentChartData { get; set; } = new();
    }
}