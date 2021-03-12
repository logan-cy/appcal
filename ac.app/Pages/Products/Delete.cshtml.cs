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
    public class DeleteModel : PageModel
    {
        [BindProperty]
        public ProductViewmodel Product { get; set; }

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
                    _ = int.TryParse(id.ToString(), out int productId);
                    Product = await GetProductAsync(productId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Products DeleteModel] OnGet failed");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                await DeleteProductAsync(Product.Id);

                return Redirect("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Products DeleteModel] Failed to delete company");
                return Page();
            }
        }

        private async Task<ProductViewmodel> GetProductAsync(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{config["Sys:ApiUrl"]}/products/single?id={id}");

            var client = clientFactory.CreateClient();
            // LYTODO add authorization bearer token here for login.
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<ProductViewmodel>(responseStream, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var product = result;

                return product;
            }
            else
            {
                return null;
            }
        }

        private async Task DeleteProductAsync(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{config["Sys:ApiUrl"]}/products/delete?id={id}");

            var client = clientFactory.CreateClient();
            // LYTODO add authorization bearer token here for login.
            await client.SendAsync(request);
        }
    }
}
