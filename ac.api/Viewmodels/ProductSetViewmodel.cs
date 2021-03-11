using System.Collections.Generic;

namespace ac.api.Viewmodels
{
    public class ProductSetViewmodel
    {
        public int Id { get; set; }
        public int DivisionId { get; set; }
        public string Name { get; set; }
        public List<ProductViewmodel> Products { get; set; }
    }
}