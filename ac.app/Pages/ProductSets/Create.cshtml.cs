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

namespace ac.app.Pages.ProductSets
{
    public class CreateModel : PageModel
    {
        [BindProperty]
        public ProductSetViewmodel ProductSet { get; set; }

        [BindProperty]
        public List<int> SelectedProducts { get; set; }

        public bool SaveSetError { get; private set; }
        public string SaveSetErrorMessage { get; private set; }

        public IEnumerable<SelectListItem> Companies { get; set; }
        public IEnumerable<SelectListItem> Divisions { get; set; }

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

                    Divisions = await GetDivisionsAsync();
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
                var division = await context.Divisions.FindAsync(ProductSet.DivisionId);
                var set = new ProductSet
                {
                    Division = division,
                    Name = ProductSet.Name
                };

                var products = new List<Product>();
                foreach (var p in ProductSet.Products)
                {
                    var product = await context.Products.FindAsync(p.Id);
                    products.Add(product);
                }
                set.Products = products;

                await context.ProductSets.AddAsync(set);
                await context.SaveChangesAsync();

                return Redirect("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to save appointment: {ex}", ex);
                SaveSetErrorMessage = ex.ToString();
                SaveSetError = true;

                return Page();
            }
        }

        #region Helpers
        public async Task<IEnumerable<SelectListItem>> GetDivisionsAsync()
        {
            try
            {
                var company = await context.Companies.FindAsync(CompanyId);
                var divisions = await context.Divisions.Include(x => x.Company)
                    .Where(x => x.Company.Id == CompanyId).Select(x => new DivisionViewmodel
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
                _logger.LogError($"Unable to filter divisions (GetDivisionsAsync): {ex}", ex);
                SaveSetErrorMessage = ex.ToString();
                SaveSetError = true;

                return null;
            }
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
                _logger.LogError($"Unable to filter divisions: {ex}", ex);
                SaveSetErrorMessage = ex.ToString();
                SaveSetError = true;

                return null;
            }
        }

        public async Task<JsonResult> OnPostFilterProductsAsync(int divisionId)
        {
            try
            {
                var division = await context.Divisions.FindAsync(divisionId);
                var products = await context.Products.Include(x => x.Division)
                    .Where(x => x.Division.Id == divisionId).Select(x => new ProductViewmodel
                    {
                        Company = new CompanyViewmodel
                        {
                            Id = x.Division.Company.Id,
                            Name = x.Division.Company.Name
                        },
                        CompanyId = x.Division.Company.Id,
                        Division = new DivisionViewmodel
                        {
                            Id = x.Division.Id,
                            Name = x.Division.Name
                        },
                        DivisionId = x.Division.Id,
                        Duration = x.Duration,
                        Id = x.Id,
                        Name = x.Name,
                        Price = x.Price
                    }).ToListAsync();

                var items = products.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                });
                var json = JsonSerializer.Serialize(items);
                return new JsonResult(json);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to filter products: {ex}", ex);
                SaveSetErrorMessage = ex.ToString();
                SaveSetError = true;

                return null;
            }
        }
        #endregion
    }
}
