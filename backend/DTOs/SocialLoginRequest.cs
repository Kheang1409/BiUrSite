using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    public class SocialLoginRequest
    {
        [Required(ErrorMessage ="Provider is required.")]
        public required string provider { get; set; }
        [Required(ErrorMessage = "Token is required.")]
        public required string token  { get; set; }
        
    }
}
