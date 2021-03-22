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
    public class DeleteModel : PageModel
    {
        [BindProperty]
        public DivisionViewmodel Division { get; set; }

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
                    _ = int.TryParse(id.ToString(), out int divisionId);
                    Division = await GetDivisionAsync(divisionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Divisions DeleteModel] OnGet failed");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                await DeleteDivisionAsync(Division.Id);

                return Redirect("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Divisions DeleteModel] Failed to delete company");
                return Page();
            }
        }

        private async Task<DivisionViewmodel> GetDivisionAsync(int id)
        {
            var division = await context.Divisions.Include(x => x.Company).FirstOrDefaultAsync(x => x.Id == id);
            var model = new DivisionViewmodel
            {
                CompanyId = division.Company.Id,
                Company = new CompanyViewmodel
                {
                    Address = division.Company.Address,
                    Id = division.Company.Id,
                    Name = division.Company.Name
                },
                Id = division.Id,
                Name = division.Name
            };

            return model;
        }

        private async Task DeleteDivisionAsync(int id)
        {
            var division = await context.Divisions.FindAsync(id);

            // If the division is deleted before the products, there could be a risk of data corruption so clear out products first.
            var products = await context.Products.Where(x => x.Division.Id == id).ToListAsync();
            context.Products.RemoveRange(products);

            context.Divisions.Remove(division);
            await context.SaveChangesAsync();
        }
    }
}
