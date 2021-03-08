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

        public AuthController(ILogger<AuthController> logger, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ApplicationDbContext context, IConfiguration config)
        {
            this.config = config;
            this.userManager = userManager;
            this.signInManager = signInManager;
            _logger = logger;
            this.context = context;
        }

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
    }
}