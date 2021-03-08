using System.Security.AccessControl;
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ac.api.Models
{
    public class Client
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "The Company field is required.")]
        public Company Company { get; set; }
        [Required(ErrorMessage = "The Name field is required.")]
        public string Name { get; set; }
        public string Address { get; set; }
        [Required(ErrorMessage = "The Email field is required.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "The Phone Number field is required.")]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "The ID Number field is required.")]
        public string IdNumber { get; set; }
    }
}