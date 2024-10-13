using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml.Linq;

namespace AonFreelancing.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public Project(int id, string title)
        {
            Id = id;
            Title = title;
        }
        public override string ToString()
        {
            return $"Project: Id={Id}, Title={Title}";
        }
    }
}
