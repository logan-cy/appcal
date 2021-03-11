using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ac.api.Models;

namespace ac.api.Viewmodels
{
    public class ProductViewmodel
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public CompanyViewmodel Company { get; set; }
        [Required(ErrorMessage = "The Division ID field is required.")]
        public int DivisionId { get; set; }
        public DivisionViewmodel Division { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}