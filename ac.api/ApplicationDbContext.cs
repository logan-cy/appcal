using System;
using ac.api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ac.api.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public virtual DbSet<Client> Clients { get; set; }
        public virtual DbSet<Company> Companies { get; set; }
        public virtual DbSet<Division> Divisions { get; set; }
        public virtual DbSet<CalendarEvent> Events { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<ProductSet> ProductSets { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}