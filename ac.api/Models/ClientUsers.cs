using Microsoft.AspNetCore.Identity;

namespace ac.api.Models
{
    public class ClientUsers
    {
        public int Id { get; set; }
        public virtual Client Client { get; set; }
        public virtual IdentityUser User { get; set; }
    }
}
