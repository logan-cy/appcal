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

namespace ac.app.Pages.Divisions
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public IEnumerable<DivisionViewmodel> Divisions { get; set; }

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
                Divisions = await GetDivisionsAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Divisions IndexModel] OnGet failed");

                return BadRequest();
            }
        }

        private async Task<IEnumerable<DivisionViewmodel>> GetDivisionsAsync()
        {
            var divisions = await context.Divisions.Include(x => x.Company).Select(x => new DivisionViewmodel
            {
                CompanyId = x.Company.Id,
                Company = new CompanyViewmodel
                {
                    Address = x.Company.Address,
                    Id = x.Company.Id,
                    Name = x.Company.Name
                },
                Id = x.Id,
                Name = x.Name
            }).ToListAsync();

            return divisions;
        }
    }
}
