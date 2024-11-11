using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AonFreelancing.Models
{
    [Table("otps")]
    public class OTP
    {
        [Key]
        public string PhoneNumber { get; set; }

        public string Code { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }

        public OTP() { }
        public OTP(string phoneNumber, string code, int expireInMinutes)
        {
            Code = code;
            PhoneNumber = phoneNumber;
            CreatedDate = DateTime.Now;
            ExpiresAt = DateTime.Now.AddMinutes(expireInMinutes);
        }
    }
}
