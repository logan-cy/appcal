using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using ac.api.Constants;
using ac.api.Data;
using ac.api.Viewmodels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ac.app.Pages.Divisions
{
    public class EditModel : PageModel
    {
        [BindProperty]
        public DivisionViewmodel Division { get; set; }
        public bool SaveDivisionError { get; private set; }
        public string SaveDivisionErrorMessage { get; private set; }

        public IEnumerable<SelectListItem> Companies { get; set; }
        public bool GetCompaniesError { get; private set; }

        public bool IsCompany { get; set; }
        public int CompanyId { get; private set; }

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
                    Division = await GetDivisionAsync(id);
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
                    return NotFound(new { message = $"Company with ID {Division.CompanyId} was not found." });
                }

                var division = await context.Divisions.FindAsync(Division.Id);
                if (division == null)
                {
                    return NotFound(new { message = $"Division with ID {Division.Id} was not found." });
                }
                division.Company = company;
                division.Name = Division.Name;

                await context.SaveChangesAsync();
                return Redirect("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to save division: {ex}", ex);
                SaveDivisionErrorMessage = ex.ToString();
                SaveDivisionError = true;

                return Page();
            }
        }

        #region Helpers
        private async Task<DivisionViewmodel> GetDivisionAsync(int? id)
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
        #endregion
    }
}
