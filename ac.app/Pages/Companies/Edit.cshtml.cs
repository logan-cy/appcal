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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ac.app.Pages.Companies
{
    public class EditModel : PageModel
    {
        [BindProperty]
        public CompanyViewmodel Company { get; set; }

        private readonly ILogger<EditModel> _logger;
        private readonly ApplicationDbContext context;

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
                    _ = int.TryParse(id.ToString(), out int companyId);
                    Company = await GetCompanyAsync(companyId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Companies IndexModel] OnGet failed");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var company = await context.Companies.FindAsync(Company.Id);
                if (company == null)
                {
                    return NotFound(new { message = $"Company with ID {Company.Id} was not found." });
                }
                company.Address = Company.Address;
                company.Name = Company.Name;

                await context.SaveChangesAsync();

                return Redirect("./Index");
            }
            catch (Exception)
            {
                return Page();
            }
        }

        private async Task<CompanyViewmodel> GetCompanyAsync(int id)
        {
            var company = await context.Companies.FindAsync(id);
            var model = new CompanyViewmodel
            {
                Address = company.Address,
                Id = company.Id,
                Name = company.Name
            };

            return model;
        }
    }
}
