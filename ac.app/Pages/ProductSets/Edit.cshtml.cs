using System;
using System.Collections.Generic;
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
    public class EditModel : PageModel
    {
        [BindProperty]
        public ProductSetViewmodel ProductSet { get; set; }

        [BindProperty]
        public List<int> SelectedProducts { get; set; }

        public IEnumerable<SelectListItem> Companies { get; set; }
        public bool GetCompaniesError { get; private set; }

        public IEnumerable<SelectListItem> Divisions { get; set; }
        public bool GetDivisionsError { get; private set; }
        public bool IsCompany { get; private set; }

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
                    _ = int.TryParse(id.ToString(), out int productSetId);
                    if (User.Identity.IsAuthenticated && User.IsInRole(nameof(SystemRoles.Company)))
                    {
                        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        var companyUser = context.CompanyUsers.Include(x => x.Company).Include(x => x.User).First(x => x.User.Id == userId);
                        if (ProductSet == null)
                            ProductSet.CompanyId = companyUser.Company.Id;

                        IsCompany = true;
                    }
                    else
                    {
                        Companies = await GetCompaniesAsync();
                    }
                    Divisions = await GetDivisionsAsync(ProductSet.CompanyId);
                    ProductSet = await GetProductSetAsync(productSetId);

                    SelectedProducts = new List<int>();
                    foreach (var product in ProductSet.Products)
                    {
                        SelectedProducts.Add(product.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Product Set EditModel] OnGet failed");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var division = await context.Divisions.FindAsync(ProductSet.DivisionId);
                if (division == null)
                {
                    return NotFound(new { message = $"Company division with ID {ProductSet.DivisionId} was not found." });
                }

                var set = await context.ProductSets
                    .Include(x => x.Division)
                    .Include(x => x.Products)
                    .FirstOrDefaultAsync(x => x.Id == ProductSet.Id);
                set.Division = division;
                set.Name = ProductSet.Name;

                // Remove all products from the set.
                var setProducts = set.Products;
                foreach (var product in setProducts)
                {
                    if (set.Products.Any(x => x.Id == product.Id))
                    {
                        set.Products.Remove(product);
                    }
                }

                // Add the new selection of products to the set.
                var products = new List<Product>();
                foreach (var p in ProductSet.Products)
                {
                    var product = await context.Products.FindAsync(p.Id);
                    products.Add(product);
                }
                set.Products = products;
                await context.SaveChangesAsync();

                return Redirect("./Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to save product: {ex}", ex);
                return Page();
            }
        }

        private async Task<ProductSetViewmodel> GetProductSetAsync(int id)
        {
            var set = await context.ProductSets
                .Include(x => x.Division)
                .Include(x => x.Division.Company)
                .Include(x => x.Products).FirstOrDefaultAsync(x => x.Id == id);

            var model = new ProductSetViewmodel
            {
                Company = new CompanyViewmodel
                {
                    Id = set.Division.Company.Id,
                    Name = set.Division.Company.Name
                },
                CompanyId = set.Division.Company.Id,
                Division = new DivisionViewmodel
                {
                    Id = set.Division.Id,
                    Name = set.Division.Name
                },
                DivisionId = set.Division.Id,
                Id = set.Id,
                Name = set.Name,
                Products = new List<ProductViewmodel>()
            };

            // Loop through the products and add them to the set.
            foreach (var product in set.Products)
            {
                var p = await context.Products.Include(x => x.Division).FirstOrDefaultAsync(x => x.Id == product.Id);
                model.Products.Add(new ProductViewmodel
                {
                    Company = new CompanyViewmodel
                    {
                        Id = p.Division.Company.Id,
                        Name = p.Division.Company.Name
                    },
                    CompanyId = p.Division.Company.Id,
                    Division = new DivisionViewmodel
                    {
                        Id = p.Division.Id,
                        Name = p.Division.Name
                    },
                    DivisionId = p.Division.Id,
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price
                });
            }

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

            return companies.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });
        }

        private async Task<IEnumerable<SelectListItem>> GetDivisionsAsync(int companyId)
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

            return divisions.Select(x => new SelectListItem
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

                return null;
            }
        }

        public async Task<IEnumerable<SelectListItem>> FilterProducts(int divisionId)
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

                return products.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to filter products: {ex}", ex);

                return null;
            }
        }
        #endregion
    }
}
