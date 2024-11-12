using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.Requests
{
    public class LoginRequest
    {
        [Required]
        [Length(14, 14)]
        public string PhoneNumber { get; set; }
        [Required]
        [MinLength(6, ErrorMessage = "Invalid Password")]
        public string Password { get; set; }
    }
}
