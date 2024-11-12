using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.Requests
{
    public class VerifyPhoneNumberRequest
    {

        [Required]
        [Length(14,14)]
        public string PhoneNumber {  get; set; }

        [Required]
        [Length(6,6)]
        public string OtpCode {  get; set; }
    }
}
