using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using ac.api.Viewmodels;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace ac.app.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly ILogger<LoginModel> _logger;

        private readonly IHttpClientFactory clientFactory;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration config;

        public LoginModel(ILogger<LoginModel> logger, IConfiguration config, IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            this.config = config;
            this.clientFactory = clientFactory;
            this.httpContextAccessor = httpContextAccessor;
        }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        [BindProperty]
        public LoginViewmodel Input { get; set; }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl = returnUrl ?? Url.Content("~/");

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            try
            {
                returnUrl ??= "/";
                if (ModelState.IsValid)
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, $"{config["Sys:ApiUrl"]}/auth/login");

                    var body = JsonSerializer.Serialize(Input);
                    var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
                    request.Content = content;

                    var client = clientFactory.CreateClient();
                    var response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        // User login succeeded, save the auth token from the API
                        // response in a time-limited cookie.
                        using var responseStream = await response.Content.ReadAsStreamAsync();
                        var result = await JsonSerializer.DeserializeAsync<LoginResultViewmodel>(responseStream, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                        var options = new CookieOptions
                        {
                            Expires = DateTime.Now.AddDays(7)
                        };
                        httpContextAccessor.HttpContext.Session.SetString("Token", result.Token);
                        httpContextAccessor.HttpContext.Session.SetString("Username", result.Username);
                        httpContextAccessor.HttpContext.Response.Cookies.Append("Token", result.Token, options);
                        httpContextAccessor.HttpContext.Response.Cookies.Append("Username", Input.Username, options);

                        _logger.LogInformation("User logged in.");
                        return LocalRedirect(returnUrl);
                    }

                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }

                // Model isn't valid.
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login failed {ex}", ex);
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }
        }
    }
}
