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
    public class CompaniesController : ControllerBase
    {
        private readonly ILogger<CompaniesController> _logger;
        private readonly ApplicationDbContext context;

        public CompaniesController(ILogger<CompaniesController> logger, ApplicationDbContext context)
        {
            this.context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get a list of all companies currently stored.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var companies = await context.Companies.Select(x => new CompanyViewmodel
                {
                    Address = x.Address,
                    Id = x.Id,
                    Name = x.Name
                }).ToListAsync();

                return Ok(companies);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to get companies", ex);
                return BadRequest(ex.ToString());
            }
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search(string query)
        {
            try
            {
                var companies = context.Companies.AsQueryable();

                if (companies.Any(x => x.Address.Contains(query)))
                {
                    companies = companies.Where(x => x.Address.Contains(query));
                }
                if (companies.Any(x => x.Name.Contains(query)))
                {
                    companies = companies.Where(x => x.Name.Contains(query));
                }

                var model = await companies.Select(x => new CompanyViewmodel
                {
                    Address = x.Address,
                    Id = x.Id,
                    Name = x.Name
                }).ToListAsync();

                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to search for companies", ex);
                return BadRequest(ex.ToString());
            }
        }

        /// <summary>
        /// Get a single company as indicated by the specified Company ID parameter value.
        /// </summary>
        /// <param name="id" type="int">The ID value of the company to be retrieved.</param>
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var company = await context.Companies.FindAsync(id);
                if (company == null)
                {
                    throw new ArgumentException($"Company with ID {id} was not found.");
                }
                var model = new CompanyViewmodel
                {
                    Address = company.Address,
                    Id = company.Id,
                    Name = company.Name
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to get company", ex);
                return BadRequest(ex.ToString());
            }
        }

        /// <summary>
        /// Creates a new company with the given details.
        /// </summary>
        /// <param name="model" type="CompanyViewmodel">The model containing the new company details.</param>
        [HttpPost("create")]
        public async Task<IActionResult> Create(CompanyViewmodel model)
        {
            try
            {
                var company = new Company
                {
                    Address = model.Address,
                    Name = model.Name
                };
                await context.Companies.AddAsync(company);
                await context.SaveChangesAsync();
                return Ok(company);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to create company", ex);
                return BadRequest(ex.ToString());
            }
        }

        /// <summary>
        /// Updates the relevant company as indicated by the id parameter.
        /// </summary>
        /// <param name="model" type="CompanyViewmodel">The model containing the new company details.</param>
        /// <param name="id" type="int">The ID value of the company to be edited.</param>
        [HttpPost("edit")]
        public async Task<IActionResult> Edit(CompanyViewmodel model, int id)
        {
            try
            {
                var company = await context.Companies.FindAsync(id);
                if (company == null)
                {
                    throw new ArgumentException($"Company with ID {id} was not found.");
                }
                company.Address = model.Address;
                company.Name = model.Name;

                await context.SaveChangesAsync();

                return Ok(company);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to update division", ex);
                return BadRequest(ex.ToString());
            }
        }

        /// <summary>
        /// Deletes the selected company as well as all of the divisions and products that belong to it.
        /// NOTE: This action is irreversible
        /// </summary>
        /// <param name="id" type="int">The ID value of the company to be deleted.</param>
        [HttpPost("delete")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var company = await context.Companies.FindAsync(id);
                if (company == null)
                {
                    throw new ArgumentException($"Company with ID {id} was not found.");
                }

                // Avoid possible data corruption by deleting products and divisions before deleting the company.
                var divisions = await context.Divisions.Where(x => x.Company.Id == id).ToListAsync();
                foreach (var division in divisions)
                {
                    var products = context.Products.Where(x => x.Division.Id == division.Id);
                    context.Products.RemoveRange(products);
                }
                context.Divisions.RemoveRange(divisions);

                context.Companies.Remove(company);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to delete company", ex);
                return BadRequest(ex.ToString());
            }
        }
    }
}
