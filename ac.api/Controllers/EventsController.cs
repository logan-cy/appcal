using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ac.api.Data;
using ac.api.Models;
using ac.api.Viewmodels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ac.api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly ILogger<EventsController> _logger;
        private readonly IConfiguration config;
        public EventsController(ILogger<EventsController> logger, ApplicationDbContext context, IConfiguration config)
        {
            this.config = config;
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


                var events = context.Events
                    .Include(x => x.Company)
                    .Include(x => x.Client).AsQueryable();

                if (companyId >= 1)
                {
                    events = events.Where(x => x.Company.Id == companyId);
                }

                var result = await events.Select(x => new EventViewmodel
                {
                    AllDay = x.AllDay,
                    ClientId = x.Client.Id,
                    CompanyId = x.Company.Id,
                    Description = x.Description,
                    End = x.End,
                    Id = x.Id,
                    Start = x.Start,
                    Title = x.Title,
                    Url = x.Url
                }).ToListAsync();

                return Ok(result);
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
        [HttpGet("single")]
        public async Task<IActionResult> Single(int id)
        {
            try
            {
                var ev = await context.Events
                    .Include(x => x.Company)
                    .Include(x => x.Client)
                    .SingleOrDefaultAsync(x => x.Id == id);

                if (ev == null)
                {
                    return NotFound(new { message = $"Event with ID {id} was not found." });
                }

                var appointment = new EventViewmodel
                {
                    AllDay = ev.AllDay,
                    ClientId = ev.Client.Id,
                    CompanyId = ev.Company.Id,
                    Description = ev.Description,
                    End = ev.End,
                    Id = ev.Id,
                    Start = ev.Start,
                    Title = ev.Title,
                    Url = ev.Url
                };

                return Ok(appointment);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to get event", ex);
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
                    AllDay = model.AllDay,
                    Client = client,
                    Company = company,
                    Description = model.Description,
                    End = model.End,
                    Start = model.Start,
                    Title = model.Title
                };

                await context.Events.AddAsync(ev);
                await context.SaveChangesAsync();

                // Once event is saved, set an edit URL where it can be updated.
                ev.Url = $"Edit?id={ev.Id}";
                await context.SaveChangesAsync();

                return Ok(ev);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to create event.", ex);
                return BadRequest(ex.ToString());
            }
        }

        [HttpPost("edit")]
        public async Task<IActionResult> Edit(EventViewmodel model)
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
                var ev = await context.Events.FindAsync(model.Id);
                ev.AllDay = (model.End == DateTime.MinValue || model.End == model.Start.AddDays(1));
                ev.Client = client;
                ev.Company = company;
                ev.Description = model.Description;
                ev.End = model.End;
                ev.Start = model.Start;
                ev.Title = model.Title;

                await context.SaveChangesAsync();

                return Ok(ev);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to update event.", ex);
                return BadRequest(ex.ToString());
            }
        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var ev = await context.Events.FindAsync(id);
                context.Events.Remove(ev);
                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to delete event.", ex);
                return BadRequest(ex.ToString());
            }
        }
    }
}