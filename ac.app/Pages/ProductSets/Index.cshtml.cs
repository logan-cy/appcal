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

namespace ac.app.Pages.ProductSets
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public IEnumerable<ProductSetViewmodel> ProductSets { get; set; }

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
                ProductSets = await GetProductSetsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Product Sets IndexModel] OnGet failed");
            }
        }

        private async Task<IEnumerable<ProductSetViewmodel>> GetProductSetsAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{config["Sys:ApiUrl"]}/productsets");

            var client = clientFactory.CreateClient();
            // LYTODO add authorization bearer token here for login.
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<List<ProductSetViewmodel>>(responseStream, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var sets = result.ToList();

                return sets;
            }
            else
            {
                var sets = Array.Empty<ProductSetViewmodel>();
                return sets;
            }
        }
    }
}
