using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;
using ac.api.Viewmodels;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.IO;
using Microsoft.AspNetCore.Http;
using ac.api.Data;
using ac.api.Models;
using Microsoft.EntityFrameworkCore;

namespace ac.app.Pages
{
    public class CreateModel : PageModel
    {
        private readonly ILogger<CreateModel> _logger;
        private readonly ApplicationDbContext context;

        public IEnumerable<SelectListItem> Companies { get; set; }
        public bool GetCompaniesError { get; private set; }

        [BindProperty]
        public EventViewmodel Appointment { get; set; }

        public bool SaveAppointmentError { get; private set; }
        public string SaveAppointmentErrorMessage { get; private set; }

        public CreateModel(ILogger<CreateModel> logger, ApplicationDbContext context)
        {
            _logger = logger;
            this.context = context;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Redirect("/Account/Login");
                }
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
                var company = await context.Companies.FindAsync(Appointment.CompanyId);
                if (company == null)
                {
                    return NotFound(new { message = $"Company with ID {Appointment.CompanyId} was not found." });
                }
                var client = await context.Clients.FindAsync(Appointment.ClientId);
                if (client == null)
                {
                    return NotFound(new { message = $"Client with ID {Appointment.ClientId} was not found." });
                }
                var ev = new CalendarEvent
                {
                    AllDay = Appointment.AllDay,
                    Client = client,
                    Company = company,
                    Description = Appointment.Description,
                    End = Appointment.End,
                    Start = Appointment.Start,
                    Title = Appointment.Title
                };

                await context.Events.AddAsync(ev);
                await context.SaveChangesAsync();

                // Once event is saved, set an edit URL where it can be updated.
                ev.Url = $"Edit?id={ev.Id}";
                await context.SaveChangesAsync();
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
            var companies = await context.Companies.Select(x => new CompanyViewmodel
            {
                Address = x.Address,
                Id = x.Id,
                Name = x.Name
            }).ToListAsync();

            return companies.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });
        }

        public async Task<IEnumerable<SelectListItem>> FilterClients(int companyId)
        {
            try
            {
                var company = await context.Companies.FindAsync(companyId);
                var clients = await context.Clients.Include(x => x.Company)
                    .Where(x => x.Company.Id == companyId).Select(x => new ClientViewmodel
                    {
                        Address = x.Address,
                        Company = new CompanyViewmodel
                        {
                            Address = x.Company.Address,
                            Id = x.Company.Id,
                            Name = x.Company.Name
                        },
                        CompanyId = x.Company.Id,
                        Email = x.Email,
                        Id = x.Id,
                        IdNumber = x.IdNumber,
                        Name = x.Name,
                        PhoneNumber = x.PhoneNumber
                    }).ToListAsync();

                return clients.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to save appointment: {ex}", ex);
                SaveAppointmentErrorMessage = ex.ToString();
                SaveAppointmentError = true;

                return null;
            }
        }
        #endregion
    }
}
