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
    public class DeleteModel : PageModel
    {
        [BindProperty]
        public CompanyViewmodel Company { get; set; }

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
                    _ = int.TryParse(id.ToString(), out int companyId);
                    Company = await GetCompanyAsync(companyId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Companies DeleteModel] OnGet failed");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                await DeleteCompanyAsync(Company.Id);

                return Redirect("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Companies DeleteModel] Failed to delete company");
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

        private async Task DeleteCompanyAsync(int id)
        {
            var company = await context.Companies.FindAsync(id);

            // Avoid possible data corruption by deleting products and divisions before deleting the company.
            var divisions = await context.Divisions.Where(x => x.Company.Id == id).ToListAsync();
            foreach (var division in divisions)
            {
                var products = context.Products.Where(x => x.Division.Id == division.Id);
                context.Products.RemoveRange(products);
            }
            context.Divisions.RemoveRange(divisions);

            context.Companies.Remove(company);
            await context.SaveChangesAsync();
        }
    }
}
