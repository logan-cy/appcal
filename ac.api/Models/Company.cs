using System.Security.AccessControl;
using System;
using System.ComponentModel.DataAnnotations;

namespace ac.api.Models
{
    public class Company
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "The Company Name field is required.")]
        public string Name { get; set; }
        public string Address { get; set; }
    }
}