using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ac.api.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsCompany { get; set; }
    }
}
