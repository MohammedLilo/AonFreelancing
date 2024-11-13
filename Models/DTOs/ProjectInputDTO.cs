using AonFreelancing.Attributes;
using AonFreelancing.Enums;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace AonFreelancing.Models.DTOs
{
    public class ProjectInputDTO
    {
        [Required]
        [MaxLength(512, ErrorMessage ="Title is too long (greater than 512)")]
        public string Title { get; set; }

        [AllowNull]
        [MaxLength(1024,ErrorMessage = "Description is too long (greater than 1024")]
        public string? Description { get; set; }

        [Required]
        [AllowedValues(["backend", "frontend", "mobile", "uiux", "fullstack"])]
        public string QualificationName { get; set; }

        [Required]
        [Range(1,int.MaxValue)]
        public int Duration { get; set; }//Number of days

        [Required]
        [AllowedValues("PerHour","Fixed",ErrorMessage ="Price type must be one of the predefined values ('PerHour', 'Fixed')")]
        public string PriceType { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public decimal Budget { get; set; }

        //allow only 30 MB of file size
        [MaxFileSize(1024 * 1024 * 30)]
        //allow only these extensions
        [AllowedFileExtensions([".jpg",".jpeg",".png"])]
       public  IFormFile? ImageFile {  get; set; }
    }
}
