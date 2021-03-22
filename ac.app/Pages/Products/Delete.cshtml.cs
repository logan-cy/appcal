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

namespace ac.app.Pages.Products
{
    public class DeleteModel : PageModel
    {
        [BindProperty]
        public ProductViewmodel Product { get; set; }

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
                    _ = int.TryParse(id.ToString(), out int productId);
                    Product = await GetProductAsync(productId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Products DeleteModel] OnGet failed");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                await DeleteProductAsync(Product.Id);

                return Redirect("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Products DeleteModel] Failed to delete company");
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

        private async Task DeleteProductAsync(int id)
        {
            var product = await context.Products.FindAsync(id);

            context.Products.Remove(product);
            await context.SaveChangesAsync();
        }
    }
}
