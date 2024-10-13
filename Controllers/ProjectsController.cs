using AonFreelancing.Models;
using Microsoft.AspNetCore.Mvc;

namespace AonFreelancing.Controllers;

[ApiController]
[Route("api/v1/projects")]
public class ProjectsController : ControllerBase
{
    static readonly List<Project> _projects = [];

    [HttpGet]
    public IActionResult GetProjects()
    {
        return Ok(_projects);
    }

    [HttpGet("{id}")]
    public IActionResult GetProject(int id)
    {
        Project? project = _projects.Find(x => x.Id == id);

        if (project == null)
            return NotFound();

        return Ok(project);
    }

    [HttpPost]
    public IActionResult CreateProject([FromBody] Project project) {
        if (project == null)
            return BadRequest();

        _projects.Add(project);
        return CreatedAtAction(nameof(GetProject), new { id = project.Id}, _projects);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteProject(int id)
    {
        Project? project = _projects.Find(x => x.Id == id);
        if (project == null)
            return NotFound();

        _projects.Remove(project);

        //return Ok($"deleted {project}");
        return NoContent();
    }

}
