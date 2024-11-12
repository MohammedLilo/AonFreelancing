using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.Requests
{
    public class UserRegistrationRequest

    {
        [Required]
        [Length(14, 14)]
        public string PhoneNumber { get; set; }

        [Required]
        [MinLength(2)]
        public string Name { get; set; }

        [Required]
        [MinLength(4)]
        public string Email { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Too short password")]
        public string Password { get; set; }

        [Required]
        [AllowedValues("Freelancer", "Client")]
        public string UserType { get; set; }

        // For freelancer type only
        public string? Skills { get; set; }

        // For Client user type 
        public string? CompanyName { get; set; }

    }
}
