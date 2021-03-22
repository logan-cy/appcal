using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ac.api.Constants;
using ac.api.Data;
using ac.api.Helpers;
using ac.api.Interfaces;
using ac.api.Models;
using ac.api.Models.DTO;
using ac.api.Viewmodels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ac.api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class CompaniesController : ControllerBase
    {
        private readonly ILogger<CompaniesController> _logger;
        private readonly IConfiguration config;
        private readonly IWebHostEnvironment environment;
        private readonly ApplicationDbContext context;
        private readonly IEmailService emailService;
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IOptions<EmailOptionsDTO> emailOptions;

        public CompaniesController(ILogger<CompaniesController> logger, IConfiguration config,
            IWebHostEnvironment environment, ApplicationDbContext context,
            IEmailService emailService,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<EmailOptionsDTO> emailOptions)
        {
            this.context = context;
            this.emailService = emailService;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
            this.emailOptions = emailOptions;
            _logger = logger;
            this.config = config;
            this.environment = environment;
        }

        /// <summary>
        /// Get a list of all companies currently stored.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var companies = await context.Companies.Select(x => new CompanyViewmodel
                {
                    Address = x.Address,
                    Id = x.Id,
                    Name = x.Name
                }).ToListAsync();

                return Ok(companies);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to get companies", ex);
                return BadRequest(ex.ToString());
            }
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search(string query)
        {
            try
            {
                var companies = context.Companies.AsQueryable();

                if (companies.Any(x => x.Address.Contains(query)))
                {
                    companies = companies.Where(x => x.Address.Contains(query));
                }
                if (companies.Any(x => x.Name.Contains(query)))
                {
                    companies = companies.Where(x => x.Name.Contains(query));
                }

                var model = await companies.Select(x => new CompanyViewmodel
                {
                    Address = x.Address,
                    Id = x.Id,
                    Name = x.Name
                }).ToListAsync();

                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to search for companies", ex);
                return BadRequest(ex.ToString());
            }
        }

        /// <summary>
        /// Get a single company as indicated by the specified Company ID parameter value.
        /// </summary>
        /// <param name="id" type="int">The ID value of the company to be retrieved.</param>
        [HttpGet("single")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var company = await context.Companies.FindAsync(id);
                if (company == null)
                {
                    return NotFound(new { message = $"Company with ID {id} was not found." });
                }
                var model = new CompanyViewmodel
                {
                    Address = company.Address,
                    Id = company.Id,
                    Name = company.Name
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to get company", ex);
                return BadRequest(ex.ToString());
            }
        }

        /// <summary>
        /// Creates a new company with the given details.
        /// </summary>
        /// <param name="model" type="CompanyViewmodel">The model containing the new company details.</param>
        [HttpPost("create")]
        public async Task<IActionResult> Create(CompanyViewmodel model)
        {
            try
            {
                var company = new Company
                {
                    Address = model.Address,
                    Name = model.Name
                };
                await context.Companies.AddAsync(company);
                await context.SaveChangesAsync();
                return Ok(company);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to create company", ex);
                return BadRequest(ex.ToString());
            }
        }

        /// <summary>
        /// Updates the relevant company as indicated by the id parameter.
        /// </summary>
        /// <param name="model" type="CompanyViewmodel">The model containing the new company details.</param>
        /// <param name="id" type="int">The ID value of the company to be edited.</param>
        [HttpPost("edit")]
        public async Task<IActionResult> Edit([FromBody] CompanyViewmodel model, int id)
        {
            try
            {
                var company = await context.Companies.FindAsync(id);
                if (company == null)
                {
                    return NotFound(new { message = $"Company with ID {id} was not found." });
                }
                company.Address = model.Address;
                company.Name = model.Name;

                await context.SaveChangesAsync();

                return Ok(company);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to update division", ex);
                return BadRequest(ex.ToString());
            }
        }

        /// <summary>
        /// Deletes the selected company as well as all of the divisions and products that belong to it.
        /// NOTE: This action is irreversible
        /// </summary>
        /// <param name="id" type="int">The ID value of the company to be deleted.</param>
        [HttpPost("delete")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var company = await context.Companies.FindAsync(id);
                if (company == null)
                {
                    return NotFound(new { message = $"Company with ID {id} was not found." });
                }

                // Avoid possible data corruption by deleting products and divisions before deleting the company.
                var divisions = await context.Divisions.Where(x => x.Company.Id == id).ToListAsync();
                foreach (var division in divisions)
                {
                    var products = context.Products.Where(x => x.Division.Id == division.Id);
                    context.Products.RemoveRange(products);
                }
                context.Divisions.RemoveRange(divisions);

                context.Companies.Remove(company);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to delete company", ex);
                return BadRequest(ex.ToString());
            }
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> RegisterCompany([FromBody]RegisterCompanyViewmodel model)
        {
            try
            {
                // As opposed to generating a signed JWT token, just parse
                // the model to a base64 string and inject it into the token.

                // Convert model to json.
                var modelJson = JsonSerializer.Serialize(model);
                // Get bytes of json string.
                var bytes = Encoding.UTF8.GetBytes(modelJson);
                // Get base64 of bytes.
                var model64 = Convert.ToBase64String(bytes);

                var key = config["Sys:Key"];
                var issuer = config["Sys:Issuer"];
                var audience = config["Sys:Audience"];

                var token = TokenHelper.JwtTokenGenerator(model64, key, issuer, audience);

                await SendRegisterCompanyEmail(model, model.User.Name, token);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError("Unable to register company user", ex);
                return BadRequest(ex.ToString());
            }
        }

        [AllowAnonymous]
        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmCompany([FromBody]RegisterCompanyViewmodel model)
        {
            try
            {
                // Create the company.
                var company = new Company
                {
                    Address = model.Company.Address,
                    Name = model.Company.Name
                };
                await context.Companies.AddAsync(company);
                await context.SaveChangesAsync();

                // Register the user account.
                var user = new IdentityUser
                {
                    Email = model.User.Email,
                    UserName = model.User.Email,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(user, model.User.Password);
                if (!result.Succeeded)
                {
                    return BadRequest(result);
                }

                // Assign user to role.
                var companyRoleName = nameof(SystemRoles.Company);
                if (!await roleManager.RoleExistsAsync(companyRoleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(companyRoleName));
                }
                await userManager.AddToRoleAsync(user, companyRoleName);

                // Create a link associating the the user with the company.
                await context.CompanyUsers.AddAsync(new CompanyUsers
                {
                    Company = company,
                    User = user
                });
                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to register new company", ex);
                return BadRequest(ex.ToString());
            }
        }


        private async Task SendRegisterCompanyEmail(RegisterCompanyViewmodel model, string name, string token)
        {
            var filePath = Path.Combine(environment.ContentRootPath, EmailTemplateConstants.USER_REGISTRATION_PATH);
            using var reader = new StreamReader(filePath);
            var mailText = await reader.ReadToEndAsync();

            var appBase = config["Sys:AppBase"];

            _ = mailText.Replace("{name}", name);
            _ = mailText.Replace("{username}", $"{model.User.Email}");

            // Link is the page on the web app, not the api endpoint.
            var link = $"{appBase}companies/confirm/?token={Uri.EscapeDataString(token)}";
            mailText = mailText.Replace("{link}", link);

            await emailService.SendAsync(model.User.Email, mailText, "Please confirm your registration - HealthImpact Appointments Portal", emailOptions.Value);
        }
    }
}
