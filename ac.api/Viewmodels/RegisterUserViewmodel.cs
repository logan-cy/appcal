using System.ComponentModel.DataAnnotations;

namespace ac.api.Viewmodels
{
    public class RegisterUserViewmodel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Password { get; set; }
    }
}