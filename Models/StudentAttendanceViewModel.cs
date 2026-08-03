namespace SIMS.Models
{
    public class StudentAttendanceViewModel
    {
        public int TotalSessions { get; set; }

        public int PresentCount { get; set; }

        public int AbsentCount { get; set; }

        public int LateCount { get; set; }

        public int ExcusedCount { get; set; }

        public decimal AttendanceRate { get; set; }

        public List<StudentAttendanceItemViewModel> Attendances { get; set; }
            = new();
    }

    public class StudentAttendanceItemViewModel
    {
        public DateTime AttendanceDate { get; set; }

        public string CourseName { get; set; } = string.Empty;

        public string TeacherName { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string? Note { get; set; }

        public string Room { get; set; } = string.Empty;
    }
}