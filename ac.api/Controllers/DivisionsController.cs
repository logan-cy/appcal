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
    public class DivisionsController : ControllerBase
    {
        private readonly ILogger<DivisionsController> _logger;
        private readonly ApplicationDbContext context;

        public DivisionsController(ILogger<DivisionsController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            this.context = context;
        }

        /// <summary>
        /// Get a list of all divisions currently stored.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var divisions = await context.Divisions.Include(x => x.Company).Select(x => new DivisionViewmodel
                {
                    CompanyId = x.Company.Id,
                    Id = x.Id,
                    Name = x.Name
                }).ToListAsync();

                return Ok(divisions);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to get divisions", ex);
                return BadRequest(ex.ToString());
            }
        }

        /// <summary>
        /// Get a list of divisions indicated by the specified Company ID parameter value.
        /// </summary>
        /// <param name="companyId" type="int">The ID value of the company whose divisions are to be retrieved.</param>
        [HttpGet("filter")]
        public async Task<IActionResult> FilterByCompany(int companyId)
        {
            try
            {
                var company = await context.Companies.FindAsync(companyId);
                if (company == null)
                {
                    throw new ArgumentException($"Company with ID {companyId} was not found.");
                }
                var divisions = await context.Divisions.Include(x => x.Company)
                    .Where(x => x.Company.Id == companyId).Select(x => new DivisionViewmodel
                    {
                        CompanyId = x.Company.Id,
                        Id = x.Id,
                        Name = x.Name
                    }).ToListAsync();

                return Ok(divisions);
            }
            catch (Exception ex)
            {
                var company = await context.Companies.FindAsync(companyId);
                if (company == null)
                {
                    throw new ArgumentException($"Company with ID {companyId} was not found.");
                }
                _logger.LogError($"Unable to get divisions for company '{company.Name}'", ex);
                return BadRequest(ex.ToString());
            }
        }

        /// <summary>
        /// Get a single division as indicated by the specified Division ID parameter value.
        /// </summary>
        /// <param name="id" type="int">The ID value of the division to be retrieved.</param>
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var division = await context.Divisions.Include(x => x.Company).FirstOrDefaultAsync(x => x.Id == id);
                if (division == null)
                {
                    throw new ArgumentException($"Division with ID {id} was not found.");
                }
                var model = new DivisionViewmodel
                {
                    CompanyId = division.Company.Id,
                    Id = division.Id,
                    Name = division.Name
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to get division", ex);
                return BadRequest(ex.ToString());
            }
        }

        /// <summary>
        /// Creates a new division with the given details.
        /// </summary>
        /// <param name="model" type="DivisionViewmodel">The model containing the new division details.</param>
        [HttpPost("create")]
        public async Task<IActionResult> Create(DivisionViewmodel model)
        {
            try
            {
                var company = await context.Companies.FindAsync(model.CompanyId);
                if (company == null)
                {
                    throw new ArgumentException($"Company with ID {model.CompanyId} was not found.");
                }

                var division = new Division
                {
                    Company = company,
                    Name = model.Name
                };
                await context.Divisions.AddAsync(division);
                await context.SaveChangesAsync();
                return Ok(division);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to create division", ex);
                return BadRequest(ex.ToString());
            }
        }

        /// <summary>
        /// Updates the relevant division as indicated by the id parameter.
        /// </summary>
        /// <param name="model" type="DivisionViewmodel">The model containing the new division details.</param>
        /// <param name="id" type="int">The ID value of the division to be edited.</param>
        [HttpPost("edit")]
        public async Task<IActionResult> Edit(DivisionViewmodel model, int id)
        {
            try
            {
                var company = await context.Companies.FindAsync(model.CompanyId);
                if (company == null)
                {
                    throw new ArgumentException($"Company with ID {model.CompanyId} was not found.");
                }

                var division = await context.Divisions.FindAsync(id);
                if (division == null)
                {
                    throw new ArgumentException($"Division with ID {id} was not found.");
                }
                division.Company = company;
                division.Name = model.Name;

                await context.SaveChangesAsync();

                return Ok(division);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to update division", ex);
                return BadRequest(ex.ToString());
            }
        }

        /// <summary>
        /// Deletes the selected division as well as all of the products that belong to it.
        /// NOTE: This action is irreversible
        /// </summary>
        /// <param name="id" type="int">The ID value of the division to be deleted.</param>
        [HttpPost("delete")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var division = await context.Divisions.FindAsync(id);
                if (division == null)
                {
                    throw new ArgumentException($"Division with ID {id} was not found.");
                }

                // If the division is deleted before the products, there could be a risk of data corruption so clear out products first.
                var products = await context.Products.Where(x => x.Division.Id == id).ToListAsync();
                context.Products.RemoveRange(products);

                context.Divisions.Remove(division);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to delete division", ex);
                return BadRequest(ex.ToString());
            }
        }
    }
}
