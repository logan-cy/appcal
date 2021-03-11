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
    public class IndexModel : PageModel
    {
        [BindProperty]
        public IEnumerable<DivisionViewmodel> Divisions { get; set; }

        private readonly ILogger<IndexModel> _logger;

        private readonly IHttpClientFactory clientFactory;
        private readonly IConfiguration config;

        public IndexModel(ILogger<IndexModel> logger, IConfiguration config, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            this.config = config;
            this.clientFactory = clientFactory;
        }

        public async Task OnGetAsync()
        {
            try
            {
                Divisions = await GetDivisionsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Divisions IndexModel] OnGet failed");
            }
        }

        private async Task<IEnumerable<DivisionViewmodel>> GetDivisionsAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{config["Sys:ApiUrl"]}/divisions");

            var client = clientFactory.CreateClient();
            // LYTODO add authorization bearer token here for login.
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<List<DivisionViewmodel>>(responseStream, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var divisions = result.ToList();

                return divisions;
            }
            else
            {
                var divisions = Array.Empty<DivisionViewmodel>();
                return divisions;
            }
        }
    }
}
