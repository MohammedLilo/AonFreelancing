using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Security.Claims;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Authorize]
    [Route("api/mobile/v1/projects")]
    [ApiController]
    public class ProjectsController : BaseController
    {
        private readonly MainAppContext _mainAppContext;
        private readonly UserManager<User> _userManager;
        public ProjectsController(
            MainAppContext mainAppContext,
            UserManager<User> userManager
            )
        {
            _mainAppContext = mainAppContext;
            _userManager = userManager;
        }


        [Authorize(Roles = "Client")]
        [HttpPost]
        public async Task<IActionResult> PostProject([FromBody] ProjectInputDTO projectInputDTO)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            long clientId = Convert.ToInt64(identity.FindFirst(ClaimTypes.NameIdentifier).Value);
            Project project = new Project(projectInputDTO, clientId);

            await _mainAppContext.Projects.AddAsync(project);
            await _mainAppContext.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, CreateSuccessResponse("Success"));
        }

        [HttpGet("feed")]
        public async Task<IActionResult> GetFeed([FromQuery] string search_query, [FromQuery] string[] qual, [FromQuery] int pageSize = 10, [FromQuery] int pageNumber = 0)
        {
            string normalizedSearchQuery = StringUtils.ReplaceWith(search_query.ToUpper(),"");
            List<ProjectOutDTO> projects;
            if (!qual.IsNullOrEmpty())
            {
                projects = await _mainAppContext.Projects.AsNoTracking().OrderByDescending(p => p.CreatedAt)//load newer projects first
                                                 .Where(p => qual.Contains(p.QualificationName))//filter by qualifications
                                                 .Where(p => p.NormalizedTitle.Contains(normalizedSearchQuery) || p.NormalizedDescription.Contains(normalizedSearchQuery))//search by name or description
                                                 .Skip(pageNumber * pageSize)
                                                 .Take(pageSize)
                                                 .Select(p => new ProjectOutDTO(p))
                                                 .ToListAsync();
                return Ok(CreateSuccessResponse(projects));
            }
            else
            {
                projects = await _mainAppContext.Projects.AsNoTracking().OrderByDescending(p => p.CreatedAt)//load newer projects first
                                                             .Where(p => p.NormalizedTitle.Contains(normalizedSearchQuery) || p.NormalizedDescription.Contains(normalizedSearchQuery))//search by name or description
                                                             .Skip(pageNumber * pageSize)
                                                             .Take(pageSize)
                                                             .Select(p => new ProjectOutDTO(p))
                                                             .ToListAsync();
                return Ok(CreateSuccessResponse(projects));

            }



            return Ok();
        }

        //[HttpGet("{id}")]
        //public IActionResult GetProject(int id)
        //{
        //    var project = _mainAppContext.Projects
        //        .Include(p => p.Client)
        //        .FirstOrDefault(p => p.Id == id);

        //    return Ok(CreateSuccessResponse(project));

        //}


    }
}
