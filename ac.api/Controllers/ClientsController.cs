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
    public class ClientsController : ControllerBase
    {
        private readonly ILogger<ClientsController> _logger;
        private readonly ApplicationDbContext context;

        public ClientsController(ILogger<ClientsController> logger, ApplicationDbContext context)
        {
            this.context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get a list of all clients currently stored.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var clients = await context.Clients.Include(x => x.Company).Select(x => new ClientViewmodel
                {
                    Address = x.Address,
                    CompanyId = x.Company.Id,
                    Id = x.Id,
                    IdNumber = x.IdNumber,
                    Name = x.Name
                }).ToListAsync();

                return Ok(clients);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to get clients", ex);
                return BadRequest(ex.ToString());
            }
        }

        /// <summary>
        /// Get a list of clients indicated by the specified Company ID parameter value.
        /// </summary>
        /// <param name="companyId" type="int">The ID value of the company whose clients are to be retrieved.</param>
        [HttpGet("filter")]
        public async Task<IActionResult> FilterByCompany(int companyId)
        {
            try
            {
                var company = await context.Clients.FindAsync(companyId);
                if (company == null)
                {
                    return NotFound(new { message = $"Company with ID {companyId} was not found." });
                }
                var divisions = await context.Clients.Include(x => x.Company)
                    .Where(x => x.Company.Id == companyId).Select(x => new ClientViewmodel
                    {
                        Address = x.Address,
                        CompanyId = x.Company.Id,
                        Id = x.Id,
                        IdNumber = x.IdNumber,
                        Name = x.Name
                    }).ToListAsync();

                return Ok(divisions);
            }
            catch (Exception ex)
            {
                var company = await context.Companies.FindAsync(companyId);
                if (company == null)
                {
                    return NotFound(new { message = $"Company with ID {companyId} was not found." });
                }
                _logger.LogError($"Unable to get clients for company '{company.Name}'", ex);
                return BadRequest(ex.ToString());
            }
        }

        /// <summary>
        /// Get a single client as indicated by the specified Client ID parameter value.
        /// </summary>
        /// <param name="id" type="int">The ID value of the client to be retrieved.</param>
        [HttpGet("single")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var client = await context.Clients.Include(x => x.Company).FirstOrDefaultAsync(x => x.Id == id);
                if (client == null)
                {
                    return NotFound(new { message = $"Client with ID {id} was not found." });
                }
                var model = new ClientViewmodel
                {
                    Address = client.Address,
                    CompanyId = client.Company.Id,
                    Id = client.Id,
                    IdNumber = client.IdNumber,
                    Name = client.Name
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to get client", ex);
                return BadRequest(ex.ToString());
            }
        }

        /// <summary>
        /// Creates a new client with the given details.
        /// </summary>
        /// <param name="model" type="ClientViewmodel">The model containing the new client details.</param>
        [HttpPost("create")]
        public async Task<IActionResult> Create(ClientViewmodel model)
        {
            try
            {
                var company = await context.Companies.FindAsync(model.CompanyId);
                if (company == null)
                {
                    return NotFound(new { message = $"Company with ID {model.CompanyId} was not found." });
                }

                var client = new Client
                {
                    Address = model.Address,
                    Company = company,
                    Id = model.Id,
                    IdNumber = model.IdNumber,
                    Name = model.Name
                };
                await context.Clients.AddAsync(client);
                await context.SaveChangesAsync();
                return Ok(client);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to create client", ex);
                return BadRequest(ex.ToString());
            }
        }

        /// <summary>
        /// Updates the relevant client as indicated by the id parameter.
        /// </summary>
        /// <param name="model" type="ClientViewmodel">The model containing the new client details.</param>
        /// <param name="id" type="int">The ID value of the client to be edited.</param>
        [HttpPost("edit")]
        public async Task<IActionResult> Edit(ClientViewmodel model, int id)
        {
            try
            {
                var company = await context.Companies.FindAsync(model.CompanyId);
                if (company == null)
                {
                    return NotFound(new { message = $"Company with ID {model.CompanyId} was not found." });
                }

                var client = await context.Clients.FindAsync(id);
                if (client == null)
                {
                    return NotFound(new { message = $"Client with ID {id} was not found." });
                }
                client.Address = model.Address;
                client.Company = company;
                client.IdNumber = model.IdNumber;
                client.Name = model.Name;

                await context.SaveChangesAsync();

                return Ok(client);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to update client", ex);
                return BadRequest(ex.ToString());
            }
        }

        /// <summary>
        /// Deletes the selected client.
        /// NOTE: This action is irreversible
        /// </summary>
        /// <param name="id" type="int">The ID value of the client to be deleted.</param>
        [HttpPost("delete")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var client = await context.Clients.FindAsync(id);
                if (client == null)
                {
                    return NotFound(new { message = $"Client with ID {id} was not found." });
                }

                context.Clients.Remove(client);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to delete client", ex);
                return BadRequest(ex.ToString());
            }
        }
    }
}
