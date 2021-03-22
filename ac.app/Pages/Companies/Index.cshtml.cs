using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ac.api.Data;
using ac.api.Viewmodels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ac.app.Pages.Companies
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public IEnumerable<CompanyViewmodel> Companies { get; set; }

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
                Companies = await GetCompaniesAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Companies IndexModel] OnGet failed");

                return BadRequest();
            }
        }

        private async Task<IEnumerable<CompanyViewmodel>> GetCompaniesAsync()
        {
            var companies = await context.Companies.Select(x => new CompanyViewmodel
            {
                Address = x.Address,
                Id = x.Id,
                Name = x.Name
            }).ToListAsync();

            return companies;
        }
    }
}
