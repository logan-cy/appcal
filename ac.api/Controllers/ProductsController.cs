using ac.api.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ac.api.Viewmodels;
using Microsoft.EntityFrameworkCore;
using ac.api.Models;
using Microsoft.AspNetCore.Authorization;

namespace ac.api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ILogger<ProductsController> _logger;
        private readonly ApplicationDbContext context;

        public ProductsController(ILogger<ProductsController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            this.context = context;
        }

        /// <summary>
        /// Get a list of all products currently stored.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
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

                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to get products", ex);
                return BadRequest(ex.ToString());
            }
        }

        /// <summary>
        /// Get a list of products filtered by the specified Division ID parameter value.
        /// </summary>
        /// <param name="divisionId" type="int">The ID value of the division whose products are to be retrieved.</param>
        [HttpGet("filter")]
        public async Task<IActionResult> FilterByDivision(int divisionId)
        {
            try
            {
                var division = await context.Divisions.FindAsync(divisionId);
                if (division == null)
                {
                    return NotFound(new { message = $"Division with ID {divisionId} was not found." });
                }
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

                return Ok(products);
            }
            catch (Exception ex)
            {
                var division = await context.Companies.FindAsync(divisionId);
                if (division == null)
                {
                    return NotFound(new { message = $"Division with ID {divisionId} was not found." });
                }
                _logger.LogError($"Unable to get products for division '{division.Name}'", ex);
                return BadRequest(ex.ToString());
            }
        }

        /// <summary>
        /// Get a single product as indicated by the specified Product ID parameter value.
        /// </summary>
        /// <param name="id" type="int">The ID value of the product to be retrieved.</param>
        [HttpGet("single")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var product = await context.Products
                    .Include(x => x.Division)
                    .Include(x => x.Division.Company).FirstOrDefaultAsync(x => x.Id == id);
                if (product == null)
                {
                    return NotFound(new { message = $"Product with ID {id} was not found." });
                }
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

                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to get product", ex);
                return BadRequest(ex.ToString());
            }
        }

        /// <summary>
        /// Creates a new product with the given details.
        /// </summary>
        /// <param name="model" type="ProductViewmodel">The model containing the new product details.</param>
        [HttpPost("create")]
        public async Task<IActionResult> Create(ProductViewmodel model)
        {
            try
            {
                var division = await context.Divisions.FindAsync(model.DivisionId);
                if (division == null)
                {
                    return NotFound(new { message = $"Company division with ID {model.DivisionId} was not found." });
                }

                var product = new Product
                {
                    Division = division,
                    Duration = model.Duration,
                    Name = model.Name,
                    Price = model.Price
                };
                await context.Products.AddAsync(product);
                await context.SaveChangesAsync();
                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to create product", ex);
                return BadRequest(ex.ToString());
            }
        }

        /// <summary>
        /// Updates the relevant product as indicated by the id parameter.
        /// </summary>
        /// <param name="model" type="ProductViewmodel">The model containing the new product details.</param>
        /// <param name="id" type="int">The ID value of the product to be edited.</param>
        [HttpPost("edit")]
        public async Task<IActionResult> Edit([FromBody] ProductViewmodel model, int id)
        {
            try
            {
                var division = await context.Divisions.FindAsync(model.DivisionId);
                if (division == null)
                {
                    return NotFound(new { message = $"Company division with ID {model.DivisionId} was not found." });
                }

                var product = await context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound(new { message = $"Product with ID {id} was not found." });
                }
                product.Division = division;
                product.Duration = model.Duration;
                product.Name = model.Name;
                product.Price = model.Price;

                await context.SaveChangesAsync();

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to update product", ex);
                return BadRequest(ex.ToString());
            }
        }

        /// <summary>
        /// Deletes the selected product.
        /// NOTE: This action is irreversible
        /// </summary>
        /// <param name="id" type="int">The ID value of the product to be deleted.</param>
        [HttpPost("delete")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var product = await context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound(new { message = $"Product with ID {id} was not found." });
                }

                context.Products.Remove(product);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to delete product", ex);
                return BadRequest(ex.ToString());
            }
        }
    }
}
