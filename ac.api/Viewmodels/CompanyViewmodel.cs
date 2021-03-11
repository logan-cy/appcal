using System.ComponentModel.DataAnnotations;

namespace ac.api.Viewmodels
{
    public class CompanyViewmodel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "The Company Name field is required.")]
        [Display(Name = "Company Name")]
        public string Name { get; set; }
        public string Address { get; set; }
    }
}