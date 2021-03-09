using System.Collections.Generic;

namespace ac.api.Models
{
    public class ProductSet
    {
        public int Id { get; set; }
        public Division Division { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }
}