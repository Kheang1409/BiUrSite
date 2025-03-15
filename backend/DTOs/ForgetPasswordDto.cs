using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    public class ForgetPasswordDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public required string email { get; set; }
    }
}
