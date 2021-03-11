using System.Reflection.PortableExecutable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ac.api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;
using ac.api.Data;
using Microsoft.EntityFrameworkCore;
using ac.api.Viewmodels;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.IO;

namespace ac.app.Pages
{
    public class CreateModel : PageModel
    {
        private readonly ILogger<CreateModel> _logger;

        public IEnumerable<SelectListItem> Companies { get; private set; }
        public bool GetCompaniesError { get; private set; }

        [BindProperty]
        public EventViewmodel Appointment { get; set; }

        public bool SaveAppointmentError { get; private set; }
        public string SaveAppointmentErrorMessage { get; private set; }

        private readonly IHttpClientFactory clientFactory;
        private readonly IConfiguration config;

        public CreateModel(ILogger<CreateModel> logger, IConfiguration config, IHttpClientFactory clientFactory)
        {
            this.config = config;
            this.clientFactory = clientFactory;
            _logger = logger;
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
                var request = new HttpRequestMessage(HttpMethod.Post, $"{config["Sys:ApiUrl"]}/events/create");

                Appointment.AllDay = Appointment.End == DateTime.MinValue;
                var body = JsonSerializer.Serialize(Appointment);
                var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
                request.Content = content;

                var client = clientFactory.CreateClient();
                // LYTODO add authorization bearer token here for login.
                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    using var responseStream = await response.Content.ReadAsStreamAsync();
                    using var reader = new StreamReader(responseStream);
                    SaveAppointmentErrorMessage = await reader.ReadToEndAsync();
                    SaveAppointmentError = true;
                }
                return Redirect("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to save appointment: {ex}", ex);
                SaveAppointmentErrorMessage = ex.ToString();
                SaveAppointmentError = true;

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
