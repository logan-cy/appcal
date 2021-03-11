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
    public class DeleteModel : PageModel
    {
        [BindProperty]
        public ProductSetViewmodel ProductSet { get; set; }

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
                    _ = int.TryParse(id.ToString(), out int productSetId);
                    ProductSet = await GetProductSetAsync(productSetId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Companies DeleteModel] OnGet failed");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                await DeleteProductSetAsync(ProductSet.Id);

                return Redirect("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Product Set DeleteModel] Failed to delete product set");
                return Page();
            }
        }

        private async Task<ProductSetViewmodel> GetProductSetAsync(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{config["Sys:ApiUrl"]}/productsets/single?id={id}");

            var client = clientFactory.CreateClient();
            // LYTODO add authorization bearer token here for login.
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<ProductSetViewmodel>(responseStream, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var set = result;

                return set;
            }
            else
            {
                return null;
            }
        }

        private async Task DeleteProductSetAsync(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{config["Sys:ApiUrl"]}/productsets/delete?id={id}");

            var client = clientFactory.CreateClient();
            // LYTODO add authorization bearer token here for login.
            var response = await client.SendAsync(request);
        }
    }
}
