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
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ac.app.Pages
{
    public class EditModel : PageModel
    {
        private readonly ILogger<EditModel> _logger;
        private readonly ApplicationDbContext context;

        public IEnumerable<SelectListItem> Companies { get; set; }
        public bool GetCompaniesError { get; private set; }

        [BindProperty]
        public EventViewmodel Appointment { get; set; }

        public bool AppointmentError { get; private set; }
        public string AppointmentErrorMessage { get; private set; }

        public EditModel(ILogger<EditModel> logger, ApplicationDbContext context)
        {
            _logger = logger;
            this.context = context;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Redirect("/Account/Login");
                }

                if (id == null)
                {
                    AppointmentError = true;
                    AppointmentErrorMessage = "Appointment wasn't found.";
                }
                Companies = await GetCompaniesAsync();
                Appointment = await GetAppointmentAsync(id);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[EditModel] OnGet failed");
                AppointmentError = true;
                AppointmentErrorMessage = ex.ToString();

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
                var product = await context.Products.FindAsync(Appointment.ProductId);
                if (product == null)
                {
                    return NotFound(new { message = $"Product with ID {Appointment.ProductId} was not found." });
                }

                var ev = await context.Events.FindAsync(Appointment.Id);
                ev.AllDay = (Appointment.End == DateTime.MinValue || Appointment.End == Appointment.Start.AddDays(1));
                ev.Client = client;
                ev.Company = company;
                ev.Description = Appointment.Description;
                ev.End = Appointment.End;
                ev.Product = product;
                ev.Start = Appointment.Start;
                ev.Title = Appointment.Title;

                await context.SaveChangesAsync();
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
                var ev = await context.Events.FindAsync(id);
                context.Events.Remove(ev);
                await context.SaveChangesAsync();

                return Redirect("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to delete appointment: {ex}", ex);
                AppointmentErrorMessage = ex.ToString();
                ModelState.AddModelError("", $"Unable to delete appointment: {ex}");
                AppointmentError = true;

                return Page();
            }
        }

        #region Helpers
        private async Task<EventViewmodel> GetAppointmentAsync(int? id)
        {
            var ev = await context.Events
                .Include(x => x.Company)
                .Include(x => x.Client)
                .Include(x => x.Product)
                .SingleOrDefaultAsync(x => x.Id == id);

            var appointment = new EventViewmodel
            {
                AllDay = ev.AllDay,
                ClientId = ev.Client.Id,
                CompanyId = ev.Company.Id,
                Description = ev.Description,
                End = ev.End,
                Id = ev.Id,
                ProductId = ev.Product.Id,
                Start = ev.Start,
                Title = ev.Title,
                Url = ev.Url
            };

            return appointment;
        }

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
                AppointmentErrorMessage = ex.ToString();
                AppointmentError = true;

                return null;
            }
        }
        #endregion
    }
}
