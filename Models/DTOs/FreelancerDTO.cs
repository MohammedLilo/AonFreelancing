using AonFreelancing.Utilities;

namespace AonFreelancing.Models.DTOs
{
    public class FreelancerDTO:UserDTO
    {

        public string Skills { get; set; }
    }


    public class FreelancerProfileDTO : UserProfileDTO { 
      
        public string? Skills { get; set; }

        public FreelancerProfileDTO() { }
        public FreelancerProfileDTO(Freelancer freelancer)
        : base(freelancer)
        {
            Skills = freelancer.Skills;
        }
    }
}
