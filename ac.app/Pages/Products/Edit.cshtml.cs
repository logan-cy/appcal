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

namespace ac.app.Pages.Products
{
    public class EditModel : PageModel
    {
        [BindProperty]
        public ProductViewmodel Product { get; set; }

        public bool SaveProductError { get; private set; }
        public string SaveProductErrorMessage { get; private set; }

        public IEnumerable<SelectListItem> Companies { get; set; }
        public bool GetCompaniesError { get; private set; }

        public IEnumerable<SelectListItem> Divisions { get; set; }
        public bool GetDivisionsError { get; private set; }

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
                    _ = int.TryParse(id.ToString(), out int productSetId);
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
                    Divisions = await GetDivisionsAsync(CompanyId);
                    Product = await GetProductAsync(productSetId);
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
                var division = await context.Divisions.FindAsync(Product.DivisionId);
                if (division == null)
                {
                    return NotFound(new { message = $"Company division with ID {Product.DivisionId} was not found." });
                }

                var product = await context.Products.FindAsync(Product.Id);
                if (product == null)
                {
                    return NotFound(new { message = $"Product with ID {Product.Id} was not found." });
                }
                product.Division = division;
                product.Duration = Product.Duration;
                product.Name = Product.Name;
                product.Price = Product.Price;

                await context.SaveChangesAsync();

                return Redirect("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to save product: {ex}", ex);
                SaveProductErrorMessage = ex.ToString();
                SaveProductError = true;

                return Page();
            }
        }

        private async Task<ProductViewmodel> GetProductAsync(int id)
        {
            var product = await context.Products
                .Include(x => x.Division)
                .Include(x => x.Division.Company).FirstOrDefaultAsync(x => x.Id == id);
            var model = new ProductViewmodel
            {
                Company = new CompanyViewmodel
                {
                    Id = product.Division.Company.Id,
                    Name = product.Division.Company.Name
                },
                CompanyId = product.Division.Company.Id,
                Division = new DivisionViewmodel
                {
                    Id = product.Division.Id,
                    Name = product.Division.Name
                },
                DivisionId = product.Division.Id,
                Duration = product.Duration,
                Id = product.Id,
                Name = product.Name,
                Price = product.Price
            };

            return model;
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

            return companies.Select(x => new SelectListItem {
                Text = x.Name,
                Value = x.Id.ToString()
            });
        }

        private async Task<IEnumerable<SelectListItem>> GetDivisionsAsync(int companyId = 0)
        {
            var divisions = context.Divisions.Include(x => x.Company).AsQueryable();

            if (companyId != 0)
            {
                divisions = divisions.Where(x => x.Company.Id == companyId);
            }
            
            var model = await divisions.Select(x => new DivisionViewmodel
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

            return model.Select(x => new SelectListItem {
                Text = x.Name,
                Value = x.Id.ToString()
            });
        }

        public async Task<IEnumerable<SelectListItem>> FilterDivisions(int companyId)
        {
            try
            {
                var company = await context.Companies.FindAsync(companyId);
                var divisions = await context.Divisions.Include(x => x.Company)
                    .Where(x => x.Company.Id == companyId).Select(x => new DivisionViewmodel
                    {
                        CompanyId = x.Company.Id,
                        Company = new CompanyViewmodel
                        {
                            Address = company.Address,
                            Id = company.Id,
                            Name = company.Name
                        },
                        Id = x.Id,
                        Name = x.Name
                    }).ToListAsync();

                return divisions.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to save appointment: {ex}", ex);
                SaveProductErrorMessage = ex.ToString();
                SaveProductError = true;

                return null;
            }
        }
        #endregion
    }
}
