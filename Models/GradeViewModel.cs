namespace SIMS.Models
{
    public class GradeViewModel
    {
        public int CourseId { get; set; }

        public string CourseName { get; set; } = string.Empty;

        public string TeacherName { get; set; } = string.Empty;

        public List<GradeStudentViewModel> Students { get; set; }
            = new();
    }

    public class GradeStudentViewModel
    {
        public int StudentId { get; set; }

        public string StudentCode { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public decimal? AssignmentScore { get; set; }

        public decimal? MidtermScore { get; set; }

        public decimal? FinalScore { get; set; }

        public decimal? TotalScore { get; set; }

        public string? Note { get; set; }
    }
}