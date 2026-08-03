using System.ComponentModel.DataAnnotations;

namespace SIMS.Models
{
    public class ProfileViewModel
    {
        public int UserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(20)]
        public string? Gender { get; set; }

        public string? StudentCode { get; set; }

        public string? TeacherCode { get; set; }

        public string? Department { get; set; }
    }
}