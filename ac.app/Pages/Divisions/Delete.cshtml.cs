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

namespace ac.app.Pages.Divisions
{
    public class DeleteModel : PageModel
    {
        [BindProperty]
        public DivisionViewmodel Division { get; set; }

        private readonly ILogger<DeleteModel> _logger;

        private readonly IHttpClientFactory clientFactory;
        private readonly IConfiguration config;

        public DeleteModel(ILogger<DeleteModel> logger, IConfiguration config, IHttpClientFactory clientFactory)
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
                    _ = int.TryParse(id.ToString(), out int divisionId);
                    Division = await GetDivisionAsync(divisionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Divisions DeleteModel] OnGet failed");
            }
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            try
            {
                if (id != null)
                {
                    _ = int.TryParse(id.ToString(), out int companyId);
                    await DeleteDivisionAsync(companyId);

                    return Redirect("Index");
                }
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Divisions DeleteModel] Failed to delete company");
                return Page();
            }
        }

        private async Task<DivisionViewmodel> GetDivisionAsync(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{config["Sys:ApiUrl"]}/divisions/single?id={id}");

            var client = clientFactory.CreateClient();
            // LYTODO add authorization bearer token here for login.
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<DivisionViewmodel>(responseStream, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var division = result;

                return division;
            }
            else
            {
                return null;
            }
        }

        private async Task DeleteDivisionAsync(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{config["Sys:ApiUrl"]}/divisions/delete?id={id}");

            var client = clientFactory.CreateClient();
            // LYTODO add authorization bearer token here for login.
            var response = await client.SendAsync(request);
        }
    }
}
