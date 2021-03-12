using System.Reflection.PortableExecutable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ac.api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace ac.app.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IHttpContextAccessor httpContextAccessor;

        public List<CalendarEvent> events { get; set; }

        public string Token { get; private set; }
        public string Username { get; private set; }

        public IndexModel(ILogger<IndexModel> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            this.httpContextAccessor = httpContextAccessor;
        }

        public void OnGet()
        {
            Token = httpContextAccessor.HttpContext.Request.Cookies["Token"];
            Username = httpContextAccessor.HttpContext.Request.Cookies["Username"];
        }
    }
}
