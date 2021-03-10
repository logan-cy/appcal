using System;
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
    public class EventsController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly ILogger<EventsController> _logger;
        public EventsController(ILogger<EventsController> logger, ApplicationDbContext context)
        {
            this._logger = logger;
            this.context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int companyId)
        {
            try
            {
                var company = await context.Companies.FindAsync(companyId);
                if (company == null)
                {
                    return NotFound(new { message = $"Company with ID {companyId} was not found." });
                }

                var events = await context.Events
                    .Include(x => x.Company)
                    .Include(x => x.Client)
                    .Where(x => x.Company.Id == companyId)
                    .Select(x => new EventViewmodel
                    {
                        ClientId = x.Client.Id,
                        CompanyId = x.Company.Id,
                        Description = x.Description,
                        End = x.End,
                        Id = x.Id,
                        Start = x.Start,
                        Title = x.Title
                    }).ToListAsync();

                return Ok(events);
            }
            catch (Exception ex)
            {
                var company = await context.Companies.FindAsync(companyId);
                if (company == null)
                {
                    return NotFound(new { message = $"Company with ID {companyId} was not found." });
                }
                _logger.LogError($"Unable to get events for company '{company.Name}'", ex);
                return BadRequest(ex.ToString());
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(EventViewmodel model)
        {
            try
            {
                var company = await context.Companies.FindAsync(model.CompanyId);
                if (company == null)
                {
                    return NotFound(new { message = $"Company with ID {model.CompanyId} was not found." });
                }
                var client = await context.Clients.FindAsync(model.ClientId);
                if (client == null)
                {
                    return NotFound(new { message = $"Client with ID {model.ClientId} was not found." });
                }
                var ev = new CalendarEvent
                {
                    Client = client,
                    Company = company,
                    Description = model.Description,
                    End = model.End,
                    Start = model.Start,
                    Title = model.Title
                };

                await context.Events.AddAsync(ev);
                await context.SaveChangesAsync();

                return Ok(ev);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to create event.", ex);
                return BadRequest(ex.ToString());
            }
        }
    }
}