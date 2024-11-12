using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace AonFreelancing.Models
{
    //Entity
    [Table("Projects")]
    public class Project
    {

        public int Id { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }

        public long ClientId { get; set; }//FK

        // Belongs to a client
        [ForeignKey("ClientId")]
        public Client Client { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string PriceType { get; set; }

        public int Duration { get; set; }

        public string QualificationName { get; set; }

        public decimal Budget {  get; set; }

        public long? FreelancerId { get; set; }

        [ForeignKey("FreelancerId")]
        public Freelancer? Freelancer { get; set; }



    }
}
