using System.ComponentModel.DataAnnotations;

namespace SIMS.Models
{
    public class AttendanceViewModel
    {
        public int ScheduleId { get; set; }

        public string CourseName { get; set; } = string.Empty;

        public string TeacherName { get; set; } = string.Empty;

        public string Room { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime AttendanceDate { get; set; } = DateTime.Today;

        public List<AttendanceStudentViewModel> Students { get; set; }
            = new();
    }

    public class AttendanceStudentViewModel
    {
        public int StudentId { get; set; }

        public string StudentCode { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Status { get; set; } = "Present";

        public string? Note { get; set; }
    }
}