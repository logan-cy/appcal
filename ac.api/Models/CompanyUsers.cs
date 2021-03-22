using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ac.api.Models
{
    public class CompanyUsers
    {
        public int Id { get; set; }
        public virtual Company Company { get; set; }
        public virtual IdentityUser User { get; set; }
    }
}
