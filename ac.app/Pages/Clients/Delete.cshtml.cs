using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ac.api.Data;
using ac.api.Viewmodels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ac.app.Pages.Clients
{
    public class DeleteModel : PageModel
    {
        [BindProperty]
        public ClientViewmodel Client { get; set; }

        private readonly ILogger<DeleteModel> _logger;
        private readonly ApplicationDbContext context;

        public DeleteModel(ILogger<DeleteModel> logger, ApplicationDbContext context)
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
                    _ = int.TryParse(id.ToString(), out int clientId);
                    Client = await GetClientAsync(clientId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Clients DeleteModel] OnGet failed");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                await DeleteCompanyAsync(Client.Id);

                return Redirect("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Clients DeleteModel] Failed to delete client");
                return Page();
            }
        }

        private async Task<ClientViewmodel> GetClientAsync(int id)
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

        private async Task DeleteCompanyAsync(int id)
        {
            var client = await context.Clients.FindAsync(id);

            context.Clients.Remove(client);
            await context.SaveChangesAsync();
        }
    }
}
