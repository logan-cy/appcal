using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ac.api.Viewmodels
{
    public class ProductViewmodel
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        [Required(ErrorMessage = "The Division ID field is required.")]
        public int DivisionId { get; set; }
        [Required(ErrorMessage = "The Product Name field is required.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "The Product Price field is required.")]
        public decimal Price { get; set; }
    }

    public class DivisionViewmodel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "The Company ID field is required.")]
        public int CompanyId { get; set; }
        [Required(ErrorMessage = "The Division Name field is required.")]
        public string Name { get; set; }
    }

    public class CompanyViewmodel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "The Company Name field is required.")]
        public string Name { get; set; }
        public string Address { get; set; }
    }

    public class ClientViewmodel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "The Company ID field is required.")]
        public int CompanyId { get; set; }
        [Required(ErrorMessage = "The Name field is required.")]
        public string Name { get; set; }
        public string Address { get; set; }
        [Required(ErrorMessage = "The Email field is required.")]
        public string IdNumber { get; set; }
        public IdentityUser User { get; set; }
    }

    public class LoginViewmodel
    {
        [Required(ErrorMessage = "The username field is required.")]
        public string Username { get; set; }
        [Required(ErrorMessage = "The password field is required.")]
        public string Password { get; set; }

        public bool Remember { get; set; }
    }
}