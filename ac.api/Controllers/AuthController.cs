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

namespace ac.api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly IConfiguration config;
        private readonly IWebHostEnvironment environment;
        private readonly IEmailService emailService;
        private readonly IOptions<EmailOptionsDTO> emailOptions;

        public AuthController(ILogger<AuthController> logger, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ApplicationDbContext context, IConfiguration config, IWebHostEnvironment environment, IEmailService emailService, IOptions<EmailOptionsDTO> emailOptions)
        {
            this.emailOptions = emailOptions;
            this.emailService = emailService;
            this.environment = environment;
            this.config = config;
            this.userManager = userManager;
            this.signInManager = signInManager;
            _logger = logger;
            this.context = context;
        }

        /// <summary>
        /// Log in with the given user credentials 
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewmodel model)
        {
            try
            {
                var user = await userManager.FindByNameAsync(model.Username);
                // Try to get a user by email if there's no user returned by username.
                if (user == null)
                {
                    user = await userManager.FindByEmailAsync(model.Username);
                }

                var result = await signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);
                if (!result.Succeeded)
                {
                    return BadRequest(result);
                }
                var key = config["Sys:Key"];
                var token = await TokenHelper.JwtTokenGenerator(user, userManager, key);
                return Ok(new { result = result, token = token });
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
                var user = new IdentityUser
                {
                    Email = model.Email
                };
                var result = await userManager.CreateAsync(user, model.Password);

                if (!result.Succeeded)
                {
                    return BadRequest(result);
                }
                // localhost:5000/Account/Confirm
                var registerEmailUrl = Request.Headers["registerEmailUrl"];

                await SendRegisterEmail(user, model.Name, registerEmailUrl);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError("Unable to register user", ex);
                return BadRequest(ex.ToString());
            }
        }

        [HttpPost("confirm")]
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

        private async Task SendRegisterEmail(IdentityUser user, string name, string registerEmailUrl)
        {
            var filePath = Path.Combine(environment.ContentRootPath, EmailTemplateConstants.USER_REGISTRATION_PATH);
            using var reader = new StreamReader(filePath);
            var mailText = await reader.ReadToEndAsync();

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var appBase = config["Sys:AppBase"];

            mailText.Replace("{name}", name);
            mailText.Replace("{username}", $"{user.Email}");

            var link = $"{appBase}activate/{Uri.EscapeDataString(token)}/{Uri.EscapeDataString(user.Id)}";
            mailText = mailText.Replace("{link}", link);

            await emailService.SendAsync(user.Email, mailText, "Please confirm your registration - HealthImpact Appointments Portal", emailOptions.Value);
        }
    }
}