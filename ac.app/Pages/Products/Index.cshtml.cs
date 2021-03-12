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

namespace ac.app.Pages.Products
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public IEnumerable<ProductViewmodel> Products { get; set; }

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
                Products = await GetProductsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Products IndexModel] OnGet failed");
            }
        }

        private async Task<IEnumerable<ProductViewmodel>> GetProductsAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{config["Sys:ApiUrl"]}/products");

            var client = clientFactory.CreateClient();
            // LYTODO add authorization bearer token here for login.
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<List<ProductViewmodel>>(responseStream, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var products = result.ToList();

                return products;
            }
            else
            {
                var products = Array.Empty<ProductViewmodel>();
                return products;
            }
        }
    }
}
