using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Models.Requests;
using AonFreelancing.Models.Responses;
using AonFreelancing.Services;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Identity;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net;


namespace AonFreelancing.Controllers.Mobile.v1
{
    [Route("api/mobile/v1/auth")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly MainAppContext _mainAppContext;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly OTPManager _otpManager;
        private readonly JwtService _jwtService;
        public AuthController(
            UserManager<User> userManager,
            MainAppContext mainAppContext,
            RoleManager<ApplicationRole> roleManager,
            IConfiguration configuration,
            OTPManager otpManager,
            JwtService jwtService
            )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mainAppContext = mainAppContext;
            _configuration = configuration;
            _otpManager = otpManager;
            _jwtService = jwtService;
        }

        [HttpPost("register/phone-number")]
        public async Task<IActionResult> RegisterWithPhoneNumberAsync([FromBody][Required][Phone] string phoneNumber)
        {
            if (await _userManager.Users.Where(u => u.PhoneNumber == phoneNumber).FirstOrDefaultAsync() != null)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), "phone number is already used by an account"));

            // Name and UserName are set to phoneNumber because currently it is the only unique value we have and they are required. 
            User user = new User { PhoneNumber = phoneNumber, Name = phoneNumber,UserName = phoneNumber};
            var userCreationResult = await _userManager.CreateAsync(user);
            
            if (userCreationResult.Succeeded)
            {
                string otpCode = _otpManager.GenerateOtp();
                OTP otp = new OTP(phoneNumber, otpCode, Convert.ToInt32(_configuration["Otp:ExpireInMinutes"]));
                await _mainAppContext.OTPs.AddAsync(otp);
                await _mainAppContext.SaveChangesAsync();

                await _otpManager.SendOTPAsync(otpCode, phoneNumber);
 
                return StatusCode(
                                   StatusCodes.Status201Created,
                                   CreateSuccessResponse(new Dictionary<string, long> { { "id", user.Id } })
                                  );
            }
            return StatusCode(
                               StatusCodes.Status500InternalServerError,
                               CreateErrorResponse("500", "An error occured while processing the request")
                                );
        }
        [HttpPost("register/verify-phone-number")]
        public async Task<IActionResult> VerifyAsync([FromBody] VerifyReq verifyReq)
        {
            var user = await _userManager.Users.Where(x => x.PhoneNumber == verifyReq.Phone).FirstOrDefaultAsync();
            if (user != null && !await _userManager.IsPhoneNumberConfirmedAsync(user))
            {
                OTP? otp = await _mainAppContext.OTPs.Where(o => o.PhoneNumber == verifyReq.Phone).FirstOrDefaultAsync();

                // verify OTP
                if (otp != null && verifyReq.Otp.Equals(otp.Code) && DateTime.Now < otp.ExpiresAt)
                {
                    user.PhoneNumberConfirmed = true;
                    await _userManager.UpdateAsync(user);

                    // disable sent OTP
                    otp.IsUsed = true;
                    await _mainAppContext.SaveChangesAsync();

                    return Ok(CreateSuccessResponse("Activated"));
                }
            }
            return Unauthorized(CreateErrorResponse(StatusCodes.Status401Unauthorized.ToString(), "UnAuthorized"));
        }
        [HttpPost("register/details")]
        //TODO replace username with email
        //TODO replace username with email
        //TODO replace username with email
        //TODO replace username with email
        //TODO introduce a new property in User.cs to mark a user as fully registered
        public async Task<IActionResult> CompleteRegister(UserRegistrationRequest regRequest)
        {
            User? user = await _userManager.FindByIdAsync(regRequest.UserId.ToString());
            if (user == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(),"account not found"));
            
            //check if account has already completed registration.
            if (user.FullyRegistered)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(),
                                    "this account registration is already completed")
                                    );
            user.Email = regRequest.Email;
            user.Name = regRequest.Name;
            user.FullyRegistered = true;

           var updatePasswordResult =  await _userManager.AddPasswordAsync(user,regRequest.Password);
            if (!updatePasswordResult.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>()
                {
                    Errors = updatePasswordResult.Errors
                    .Select(e => new Error()
                    {
                        Code = e.Code,
                        Message = e.Description
                    })
                    .ToList()
                });

            var userUpdateResult =  await _userManager.UpdateAsync(user);
            if (!userUpdateResult.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>()
                {
                    Errors = userUpdateResult.Errors
                    .Select(e => new Error()
                    {
                        Code = e.Code,
                        Message = e.Description
                    })
                    .ToList()
                });
            //this line is crucial for the current implementation, omitting it will cause System.InvalidOperationException: 'The instance of entity type 'Client' cannot be tracked because another instance with the key value '{Id: 15}' is already being tracked. When attaching existing entities, ensure that only one entity instance with a given key value is attached.'
            _mainAppContext.Remove(user);

            if (regRequest.UserType == Constants.USER_TYPE_FREELANCER)
            {
                user = new Freelancer(user) { Skills = regRequest.Skills };
            }
            else if (regRequest.UserType == Constants.USER_TYPE_CLIENT)
            {
                user = new Client(user) { CompanyName = regRequest.CompanyName };
            }
            await _mainAppContext.AddAsync(user);
            await _mainAppContext.SaveChangesAsync();

            var role = await _roleManager.FindByNameAsync(regRequest.UserType);
            await _userManager.AddToRoleAsync(user, role.Name);

            return CreatedAtAction(nameof(UsersController.GetProfileById), "Users", new { id = user.Id }, null);
        }

        //[ApiExplorerSettings(IgnoreApi = true)] to hide it from swagger UI
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegRequest regRequest)
        {

            User user = new User();
            if (regRequest.UserType == Constants.USER_TYPE_FREELANCER)
            {
                // Register User
                user = new Freelancer
                {
                    Name = regRequest.Name,
                    UserName = regRequest.Username,
                    PhoneNumber = regRequest.PhoneNumber,
                    Skills = regRequest.Skills
                };
            }
            else if (regRequest.UserType == Constants.USER_TYPE_CLIENT)
            {
                user = new Client
                {
                    Name = regRequest.Name,
                    UserName = regRequest.Username,
                    PhoneNumber = regRequest.PhoneNumber,
                    CompanyName = regRequest.CompanyName
                };
            }
            //check if username or phoneNumber is taken
            if (await _userManager.Users.Where(u => u.UserName == regRequest.Username || u.PhoneNumber == regRequest.PhoneNumber).FirstOrDefaultAsync() != null)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), "username or phone number is already used by an account"));

            //create new User with hashed passworrd in the database
            var userCreationResult = await _userManager.CreateAsync(user, regRequest.Password);
            if (!userCreationResult.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>()
                {
                    Errors = userCreationResult.Errors
                    .Select(e => new Error()
                    {
                        Code = e.Code,
                        Message = e.Description
                    })
                    .ToList()
                });

            // To be fixed
            //assign a role to the newly created User
            //var role = new ApplicationRole { Name = regRequest.UserType };
            //await _roleManager.CreateAsync(role);
            var role = await _roleManager.FindByNameAsync(regRequest.UserType);
            await _userManager.AddToRoleAsync(user, role.Name);

            string otpCode = _otpManager.GenerateOtp();
            //persist the otp to the otps table
            OTP otp = new OTP()
            {
                Code = otpCode,
                PhoneNumber = regRequest.PhoneNumber,
                CreatedDate = DateTime.Now,
                ExpiresAt = DateTime.Now.AddMinutes(1),
            };
            await _mainAppContext.OTPs.AddAsync(otp);
            await _mainAppContext.SaveChangesAsync();

            //send the otp to the specified phone number
            await _otpManager.SendOTPAsync(otp.Code, otp.PhoneNumber);

            // Get created User (if it is a freelancer)
            if (regRequest.UserType == Constants.USER_TYPE_FREELANCER)
            {
                var createdUser = await _mainAppContext.Users.OfType<Freelancer>()
                        .Where(u => u.UserName == regRequest.Username)
                        .Select(u => new FreelancerResponseDTO()
                        {
                            Id = u.Id,
                            Name = u.Name,
                            Username = u.UserName,
                            PhoneNumber = u.PhoneNumber,
                            Skills = u.Skills,
                            UserType = Constants.USER_TYPE_FREELANCER,
                            Role = new RoleResponseDTO { Id = role.Id, Name = role.Name }
                        })
                        .FirstOrDefaultAsync();
                return Ok(CreateSuccessResponse(createdUser));
            }
            // Get created User (if it is a client)
            else if (regRequest.UserType == Constants.USER_TYPE_CLIENT)
            {
                var createdUser = await _mainAppContext.Users.OfType<Client>()
                          .Where(c => c.UserName == regRequest.Username)
                          .Select(c => new ClientResponseDTO
                          {
                              Id = c.Id,
                              Name = c.Name,
                              Username = c.UserName,
                              PhoneNumber = c.PhoneNumber,
                              CompanyName = c.CompanyName,
                              UserType = Constants.USER_TYPE_CLIENT,
                              Role = new RoleResponseDTO { Id = role.Id, Name = role.Name }
                          })
                          .FirstOrDefaultAsync();
                return Ok(CreateSuccessResponse(createdUser));
            }
            //this fallback return value will not be returned due to model validation.
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] AuthRequest Req)
        {
            var user = await _userManager.FindByNameAsync(Req.UserName);
            if (user != null && await _userManager.CheckPasswordAsync(user, Req.Password))
            {
                if (!await _userManager.IsPhoneNumberConfirmedAsync(user))
                    return Unauthorized(CreateErrorResponse(StatusCodes.Status401Unauthorized.ToString(), "Verify Your Account First"));

                var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
                var token = _jwtService.GenerateJWT(user, role);
                return Ok(CreateSuccessResponse(new LoginResponse()
                {
                    AccessToken = token,
                    UserDetailsDTO = new UserDetailsDTO(user, role)
                }));
            }

            return Unauthorized(CreateErrorResponse(StatusCodes.Status401Unauthorized.ToString(), "UnAuthorized"));
        }

       
        //[HttpPost("forgotpassword")]
        //public async Task<IActionResult> ForgotPasswordMethod([FromBody] ForgotPasswordReq forgotPasswordRequest)
        //{
        //    var user = await _userManager.Users.Where(u => u.PhoneNumber == forgotPasswordRequest.PhoneNumber).FirstOrDefaultAsync();
        //    if (user != null)
        //    {
        //        string token = await _userManager.GeneratePasswordResetTokenAsync(user);

        //        //send whatsapp  message containing the reset password url
        //        await _otpManager.sendForgotPasswordMessageAsync(token, user.PhoneNumber);
        //    }
        //    // to maintain confidentiality, we always return an OK response even if the user was not found. 
        //    return Ok(CreateSuccessResponse("Check your WhatsApp inbox for password reset token."));
        //}

        //[HttpPost("resetpassword")]
        //public async Task<IActionResult> ResetPasswordMethod([FromBody] ResetPasswordReq resetPasswordReq)
        //{
        //    User? user = await _userManager.Users.Where(u => u.PhoneNumber == resetPasswordReq.PhoneNumber).FirstOrDefaultAsync();
        //    if (user != null)
        //        await _userManager.ResetPasswordAsync(user, resetPasswordReq.Token, resetPasswordReq.Password);

        //    // to maintain confidentiality, we always return an OK response even if the user was not found. 
        //    return Ok(CreateSuccessResponse("Your password have been reset"));
        //}
    }
}
