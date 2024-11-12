using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.Requests
{
    public class InitialUserRegistrationRequest
    {
        [Required]
        [Phone]
        [Length(14, 14)]
        public string PhoneNumber { get; set; } 
    }
}
