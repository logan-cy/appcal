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
    public class IndexModel : PageModel
    {
        [BindProperty]
        public IEnumerable<ClientViewmodel> Clients { get; set; }

        private readonly ILogger<IndexModel> _logger;
        private readonly ApplicationDbContext context;

        public IndexModel(ILogger<IndexModel> logger, ApplicationDbContext context)
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
                Clients = await GetClientsAsync();

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Clients IndexModel] OnGet failed");

                return BadRequest();
            }
        }

        private async Task<IEnumerable<ClientViewmodel>> GetClientsAsync()
        {
            var data = await context.Clients.Include(x => x.Company).ToListAsync();
            var clients = data.Select(x => new ClientViewmodel
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
            });

            return clients;
        }
    }
}
