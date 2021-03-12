using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ac.api.Viewmodels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ac.app.Pages.Companies
{
    public class EditModel : PageModel
    {
        [BindProperty]
        public CompanyViewmodel Company { get; set; }

        private readonly ILogger<EditModel> _logger;

        private readonly IHttpClientFactory clientFactory;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration config;

        public EditModel(ILogger<EditModel> logger, IConfiguration config, IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            this.config = config;
            this.clientFactory = clientFactory;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task OnGetAsync(int? id)
        {
            try
            {
                if (id != null)
                {
                    _ = int.TryParse(id.ToString(), out int companyId);
                    Company = await GetCompanyAsync(companyId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Companies IndexModel] OnGet failed");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"{config["Sys:ApiUrl"]}/companies/edit?id={Company.Id}");

                var body = JsonSerializer.Serialize(Company);
                var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
                request.Content = content;

                var tokenBytes = httpContextAccessor.HttpContext.Session.Get("Token");
                var token = System.Text.Encoding.Default.GetString(tokenBytes);
                var client = clientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return Redirect("Index");
                }
                else
                {
                    return Page();
                }
            }
            catch (Exception)
            {
                return Page();
            }
        }

        private async Task<CompanyViewmodel> GetCompanyAsync(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{config["Sys:ApiUrl"]}/companies/single?id={id}");

            var tokenBytes = httpContextAccessor.HttpContext.Session.Get("Token");
            var token = System.Text.Encoding.Default.GetString(tokenBytes);
            var client = clientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<CompanyViewmodel>(responseStream, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var company = result;

                return company;
            }
            else
            {
                return null;
            }
        }
    }
}
