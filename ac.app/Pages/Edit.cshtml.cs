using System.Reflection.PortableExecutable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ac.api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ac.api.Data;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using ac.api.Viewmodels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using System.IO;

namespace ac.app.Pages
{
    public class EditModel : PageModel
    {
        private readonly ILogger<EditModel> _logger;

        public IEnumerable<SelectListItem> Companies { get; set; }
        public bool GetCompaniesError { get; private set; }

        [BindProperty]
        public EventViewmodel Appointment { get; set; }

        public bool AppointmentError { get; private set; }
        public string AppointmentErrorMessage { get; private set; }

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
                if (id == null)
                {
                    AppointmentError = true;
                    AppointmentErrorMessage = "Appointment wasn't found.";
                }
                Companies = await GetCompaniesAsync();
                Appointment = await GetAppointmentAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[EditModel] OnGet failed");
                AppointmentError = true;
                AppointmentErrorMessage = ex.ToString();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"{config["Sys:ApiUrl"]}/events/edit?id={Appointment.Id}");

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
                    AppointmentErrorMessage = await reader.ReadToEndAsync();
                    AppointmentError = true;
                }
                return Redirect("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to save appointment: {ex}", ex);
                AppointmentErrorMessage = ex.ToString();
                AppointmentError = true;

                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"{config["Sys:ApiUrl"]}/events/delete?id={id}");

                var client = clientFactory.CreateClient();
                // LYTODO add authorization bearer token here for login.
                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    using var responseStream = await response.Content.ReadAsStreamAsync();
                    using var reader = new StreamReader(responseStream);
                    AppointmentErrorMessage = await reader.ReadToEndAsync();
                    AppointmentError = true;
                }
                return Redirect("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to delete appointment: {ex}", ex);
                AppointmentErrorMessage = ex.ToString();
                AppointmentError = true;

                return Page();
            }
        }

        #region Helpers
        private async Task<EventViewmodel> GetAppointmentAsync(int? id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{config["Sys:ApiUrl"]}/events/single?id={id}");

            var client = clientFactory.CreateClient();
            // LYTODO add authorization bearer token here for login.
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<EventViewmodel>(responseStream, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                return result;
            }
            else
            {
                AppointmentError = true;
                AppointmentErrorMessage = $"Unable to get appointment {response.ReasonPhrase}";
                return null;
            }
        }

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
