using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "Otp is required.")]
        public required string Otp { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public required string NewPassword { get; set; }
    }
}
