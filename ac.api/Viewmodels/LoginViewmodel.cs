using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ac.api.Viewmodels
{
    public class LoginViewmodel
    {
        [EmailAddress]
        [Required(ErrorMessage = "The username field is required.")]
        public string Username { get; set; }
        [Required(ErrorMessage = "The password field is required.")]
        public string Password { get; set; }

        public bool Remember { get; set; }
    }
    public class LoginResultViewmodel
    {
        public SignInResult Result { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }
    }
}