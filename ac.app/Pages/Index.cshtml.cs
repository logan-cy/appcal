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
using ac.api.Viewmodels;
using System.Text.Json;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Rendering;
using ac.api.Constants;
using System.Text;
using ac.api.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ac.app.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ApplicationDbContext context;

        [BindProperty]
        public int CompanyId { get; set; }
        public IEnumerable<SelectListItem> Companies { get; set; }
        [BindProperty]
        public IEnumerable<EventViewmodel> Events { get; set; }
        [BindProperty]
        public bool IsAdmin { get; set; }

        public string Token { get; private set; }
        public string Username { get; private set; }

        public IndexModel(ILogger<IndexModel> logger, ApplicationDbContext context)
        {
            _logger = logger;
            this.context = context;
        }

        public async Task<IActionResult> OnGet()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Redirect("~/Account/Login");
            }

            var companyId = 0;
            var isAdmin = User.IsInRole(nameof(SystemRoles.Admin));
            var isClient = User.IsInRole(nameof(SystemRoles.Client));
            var isCompany = User.IsInRole(nameof(SystemRoles.Company));
            if (isCompany)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var companyUser = context.CompanyUsers.Include(x => x.Company).Include(x => x.User).First(x => x.User.Id == userId);
                companyId = companyUser.Company.Id;
            }

            if (!isClient)
            {
                Events = await GetEventsAsync(companyId);
                if (isAdmin)
                {
                    Companies = await GetCompaniesAsync();
                }
            }
            return Page();
        }

        public async Task<IActionResult> OnPostGetEventsAsync(int id)
        {
            try
            {
                var events = context.Events
                    .Include(x => x.Company)
                    .Include(x => x.Client)
                    .Include(x => x.Product).AsQueryable();

                if (id >= 1)
                {
                    events = events.Where(x => x.Company.Id == id);
                }

                var result = await events.Select(x => new EventViewmodel
                {
                    AllDay = x.AllDay,
                    ClientId = x.Client.Id,
                    CompanyId = x.Company.Id,
                    Description = x.Description,
                    End = x.End,
                    Id = x.Id,
                    ProductId = x.Product.Id,
                    Start = x.Start,
                    Title = x.Title,
                    Url = x.Url
                }).ToListAsync();

                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to get appointments: {ex}", ex);
                var evs = Array.Empty<EventViewmodel>();

                return new JsonResult(evs);
            }
        }

        private async Task<IEnumerable<EventViewmodel>> GetEventsAsync(int companyId = 0)
        {
            var events = context.Events
                .Include(x => x.Company)
                .Include(x => x.Client)
                .Include(x => x.Product).AsQueryable();

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
                ProductId = x.Product.Id,
                Start = x.Start,
                Title = x.Title,
                Url = x.Url
            }).ToListAsync();

            return result;
        }

        private async Task<IEnumerable<SelectListItem>> GetCompaniesAsync()
        {
            var companies = await context.Companies.Select(x => new CompanyViewmodel
            {
                Address = x.Address,
                Id = x.Id,
                Name = x.Name
            }).ToListAsync();

            return companies.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });
        }
    }
}
