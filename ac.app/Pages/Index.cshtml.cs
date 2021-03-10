using System.Reflection.PortableExecutable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ac.api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ac.app.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public List<Event> events { get; set; }

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            try
            {
                var events = new List<Event>
                {
                    new Event{
                        Id = 1,
                        Company = new Company{
                            Id = 1,
                            Name = "Fitwise",
                            Address = "4490 Sampson Street, Kiowa, CO, 80117"
                        },
                        Start = new DateTime(2021, 3, 11, 13, 0, 0),
                        End = new DateTime(2021, 3, 11, 13, 30, 0),
                        Title = "Lunch appointment",
                        Description = "This is just a test event to show on the calendar"
                    }
                };

                this.events = events;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
