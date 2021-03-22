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
    public class DeleteModel : PageModel
    {
        [BindProperty]
        public ProductSetViewmodel ProductSet { get; set; }

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
                    _ = int.TryParse(id.ToString(), out int productSetId);
                    ProductSet = await GetProductSetAsync(productSetId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Product Set DeleteModel] OnGet failed");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                await DeleteProductSetAsync(ProductSet.Id);

                return Redirect("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Product Set DeleteModel] Failed to delete product set");
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

        private async Task DeleteProductSetAsync(int id)
        {
            var productSet = await context.ProductSets.Include(x => x.Division).Include(x => x.Products).FirstOrDefaultAsync(x => x.Id == id);

            // Remove all products from the set so as to satisfy FK constraints.
            var setProducts = productSet.Products;
            foreach (var product in setProducts)
            {
                if (productSet.Products.Any(x => x.Id == product.Id))
                {
                    productSet.Products.Remove(product);
                }
            }

            context.ProductSets.Remove(productSet);
            await context.SaveChangesAsync();
        }
    }
}
