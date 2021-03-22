using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ac.api.Constants;
using ac.api.Data;
using ac.api.Helpers;
using ac.api.Interfaces;
using ac.api.Models.DTO;
using ac.api.Viewmodels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ac.app.Pages.Companies
{
    public class RegisterModel : PageModel
    {
        private readonly ILogger<RegisterModel> logger;
        private readonly IEmailService emailService;
        private readonly IOptions<EmailOptionsDTO> emailOptions;
        private readonly IWebHostEnvironment environment;
        private readonly IConfiguration config;

        public bool IsRegistered { get; set; }

        [BindProperty]
        public CompanyViewmodel CompanyInput { get; set; }
        [BindProperty]
        public InputModel UserInput { get; set; }

        public RegisterModel(ILogger<RegisterModel> logger, IConfiguration config, IEmailService emailService, IOptions<EmailOptionsDTO> emailOptions, IWebHostEnvironment environment)
        {
            this.config = config;
            this.logger = logger;
            this.emailService = emailService;
            this.emailOptions = emailOptions;
            this.environment = environment;
        }

        public void OnGet()
        {
        }

        public async Task OnPost()
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (UserInput.ConfirmPassword == UserInput.Password)
                    {
                        // As opposed to generating a signed JWT token, just parse
                        // the model to a base64 string and inject it into the token.
                        var model = new RegisterCompanyViewmodel
                        {
                            Company = CompanyInput,
                            User = new RegisterUserViewmodel
                            {
                                Email = UserInput.Email,
                                IsCompany = true,
                                Name = UserInput.Name,
                                Password = UserInput.Password
                            }
                        };

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

                        await SendRegisterCompanyEmail(UserInput.Email, UserInput.Name, token);
                    }
                    else
                    {
                        ModelState.AddModelError(nameof(UserInput.ConfirmPassword), "Passwords don't match");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Please ensure that you've filled in all the required fields.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Unable to register new company: {ex}", ex);
            }
        }

        private async Task SendRegisterCompanyEmail(string email, string name, string token)
        {
            var filePath = Path.Combine(environment.ContentRootPath, EmailTemplateConstants.USER_REGISTRATION_PATH);
            using var reader = new StreamReader(filePath);
            var mailText = await reader.ReadToEndAsync();

            var appBase = config["Sys:AppBase"];

            _ = mailText.Replace("{name}", name);
            _ = mailText.Replace("{username}", $"{email}");

            // Link is the page on the web app, not the api endpoint.
            var link = $"{appBase}companies/confirm/?token={Uri.EscapeDataString(token)}";
            mailText = mailText.Replace("{link}", link);

            await emailService.SendAsync(email, mailText, "Please confirm your registration - HealthImpact Appointments Portal", emailOptions.Value);
        }

        public class InputModel
        {
            [Required]
            [StringLength(100, ErrorMessage = "The {0} field is required.")]
            [Display(Name = "Name")]
            public string Name { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }
    }
}
