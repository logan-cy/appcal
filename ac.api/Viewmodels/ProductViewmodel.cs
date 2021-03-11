using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ac.api.Models;

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
}