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
    public class IndexModel : PageModel
    {
        [BindProperty]
        public IEnumerable<ProductViewmodel> Products { get; set; }

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
                Products = await GetProductsAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Products IndexModel] OnGet failed");

                return BadRequest();
            }
        }

        private async Task<IEnumerable<ProductViewmodel>> GetProductsAsync()
        {
            var products = await context.Products
                .Include(x => x.Division)
                .Include(x => x.Division.Company).Select(x => new ProductViewmodel
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

            return products;
        }
    }
}
