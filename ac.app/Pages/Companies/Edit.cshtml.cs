using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ac.api.Viewmodels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ac.app.Pages.Companies
{
    public class EditModel : PageModel
    {
        [BindProperty]
        public CompanyViewmodel Company { get; private set; }

        private readonly ILogger<EditModel> _logger;

        private readonly IHttpClientFactory clientFactory;
        private readonly IConfiguration config;

        public EditModel(ILogger<EditModel> logger, IConfiguration config, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            this.config = config;
            this.clientFactory = clientFactory;
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

        private async Task<CompanyViewmodel> GetCompanyAsync(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{config["Sys:ApiUrl"]}/companies/single?id={id}");

            var client = clientFactory.CreateClient();
            // LYTODO add authorization bearer token here for login.
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
