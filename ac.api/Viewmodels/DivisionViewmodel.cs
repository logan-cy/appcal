using System.ComponentModel.DataAnnotations;

namespace ac.api.Viewmodels
{
    public class DivisionViewmodel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "The Company ID field is required.")]
        public int CompanyId { get; set; }
        public CompanyViewmodel Company { get; set; }
        [Required(ErrorMessage = "The Division Name field is required.")]
        public string Name { get; set; }
    }
}