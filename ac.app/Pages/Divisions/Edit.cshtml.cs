using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ac.api.Viewmodels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ac.app.Pages.Divisions
{
    public class EditModel : PageModel
    {
        [BindProperty]
        public DivisionViewmodel Division { get; set; }
        public bool SaveDivisionError { get; private set; }
        public string SaveDivisionErrorMessage { get; private set; }

        public IEnumerable<SelectListItem> Companies { get; set; }
        public bool GetCompaniesError { get; private set; }

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
                    Companies = await GetCompaniesAsync();
                    Division = await GetDivisionAsync(id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to load page: {ex}", ex);
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"{config["Sys:ApiUrl"]}/divisions/edit?id={Division.Id}");

                var body = JsonSerializer.Serialize(Division);
                var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
                request.Content = content;

                var tokenBytes = httpContextAccessor.HttpContext.Session.Get("Token");
                var token = System.Text.Encoding.Default.GetString(tokenBytes);
                var client = clientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    using var responseStream = await response.Content.ReadAsStreamAsync();
                    using var reader = new StreamReader(responseStream);
                    SaveDivisionErrorMessage = await reader.ReadToEndAsync();
                    SaveDivisionError = true;
                }
                return Redirect("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to save division: {ex}", ex);
                SaveDivisionErrorMessage = ex.ToString();
                SaveDivisionError = true;

                return Page();
            }
        }

        #region Helpers
        private async Task<DivisionViewmodel> GetDivisionAsync(int? id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{config["Sys:ApiUrl"]}/divisions/single?id={id}");

            var tokenBytes = httpContextAccessor.HttpContext.Session.Get("Token");
            var token = System.Text.Encoding.Default.GetString(tokenBytes);
            var client = clientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<DivisionViewmodel>(responseStream, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                return result;
            }
            else
            {
                SaveDivisionError = true;
                SaveDivisionErrorMessage = $"Unable to get division {response.ReasonPhrase}";
                return null;
            }
        }

        private async Task<IEnumerable<SelectListItem>> GetCompaniesAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{config["Sys:ApiUrl"]}/companies");

            var tokenBytes = httpContextAccessor.HttpContext.Session.Get("Token");
            var token = System.Text.Encoding.Default.GetString(tokenBytes);
            var client = clientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

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
