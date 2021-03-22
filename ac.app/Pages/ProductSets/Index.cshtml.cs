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

namespace ac.app.Pages.ProductSets
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public IEnumerable<ProductSetViewmodel> ProductSets { get; set; }

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
                ProductSets = await GetProductSetsAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Product Sets IndexModel] OnGet failed");

                return BadRequest();
            }
        }

        private async Task<IEnumerable<ProductSetViewmodel>> GetProductSetsAsync()
        {
            var sets = await context.ProductSets
                .Include(x => x.Division)
                .Include(x => x.Division.Company)
                .Include(x => x.Products).ToListAsync();
            var model = new List<ProductSetViewmodel>();

            // Make sure that the Products list belonging to each set is being built correctly.
            foreach (var set in sets)
            {
                // Create a new set VM for each set in the collection.
                var productSet = new ProductSetViewmodel
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
                var products = new List<ProductViewmodel>();
                foreach (var product in set.Products)
                {
                    var p = await context.Products
                    .Include(x => x.Division).Include(x => x.Division.Company).FirstOrDefaultAsync(x => x.Id == product.Id);
                    products.Add(new ProductViewmodel
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
                productSet.Products = products;

                // Add the new VM to the model returned by this endpoint.
                model.Add(productSet);
            }

            return model;
        }
    }
}
