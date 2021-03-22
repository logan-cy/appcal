using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ac.api.Data;
using ac.api.Models;
using ac.api.Viewmodels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ac.app.Pages.Companies
{
    public class CreateModel : PageModel
    {
        [BindProperty]
        public CompanyViewmodel Company { get; set; }

        public bool SaveCompanyError { get; private set; }
        public string SaveCompanyErrorMessage { get; private set; }

        private readonly ILogger<CreateModel> _logger;
        private readonly ApplicationDbContext context;

        public CreateModel(ILogger<CreateModel> logger, ApplicationDbContext context)
        {
            _logger = logger;
            this.context = context;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var company = new Company
                {
                    Address = Company.Address,
                    Name = Company.Name
                };
                await context.Companies.AddAsync(company);
                await context.SaveChangesAsync();

                return Redirect("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to save company: {ex}", ex);
                SaveCompanyErrorMessage = ex.ToString();
                SaveCompanyError = true;

                return Page();
            }
        }
    }
}
