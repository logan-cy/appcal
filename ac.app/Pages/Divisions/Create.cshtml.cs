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
using ac.api.Models;
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
    public class CreateModel : PageModel
    {
        [BindProperty]
        public DivisionViewmodel Division { get; set; }
        public bool SaveDivisionError { get; private set; }
        public string SaveDivisionErrorMessage { get; private set; }

        public IEnumerable<SelectListItem> Companies { get; set; }
        public bool GetCompaniesError { get; private set; }
        public int CompanyId { get; private set; }
        public bool IsCompany { get; set; }

        private readonly ILogger<CreateModel> _logger;
        private readonly ApplicationDbContext context;

        public CreateModel(ILogger<CreateModel> logger, ApplicationDbContext context)
        {
            _logger = logger;
            this.context = context;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
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

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to load page: {ex}", ex);
                return BadRequest();
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

                var division = new Division
                {
                    Company = company,
                    Name = Division.Name
                };
                await context.Divisions.AddAsync(division);
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
