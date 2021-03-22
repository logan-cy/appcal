using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ac.api.Constants
{
    public enum SystemRoles
    {
        [Display(Name = "Admin")]
        Admin,
        [Display(Name = "Company")]
        Company,
        [Display(Name = "Client")]
        Client
    }
}
