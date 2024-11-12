using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AonFreelancing.Models
{
    [Table("TempUsers")]
    public class TempUser
    {
        [Key]
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed {  get; set; }
        public TempUser() { }
        public TempUser(string phoneNumber)
        {
            PhoneNumber = phoneNumber;
        }
    }
}
