using System.ComponentModel.DataAnnotations;

namespace ac.api.Viewmodels
{
    public class PasswordResetViewmodel
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string Code { get; set; }

        public string Password { get; set; }
    }
}