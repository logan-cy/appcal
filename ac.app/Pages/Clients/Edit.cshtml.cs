using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using ac.api.Constants;
using ac.api.Data;
using ac.api.Viewmodels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ac.app.Pages.Clients
{
    public class EditModel : PageModel
    {
        [BindProperty]
        public ClientViewmodel Client { get; set; }

        public IEnumerable<SelectListItem> Companies { get; set; }
        public bool GetCompaniesError { get; private set; }

        private readonly ILogger<EditModel> _logger;
        private readonly ApplicationDbContext context;

        public int CompanyId { get; private set; }
        public bool IsCompany { get; set; }

        public EditModel(ILogger<EditModel> logger, ApplicationDbContext context)
        {
            _logger = logger;
            this.context = context;
        }

        public async Task OnGetAsync(int? id)
        {
            try
            {
                if (id != null)
                {
                    if (User.Identity.IsAuthenticated && User.IsInRole(nameof(SystemRoles.Company)))
                    {
                        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        var companyUser = context.CompanyUsers.Include(x => x.Company).Include(x => x.User).First(x => x.User.Id == userId);

                        CompanyId = companyUser.Company.Id;
                        IsCompany = true;
                    }
                    else
                    {
                        Companies = await GetCompaniesAsync();
                    }
                    Client = await GetClientAsync(id);
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
                if (CompanyId == 0)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var companyUser = context.CompanyUsers.Include(x => x.Company).Include(x => x.User).First(x => x.User.Id == userId);

                    CompanyId = companyUser.Company.Id;
                }

                var company = await context.Companies.FindAsync(CompanyId);
                if (company == null)
                {
                    return NotFound(new { message = $"Company with ID {Client.CompanyId} was not found." });
                }

                var client = await context.Clients.FindAsync(Client.Id);
                if (client == null)
                {
                    return NotFound(new { message = $"Client with ID {Client.Id} was not found." });
                }
                client.Address = Client.Address;
                client.Company = company;
                client.Email = Client.Email;
                client.IdNumber = Client.IdNumber;
                client.Name = Client.Name;
                client.PhoneNumber = Client.PhoneNumber;

                await context.SaveChangesAsync();

                return Redirect("./Index");
            }
            catch (Exception)
            {
                return Page();
            }
        }

        private async Task<ClientViewmodel> GetClientAsync(int? id)
        {
            var client = await context.Clients.Include(x => x.Company).FirstOrDefaultAsync(x => x.Id == id);
            var model = new ClientViewmodel
            {
                Address = client.Address,
                Company = new CompanyViewmodel
                {
                    Address = client.Company.Address,
                    Id = client.Company.Id,
                    Name = client.Company.Name
                },
                CompanyId = client.Company.Id,
                Email = client.Email,
                Id = client.Id,
                IdNumber = client.IdNumber,
                Name = client.Name,
                PhoneNumber = client.PhoneNumber
            };

            return model;
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
    }
}
