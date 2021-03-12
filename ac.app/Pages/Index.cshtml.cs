using System.Reflection.PortableExecutable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ac.api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using ac.api.Viewmodels;
using System.Text.Json;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ac.app.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration config;
        private readonly IHttpClientFactory clientFactory;
        private readonly IHttpContextAccessor httpContextAccessor;

        [BindProperty]
        public int CompanyId { get; set; }
        public IEnumerable<SelectListItem> Companies { get; set; }

        public string Token { get; private set; }
        public string Username { get; private set; }

        public IndexModel(ILogger<IndexModel> logger, IConfiguration config, IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            this.config = config;
            this.clientFactory = clientFactory;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> OnGet()
        {
            var token = httpContextAccessor.HttpContext.Session.Get("Token");
            if (token == null)
            {
                return Redirect("~/Account/Login");
            }

            Companies = await GetCompaniesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostGetEventsAsync(int id)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{config["Sys:ApiUrl"]}/events?companyId={id}");

                var tokenBytes = httpContextAccessor.HttpContext.Session.Get("Token");
                var token = System.Text.Encoding.Default.GetString(tokenBytes);
                var client = clientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    using var responseStream = await response.Content.ReadAsStreamAsync();
                    var result = await JsonSerializer.DeserializeAsync<List<EventViewmodel>>(responseStream, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                    var evs = result;

                    return new JsonResult(evs);
                }

                return new JsonResult(string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to get appointments: {ex}", ex);
                var evs = Array.Empty<EventViewmodel>();

                return new JsonResult(evs);

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
                var companies = Array.Empty<SelectListItem>();

                return companies;
            }
        }
    }
}
