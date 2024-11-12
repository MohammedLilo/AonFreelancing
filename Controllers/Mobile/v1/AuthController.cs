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
using System.Text;


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
        public async Task<IActionResult> RegisterWithPhoneNumberAsync([FromBody] InitialUserRegistrationRequest initialRequest)
        {
            if (await _mainAppContext.Users.Where(u => u.PhoneNumber == initialRequest.PhoneNumber).FirstOrDefaultAsync() != null)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), "phone number is already used by an account"));
           
            if(await _mainAppContext.OTPs.Where(o=>o.PhoneNumber == initialRequest.PhoneNumber).FirstOrDefaultAsync() != null)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), "otp is already sent"));

            string otpCode = _otpManager.GenerateOtp();
            OTP otp = new OTP(initialRequest.PhoneNumber, otpCode, Convert.ToInt32(_configuration["Otp:ExpireInMinutes"]));
            
            await _mainAppContext.TempUsers.AddAsync(new TempUser(initialRequest.PhoneNumber));
            await _mainAppContext.OTPs.AddAsync(otp);
            await _mainAppContext.SaveChangesAsync();

            await _otpManager.SendOTPAsync(otpCode, initialRequest.PhoneNumber);

            //include the expiration time in the response to display a count down.
            return Ok(CreateSuccessResponse(otp.ExpiresAt));

        }
        [HttpPost("register/verify-phone-number")]
        public async Task<IActionResult> VerifyAsync([FromBody] VerifyPhoneNumberRequest verifyPhoneNumberRequest)
        {
            TempUser? storedTempUser = await _mainAppContext.TempUsers.Where(tu => tu.PhoneNumber == verifyPhoneNumberRequest.PhoneNumber).FirstOrDefaultAsync();
            if(storedTempUser == null)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(),"invalid phone number"));

            OTP? otp = await _mainAppContext.OTPs.Where(o=>o.PhoneNumber == verifyPhoneNumberRequest.PhoneNumber).FirstOrDefaultAsync();


            // verify OTP
            if (otp != null && DateTime.Now < otp.ExpiresAt && !otp.IsUsed &&otp.Code.Equals(verifyPhoneNumberRequest.OtpCode))
            {
                storedTempUser.PhoneNumberConfirmed = true;
                otp.IsUsed = true; // disable sent OTP
                await _mainAppContext.SaveChangesAsync();

                return Ok(CreateSuccessResponse("Activated"));
            }

            return Unauthorized(CreateErrorResponse(StatusCodes.Status401Unauthorized.ToString(), "UnAuthorized"));
        }

        [HttpPost("register/details")]
        public async Task<IActionResult> CompleteRegister(UserRegistrationRequest regRequest)
        {
            User? user = await _mainAppContext.Users.Where(u=>u.PhoneNumber == regRequest.PhoneNumber).FirstOrDefaultAsync();
            if (user != null) //check if the provided phone number is already used.
                return Unauthorized(CreateErrorResponse(StatusCodes.Status401Unauthorized.ToString(), "phone number is already used"));
            
            TempUser? storedTempUser = await _mainAppContext.TempUsers.Where(tu=>tu.PhoneNumber == regRequest.PhoneNumber).FirstOrDefaultAsync();
            if(storedTempUser == null)//check if the request is associated with a temp user record.
                return Unauthorized(CreateErrorResponse(StatusCodes.Status401Unauthorized.ToString(), "submit and verify your phone number before registering your details"));


            if (regRequest.UserType == Constants.USER_TYPE_FREELANCER) 
                user = new Freelancer(regRequest);

            else if (regRequest.UserType == Constants.USER_TYPE_CLIENT)
                user = new Client(regRequest);

            string normalizedName = StringUtils.ReplaceWhitespace(user.Name.ToLower(), "");
            string username;
            do
            {
                username =normalizedName + new Random().Next(999_999);
            } while (await _mainAppContext.Users.AnyAsync(u => u.UserName == username));
            
            user.UserName = username;
            user.PhoneNumberConfirmed = true;
            var userCreationResult =  await _userManager.CreateAsync(user,regRequest.Password);
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
            
            var role = await _roleManager.FindByNameAsync(regRequest.UserType);
            await _userManager.AddToRoleAsync(user, role.Name);

            return CreatedAtAction(nameof(UsersController.GetProfileById), "Users", new { id = user.Id }, null);
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest loginReq)
        {
            var user = await _mainAppContext.Users.Where(u => u.PhoneNumber == loginReq.PhoneNumber).FirstOrDefaultAsync();
            if (user != null && await _userManager.CheckPasswordAsync(user, loginReq.Password))
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

        //[ApiExplorerSettings(IgnoreApi = true)]// to hide it only from swagger
        //[HttpPost("register")]
        //public async Task<IActionResult> RegisterAsync([FromBody] RegRequest regRequest)
        //{

        //    User user = new User();
        //    if (regRequest.UserType == Constants.USER_TYPE_FREELANCER)
        //    {
        //        // Register User
        //        user = new Freelancer
        //        {
        //            Name = regRequest.Name,
        //            UserName = regRequest.Username,
        //            PhoneNumber = regRequest.PhoneNumber,
        //            Skills = regRequest.Skills
        //        };
        //    }
        //    else if (regRequest.UserType == Constants.USER_TYPE_CLIENT)
        //    {
        //        user = new Client
        //        {
        //            Name = regRequest.Name,
        //            UserName = regRequest.Username,
        //            PhoneNumber = regRequest.PhoneNumber,
        //            CompanyName = regRequest.CompanyName
        //        };
        //    }
        //    //check if username or phoneNumber is taken
        //    if (await _userManager.Users.Where(u => u.UserName == regRequest.Username || u.PhoneNumber == regRequest.PhoneNumber).FirstOrDefaultAsync() != null)
        //        return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), "username or phone number is already used by an account"));

        //    //create new User with hashed passworrd in the database
        //    var userCreationResult = await _userManager.CreateAsync(user, regRequest.Password);
        //    if (!userCreationResult.Succeeded)
        //        return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>()
        //        {
        //            Errors = userCreationResult.Errors
        //            .Select(e => new Error()
        //            {
        //                Code = e.Code,
        //                Message = e.Description
        //            })
        //            .ToList()
        //        });

        //    // To be fixed
        //    //assign a role to the newly created User
        //    //var role = new ApplicationRole { Name = regRequest.UserType };
        //    //await _roleManager.CreateAsync(role);
        //    var role = await _roleManager.FindByNameAsync(regRequest.UserType);
        //    await _userManager.AddToRoleAsync(user, role.Name);

        //    string otpCode = _otpManager.GenerateOtp();
        //    //persist the otp to the otps table
        //    OTP otp = new OTP()
        //    {
        //        Code = otpCode,
        //        PhoneNumber = regRequest.PhoneNumber,
        //        CreatedDate = DateTime.Now,
        //        ExpiresAt = DateTime.Now.AddMinutes(1),
        //    };
        //    await _mainAppContext.OTPs.AddAsync(otp);
        //    await _mainAppContext.SaveChangesAsync();

        //    //send the otp to the specified phone number
        //    await _otpManager.SendOTPAsync(otp.Code, otp.PhoneNumber);

        //    // Get created User (if it is a freelancer)
        //    if (regRequest.UserType == Constants.USER_TYPE_FREELANCER)
        //    {
        //        var createdUser = await _mainAppContext.Users.OfType<Freelancer>()
        //                .Where(u => u.UserName == regRequest.Username)
        //                .Select(u => new FreelancerProfileDTO()
        //                {
        //                    Id = u.Id,
        //                    Name = u.Name,
        //                    //Username = u.UserName,
        //                    PhoneNumber = u.PhoneNumber,
        //                    Skills = u.Skills,
        //                    //UserType = Constants.USER_TYPE_FREELANCER,
        //                    //Role = new RoleResponseDTO { Id = role.Id, Name = role.Name }
        //                })
        //                .FirstOrDefaultAsync();
        //        return Ok(CreateSuccessResponse(createdUser));
        //    }
        //    // Get created User (if it is a client)
        //    else if (regRequest.UserType == Constants.USER_TYPE_CLIENT)
        //    {
        //        var createdUser = await _mainAppContext.Users.OfType<Client>()
        //                  .Where(c => c.UserName == regRequest.Username)
        //                  .Select(c => new ClientProfileDTO
        //                  {
        //                      Id = c.Id,
        //                      Name = c.Name,
        //                      //Username = c.UserName,
        //                      PhoneNumber = c.PhoneNumber,
        //                      CompanyName = c.CompanyName,
        //                      //UserType = Constants.USER_TYPE_CLIENT,
        //                      //Role = new RoleResponseDTO { Id = role.Id, Name = role.Name }
        //                  })
        //                  .FirstOrDefaultAsync();
        //        return Ok(CreateSuccessResponse(createdUser));
        //    }
        //    //this fallback return value will not be returned due to model validation.
        //    return Ok();
        //}




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
