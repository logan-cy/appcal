using System;
using System.Collections.Generic;
using System.IO;
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

namespace ac.app.Pages.Products
{
    public class CreateModel : PageModel
    {
        [BindProperty]
        public ProductViewmodel Product { get; set; }

        public bool SaveSetError { get; private set; }
        public string SaveSetErrorMessage { get; private set; }

        public IEnumerable<SelectListItem> Companies { get; set; }
        public bool GetCompaniesError { get; private set; }

        private readonly ILogger<CreateModel> _logger;

        private readonly IHttpClientFactory clientFactory;
        private readonly IConfiguration config;

        public CreateModel(ILogger<CreateModel> logger, IConfiguration config, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            this.config = config;
            this.clientFactory = clientFactory;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                Companies = await GetCompaniesAsync();

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to load page: {ex}", ex);
                return BadRequest();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"{config["Sys:ApiUrl"]}/products/create");


                var body = JsonSerializer.Serialize(Product);
                var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
                request.Content = content;

                var client = clientFactory.CreateClient();
                // LYTODO add authorization bearer token here for login.
                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    using var responseStream = await response.Content.ReadAsStreamAsync();
                    using var reader = new StreamReader(responseStream);
                    SaveSetErrorMessage = await reader.ReadToEndAsync();
                    SaveSetError = true;
                }
                return Redirect("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to save appointment: {ex}", ex);
                SaveSetErrorMessage = ex.ToString();
                SaveSetError = true;

                return Page();
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
        #endregion
    }
}
