using System.Text;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ac.api.Data;
using ac.api.Helpers;
using ac.api.Models;
using ac.api.Viewmodels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using ac.api.Constants;
using ac.api.Interfaces;
using ac.api.Models.DTO;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace ac.api.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly IConfiguration config;
        private readonly IWebHostEnvironment environment;
        private readonly IEmailService emailService;
        private readonly IOptions<EmailOptionsDTO> emailOptions;
        private readonly IUserService userService;

        public AuthController(ILogger<AuthController> logger,
            RoleManager<IdentityRole> roleManager,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ApplicationDbContext context,
            IConfiguration config,
            IWebHostEnvironment environment,
            IEmailService emailService,
            IOptions<EmailOptionsDTO> emailOptions,
            IUserService userService)
        {
            this.emailOptions = emailOptions;
            this.userService = userService;
            this.emailService = emailService;
            this.environment = environment;
            this.config = config;
            this.userManager = userManager;
            this.signInManager = signInManager;
            _logger = logger;
            this.roleManager = roleManager;
            this.context = context;
        }

        /// <summary>
        /// Log in with the given user credentials.
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewmodel model)
        {
            try
            {
                var key = config["Sys:Key"];
                var issuer = config["Sys:Issuer"];
                var audience = config["Sys:Audience"];
                var keyBytes = Encoding.ASCII.GetBytes(key);

                var user = await userManager.FindByNameAsync(model.Username);
                // Try to get a user by email if there's no user returned by username.
                if (user == null)
                {
                    user = await userManager.FindByEmailAsync(model.Username);
                }

                // user service can only send the successful result or throw an error - no need to null check it.
                var resultModel = await userService.Authenticate(model.Username, model.Password);
                switch (resultModel.Role)
                {
                    // Admin role doesn't take any special considerations so exclude it.
                    case SystemRoles.Company:
                        var companyUser = await context.CompanyUsers.Include(x => x.Company).Include(x => x.User).SingleAsync(x => x.User.Id == resultModel.UserId);
                        resultModel.UserTypeId = companyUser.Company.Id.ToString();
                        break;
                    case SystemRoles.Client:
                        var clientUser = await context.ClientUsers.Include(x => x.Client).Include(x => x.User).SingleAsync(x => x.User.Id == resultModel.UserId);
                        resultModel.UserTypeId = clientUser.Client.Id.ToString();
                        break;
                }

                // Serialize the model to json so that it can be added to a claim.
                var modelJson = JsonSerializer.Serialize(resultModel);

                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, user.Id.ToString()),
                        new Claim(ClaimTypes.UserData, modelJson)
                    }),
                    Expires = DateTime.Now.AddDays(7),
                    Issuer = issuer,
                    IssuedAt = DateTime.Now,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha512Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);

                return Ok(tokenHandler.WriteToken(token));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to log in", ex);
                return BadRequest(ex.ToString());
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> CreateUser(RegisterUserViewmodel model)
        {
            try
            {
                var user = new ApplicationUser
                {
                    Email = model.Email,
                    UserName = model.Email,
                    PasswordHash = model.Password,
                    IsCompany = model.IsCompany
                };
                var user64 = TokenHelper.GenerateTokenFromObject(user);

                var key = config["Sys:Key"];
                var issuer = config["Sys:Issuer"];
                var audience = config["Sys:Audience"];

                var token = TokenHelper.JwtTokenGenerator(user64, key, issuer, audience);

                await SendRegisterUserEmail(user, model.Name, token);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError("Unable to register user", ex);
                return BadRequest(ex.ToString());
            }
        }

        

        [HttpPost("activate")]
        public async Task<IActionResult> Activate([FromBody] PasswordResetViewmodel model)
        {
            try
            {
                model.UserId = Uri.UnescapeDataString(model.UserId);
                model.Code = Uri.UnescapeDataString(model.Code);

                var currentUser = await context.Users.FirstOrDefaultAsync(x => x.Id == model.UserId);
                if (currentUser == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                var result = await userManager.ConfirmEmailAsync(currentUser, model.Code);
                if (!result.Succeeded)
                {
                    return BadRequest(new { message = result.Errors.FirstOrDefault().Description });
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError("Unable to activate your account.", ex);
                return BadRequest(ex.ToString());
            }
        }

        [HttpPost("confirm")]
        public async Task<IActionResult> Confirm(string userToken)
        {
            try
            {

                // Get the user from the token.
                var user = TokenHelper.ResolveObjectFromToken<ApplicationUser>(userToken);

                // Register the user account.
                var result = await userManager.CreateAsync(user, user.PasswordHash);

                if (!result.Succeeded)
                {
                    return BadRequest(result);
                }

                // User registration succeeded, assign role.
                if (user.IsCompany)
                {
                    // Get the role name as a string ONCE to eliminate repetition.
                    var companyRoleName = nameof(SystemRoles.Company);

                    if (!await roleManager.RoleExistsAsync(companyRoleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(companyRoleName));
                    }
                    await userManager.AddToRoleAsync(user, companyRoleName);
                }
                else
                {
                    // Get the role name as a string ONCE to eliminate repetition.
                    var clientRoleName = nameof(SystemRoles.Client);

                    if (!await roleManager.RoleExistsAsync(clientRoleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(clientRoleName));
                    }
                    await userManager.AddToRoleAsync(user, clientRoleName);
                }

                // Finally, mark the user account as EmailConfirmed = true.
                var u = await context.Users.FindAsync(user.Id);
                u.EmailConfirmed = true;

                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError("Unable to confirm user registration", ex);
                return BadRequest(ex.ToString());
            }
        }



        private async Task SendRegisterUserEmail(IdentityUser user, string name, string userToken)
        {
            var filePath = Path.Combine(environment.ContentRootPath, EmailTemplateConstants.USER_REGISTRATION_PATH);
            using var reader = new StreamReader(filePath);
            var mailText = await reader.ReadToEndAsync();

            var appBase = config["Sys:AppBase"];

            _ = mailText.Replace("{name}", name);
            _ = mailText.Replace("{username}", $"{user.Email}");

            var link = $"{appBase}confirm/{Uri.EscapeDataString(userToken)}";
            mailText = mailText.Replace("{link}", link);

            await emailService.SendAsync(user.Email, mailText, "Please confirm your registration - HealthImpact Appointments Portal", emailOptions.Value);
        }

    }
}