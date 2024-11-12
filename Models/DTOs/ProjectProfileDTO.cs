using System.Data;

namespace AonFreelancing.Models.DTOs
{
    public class ProjectProfileDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public ProjectProfileDTO() { }
        public ProjectProfileDTO(Project project)
        {
            Id = project.Id;
            Name = project.Title;
            Description = project.Description;
            StartDate = project.StartDate;
            EndDate = project.EndDate;
        }



    }
}
