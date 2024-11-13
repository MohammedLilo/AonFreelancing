﻿using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Services;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
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
        readonly FileStorageService _fileStorageService;
        public ProjectsController(
            MainAppContext mainAppContext,
            UserManager<User> userManager,
            FileStorageService fileStorageService
            )
        {
            _mainAppContext = mainAppContext;
            _userManager = userManager;
            _fileStorageService = fileStorageService;
        }


        [Authorize(Roles = "Client")]
        [HttpPost]
        public async Task<IActionResult> PostProject( ProjectInputDTO projectInputDTO)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            long clientId = Convert.ToInt64(identity.FindFirst(ClaimTypes.NameIdentifier).Value);
            Project project = new Project(projectInputDTO, clientId);

            if (projectInputDTO.ImageFile != null)
            {
                string fileName = await _fileStorageService.SaveAsync(projectInputDTO.ImageFile);
                project.ImageFileName = fileName;
            }
            await _mainAppContext.Projects.AddAsync(project);
            await _mainAppContext.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, CreateSuccessResponse("Success"));
        }
        
        [HttpGet("feed")]
        public async Task<IActionResult> GetFeed([FromQuery] string[] qual, [FromQuery] string search_query="", [FromQuery] int pageSize = 10, [FromQuery] int pageNumber = 0)
        {
            var imagesBaseUrl = $"{Request.Scheme}://{Request.Host}/images";
            string normalizedSearchQuery = StringUtils.ReplaceWith(search_query.ToUpper(),"");
            List<ProjectOutDTO> projects;
            if (!qual.IsNullOrEmpty())
            {
                projects = await _mainAppContext.Projects.AsNoTracking().OrderByDescending(p => p.CreatedAt)//load newer projects first
                                                 .Where(p => qual.Contains(p.QualificationName))//filter by qualifications
                                                 .Where(p => p.NormalizedTitle.Contains(normalizedSearchQuery) || p.NormalizedDescription.Contains(normalizedSearchQuery))//search by name or description
                                                 .Skip(pageNumber * pageSize)
                                                 .Take(pageSize)
                                                 .Select(p => new ProjectOutDTO(p,imagesBaseUrl))
                                                 .ToListAsync();
            }
            else
            {
                projects = await _mainAppContext.Projects.AsNoTracking().OrderByDescending(p => p.CreatedAt)//load newer projects first
                                                             .Where(p => p.NormalizedTitle.Contains(normalizedSearchQuery) || p.NormalizedDescription.Contains(normalizedSearchQuery))//search by name or description
                                                             .Skip(pageNumber * pageSize)
                                                             .Take(pageSize)
                                                             .Select(p => new ProjectOutDTO(p, imagesBaseUrl))
                                                             .ToListAsync();
            }
            if(!projects.IsNullOrEmpty())
                return Ok(CreateSuccessResponse(projects));
            
            return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "No projects where found"));
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
