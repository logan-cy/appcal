using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ac.app.Enums
{
    public enum LoginFields
    {
        [Display(Name = "Roles")]
        Roles,
        [Display(Name = "Token")]
        Token,
        [Display(Name = "Username")]
        Username,
        [Display(Name = "UserType")]
        UserType,
        [Display(Name = "UserTypeId")]
        UserTypeId
    }
}
