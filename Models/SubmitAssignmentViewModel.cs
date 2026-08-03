using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SIMS.Models
{
    public class SubmitAssignmentViewModel
    {
        public int AssignmentId { get; set; }

        public string AssignmentTitle { get; set; } = string.Empty;

        public string CourseName { get; set; } = string.Empty;

        public DateTime DueDate { get; set; }

        [Required(ErrorMessage = "Please select a PDF file.")]
        public IFormFile? PdfFile { get; set; }
    }
}