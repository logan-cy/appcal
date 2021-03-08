using System.Security.AccessControl;
using System;
using System.ComponentModel.DataAnnotations;

namespace ac.api.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "The Division field is required.")]
        public Division Division { get; set; }
        [Required(ErrorMessage = "The Product Name field is required.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "The Price field is required.")]
        public decimal Price { get; set; }
    }
}