using System.ComponentModel.DataAnnotations;

namespace SIMS.Models
{
    public class GradeSubmissionViewModel
    {
        public int SubmissionId { get; set; }

        public string StudentCode { get; set; } = "";

        public string StudentName { get; set; } = "";

        public string AssignmentTitle { get; set; } = "";

        public string FileName { get; set; } = "";

        public DateTime SubmittedAt { get; set; }

        [Range(0, 10)]
        public decimal? Score { get; set; }

        public string? Feedback { get; set; }
    }
}