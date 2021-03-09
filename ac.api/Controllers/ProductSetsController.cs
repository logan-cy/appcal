using System.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ac.api.Data;
using ac.api.Models;
using ac.api.Viewmodels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ac.api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductSetsController : ControllerBase
    {
        private readonly ILogger<ProductSetsController> _logger;
        private readonly ApplicationDbContext context;

        public ProductSetsController(ILogger<ProductSetsController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            this.context = context;
        }

        /// <summary>
        /// Get a list of all product sets currently stored.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var sets = await context.ProductSets
                    .Include(x => x.Division)
                    .Include(x => x.Products).ToListAsync();
                var model = new List<ProductSetViewmodel>();

                // Make sure that the Products list belonging to each set is being built correctly.
                foreach (var set in sets)
                {
                    // Create a new set VM for each set in the collection.
                    var productSet = new ProductSetViewmodel
                    {
                        DivisionId = set.Division.Id,
                        Id = set.Id,
                        Name = set.Name
                    };
                    // Loop through the products and add them to the set.
                    var products = new List<ProductViewmodel>();
                    foreach (var product in set.Products)
                    {
                        var p = await context.Products
                        .Include(x => x.Division).Include(x => x.Division.Company).FirstOrDefaultAsync(x => x.Id == product.Id);
                        products.Add(new ProductViewmodel
                        {
                            CompanyId = p.Division.Company.Id,
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

                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to get product sets", ex);
                return BadRequest(ex.ToString());
            }
        }

        /// <summary>
        /// Get a list of product sets filtered by the specified Division ID parameter value.
        /// </summary>
        [HttpGet("filter")]
        public async Task<IActionResult> FilterByDivision(int divisionId)
        {
            try
            {
                var sets = await context.ProductSets
                    .Include(x => x.Division)
                    .Include(x => x.Products)
                    .Where(x => x.Division.Id == divisionId).ToListAsync();
                var model = new List<ProductSetViewmodel>();

                // Make sure that the Products list belonging to each set is being built correctly.
                foreach (var set in sets)
                {
                    // Create a new set VM for each set in the collection.
                    var productSet = new ProductSetViewmodel
                    {
                        DivisionId = set.Division.Id,
                        Id = set.Id,
                        Name = set.Name
                    };
                    // Loop through the products and add them to the set.
                    foreach (var product in set.Products)
                    {
                        var p = await context.Products.Include(x => x.Division).FirstOrDefaultAsync(x => x.Id == product.Id);
                        productSet.Products.Add(new ProductViewmodel
                        {
                            DivisionId = p.Division.Id,
                            Name = p.Name,
                            Price = p.Price
                        });
                    }

                    // Add the new VM to the model returned by this endpoint.
                    model.Add(productSet);
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to get product sets", ex);
                return BadRequest(ex.ToString());
            }
        }

        /// <summary>
        /// Get a single product set as indicated by the specified Product set ID parameter value.
        /// </summary>
        /// <param name="id" type="int">The ID value of the product to be retrieved.</param>
        [HttpGet("single")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var set = await context.ProductSets.Include(x => x.Division).Include(x => x.Products).FirstOrDefaultAsync(x => x.Id == id);
                if (set == null)
                {
                    return NotFound(new { message = $"Product set with ID {id} was not found." });
                }

                var model = new ProductSetViewmodel
                {
                    DivisionId = set.Division.Id,
                    Id = set.Id,
                    Name = set.Name
                };

                // Loop through the products and add them to the set.
                foreach (var product in set.Products)
                {
                    var p = await context.Products.Include(x => x.Division).FirstOrDefaultAsync(x => x.Id == product.Id);
                    model.Products.Add(new ProductViewmodel
                    {
                        DivisionId = p.Division.Id,
                        Name = p.Name,
                        Price = p.Price
                    });
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to get product set", ex);
                return BadRequest(ex.ToString());
            }
        }

        /// <summary>
        /// Creates a new product set with the given details.
        /// </summary>
        /// <param name="model" type="ProductSetViewmodel">The model containing the new product set details.</param>
        [HttpPost("create")]
        public async Task<IActionResult> Create(ProductSetViewmodel model)
        {
            try
            {
                var division = await context.Divisions.FindAsync(model.DivisionId);
                if (division == null)
                {
                    return NotFound(new { message = $"Company division with ID {model.DivisionId} was not found." });
                }

                var set = new ProductSet
                {
                    Division = division,
                    Name = model.Name
                };
                var products = new List<Product>();
                foreach (var p in model.Products)
                {
                    var product = await context.Products.FindAsync(p.Id);
                    if (product == null)
                    {
                        return NotFound(new { message = $"Product with ID {p.Id} ({p.Name}) was not found." });
                    }
                    products.Add(product);
                }
                set.Products = products;

                await context.ProductSets.AddAsync(set);
                await context.SaveChangesAsync();

                return Ok(set);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to create product set", ex);
                return BadRequest(ex.ToString());
            }
        }

        /// <summary>
        /// Updates an exist product set with the given details.
        /// </summary>
        /// <param name="model" type="ProductSetViewmodel">The model containing the new product set details.</param>
        /// <param name="id" type="int">The ID value of the product set to ber edited.</param>
        [HttpPost("edit")]
        public async Task<IActionResult> Edit(ProductSetViewmodel model, int id)
        {
            try
            {
                var division = await context.Divisions.FindAsync(model.DivisionId);
                if (division == null)
                {
                    return NotFound(new { message = $"Company division with ID {model.DivisionId} was not found." });
                }

                var set = await context.ProductSets
                    .Include(x => x.Division)
                    .Include(x => x.Products)
                    .FirstOrDefaultAsync(x => x.Id == id);
                set.Division = division;
                set.Name = model.Name;

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
                foreach (var p in model.Products)
                {
                    var product = await context.Products.FindAsync(p.Id);
                    products.Add(product);
                }
                set.Products = products;
                await context.SaveChangesAsync();

                return Ok(set);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to update product set", ex);
                return BadRequest(ex.ToString());
            }
        }

        /// <summary>
        /// Deletes the selected product set.
        /// NOTE: This action is irreversible
        /// </summary>
        /// <param name="id" type="int">The ID value of the product set to be deleted.</param>
        [HttpPost("delete")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var productSet = await context.ProductSets.Include(x => x.Division).Include(x => x.Products).FirstOrDefaultAsync(x => x.Id == id);
                if (productSet == null)
                {
                    return NotFound(new { message = $"Product set with ID {id} was not found." });
                }

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
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to delete product set", ex);
                return BadRequest(ex.ToString());
            }
        }
    }
}