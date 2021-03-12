using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ac.api.Viewmodels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ac.app.Pages.ProductSets
{
    public class EditModel : PageModel
    {
        [BindProperty]
        public ProductSetViewmodel ProductSet { get; set; }

        [BindProperty]
        public List<int> SelectedProducts { get; set; }

        public IEnumerable<SelectListItem> Companies { get; set; }
        public bool GetCompaniesError { get; private set; }

        public IEnumerable<SelectListItem> Divisions { get; set; }
        public bool GetDivisionsError { get; private set; }

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
                    _ = int.TryParse(id.ToString(), out int productSetId);
                    Companies = await GetCompaniesAsync();
                    Divisions = await GetDivisionsAsync();
                    ProductSet = await GetProductSetAsync(productSetId);

                    SelectedProducts = new List<int>();
                    foreach (var product in ProductSet.Products)
                    {
                        SelectedProducts.Add(product.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Product Set EditModel] OnGet failed");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"{config["Sys:ApiUrl"]}/productsets/edit?id={ProductSet.Id}");

                if (ProductSet.Products == null)
                {
                    ProductSet.Products = new List<ProductViewmodel>();
                }
                foreach (var productId in SelectedProducts)
                {
                    ProductSet.Products.Add(new ProductViewmodel
                    {
                        Id = productId
                    });
                }

                var body = JsonSerializer.Serialize(ProductSet);
                var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
                request.Content = content;

                var client = clientFactory.CreateClient();
                // LYTODO add authorization bearer token here for login.
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
            catch (Exception ex)
            {
                _logger.LogError($"Unable to save product: {ex}", ex);
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

        #region Helpers
        private async Task<IEnumerable<SelectListItem>> GetCompaniesAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{config["Sys:ApiUrl"]}/companies");

            var client = clientFactory.CreateClient();
            // LYTODO add authorization bearer token here for login.
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<List<CompanyViewmodel>>(responseStream, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var companies = result.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }).ToList();

                return companies;
            }
            else
            {
                GetCompaniesError = true;
                var companies = Array.Empty<SelectListItem>();

                return companies;
            }
        }

        private async Task<IEnumerable<SelectListItem>> GetDivisionsAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{config["Sys:ApiUrl"]}/divisions");

            var client = clientFactory.CreateClient();
            // LYTODO add authorization bearer token here for login.
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<List<CompanyViewmodel>>(responseStream, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var divisions = result.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }).ToList();

                return divisions;
            }
            else
            {
                GetDivisionsError = true;
                var divisions = Array.Empty<SelectListItem>();

                return divisions;
            }
        }
        #endregion
    }
}
