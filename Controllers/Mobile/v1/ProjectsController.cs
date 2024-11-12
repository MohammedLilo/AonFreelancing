using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
