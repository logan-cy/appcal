using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ac.api.Viewmodels
{
    public class ClientViewmodel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "The Company ID field is required.")]
        public int CompanyId { get; set; }
        public CompanyViewmodel Company { get; set; }
        [Required(ErrorMessage = "The Name field is required.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "The Email field is required.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "The Phone number field is required.")]
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        [Required(ErrorMessage = "The ID Number field is required.")]
        public string IdNumber { get; set; }
        public IdentityUser User { get; set; }
    }
}