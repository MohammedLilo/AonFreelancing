using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class ClientDTO:UserOutDTO
    {
        public string CompanyName { get; set; }

        // Has many projects, 1-m
        public IEnumerable<ProjectOutDTO> Projects { get; set; }
    }


    public class ClientProfileDTO : UserProfileDTO
    {
        public string CompanyName { get; set; }
        public IEnumerable<ProjectProfileDTO>? Projects { get; set; }

        public ClientProfileDTO() { }
        public ClientProfileDTO(Client client)
        : base(client)
        {
            CompanyName = client.CompanyName;
            Projects = client.Projects?.Select(p => new ProjectProfileDTO(p));
        }

    }
}
