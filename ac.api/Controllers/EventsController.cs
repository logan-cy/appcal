using System;
using System.Linq;
using System.Threading.Tasks;
using ac.api.Data;
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

                var events = await context.Events.Where(x => x.Company.Id == companyId).ToListAsync();

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
    }
}