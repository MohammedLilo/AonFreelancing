using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AonFreelancing.Models
{
    [Table("Freelancers")]
    public class Freelancer : User
    {
   
        public string Skills { get; set; }

        public Freelancer() { }
        public Freelancer(User user)
        {
            Id = user.Id;
            AccessFailedCount = user.AccessFailedCount;
            Email = user.Email;
            EmailConfirmed = user.EmailConfirmed;
            LockoutEnabled = user.LockoutEnabled;
            LockoutEnd = user.LockoutEnd;
            Name = user.Name;
            NormalizedEmail = user.NormalizedEmail;
            UserName = user.UserName;
            NormalizedUserName = user.NormalizedUserName;
            PhoneNumber = user.PhoneNumber;
            PhoneNumberConfirmed = user.PhoneNumberConfirmed;
            PasswordHash = user.PasswordHash;
            ConcurrencyStamp = user.ConcurrencyStamp;
            SecurityStamp = user.SecurityStamp;
            TwoFactorEnabled = user.TwoFactorEnabled;
            FullyRegistered = user.FullyRegistered;
        }

        //public override void DisplayProfile()
        //{
        //    Console.WriteLine($"Overrided Method in Freelancer Class");
        //}

    }


}
