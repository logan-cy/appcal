using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ac.api.Constants;
using ac.api.Data;
using ac.api.Helpers;
using ac.api.Models;
using ac.api.Viewmodels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ac.app.Pages.Companies
{
    public class ConfirmModel : PageModel
    {

        private readonly ILogger<ConfirmModel> _logger;
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        [BindProperty]
        public bool IsSuccess { get; set; }

        public ConfirmModel(ILogger<ConfirmModel> logger, ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            this.context = context;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public async Task OnGet(string token)
        {
            try
            {
                var securityToken = TokenHelper.ReadToken(token);
                // Get the base64 string of the user data that was submitted on registration.
                var data64 = securityToken.Claims.First(claim => claim.Type.ToLower().Contains("userdata")).Value;
                // Get the bytes of the base64 string.
                var dataBytes = Convert.FromBase64String(data64);
                //Get the json of the byte array.
                var json = Encoding.UTF8.GetString(dataBytes);
                var model = JsonSerializer.Deserialize<RegisterCompanyViewmodel>(json);

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
                    IsSuccess = false;
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
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to confirm company registration: {ex}", ex);
            }
        }
    }
}
