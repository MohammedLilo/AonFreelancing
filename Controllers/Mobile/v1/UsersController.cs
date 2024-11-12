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
using System.Security.Claims;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Authorize]
    [Route("api/mobile/v1/users")]
    [ApiController]
    public class UsersController : BaseController
    {
        private readonly MainAppContext _mainAppContext;
        private readonly RoleManager<ApplicationRole> _roleManager;
        public UsersController(MainAppContext mainAppContext, RoleManager<ApplicationRole> roleManager)
        {
            _mainAppContext = mainAppContext;
            _roleManager = roleManager;
        }

        [HttpGet("{id}/profile")]
        public async Task<IActionResult> GetProfileById([FromRoute] long id)
        {
            var freelancerProfileDTO = await _mainAppContext.Users.OfType<Freelancer>()
                                                                .Where(f => f.Id == id)
                                                                .Select(f => new FreelancerProfileDTO(f))
                                                                .FirstOrDefaultAsync();
            if (freelancerProfileDTO != null)
                return Ok(CreateSuccessResponse(freelancerProfileDTO));


            var clientProfileDTO = await _mainAppContext.Users.OfType<Client>()
                                                                .Where(c => c.Id == id)
                                                                .Include(c => c.Projects)
                                                                .Select(c => new ClientProfileDTO(c))
                                                                .FirstOrDefaultAsync();
            if (clientProfileDTO != null)
                return Ok(CreateSuccessResponse(clientProfileDTO));

            return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "User not found"));
        }

    }
}
