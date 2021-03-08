using System.Security.AccessControl;
using System;
using System.ComponentModel.DataAnnotations;

namespace ac.api.Models
{
    public class Division
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "The Company field is required.")]
        public Company Company { get; set; }
        [Required(ErrorMessage = "The Company Name field is required.")]
        public string Name { get; set; }
    }
}