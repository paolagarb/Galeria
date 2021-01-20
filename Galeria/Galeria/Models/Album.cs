using Microsoft.AspNetCore.Identity;

namespace Galeria.Models
{
    public class Album
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public virtual IdentityUser IdentityUser { get; set; }
        public string IdentityUserId { get; set; }
        public Album()
        {

        }
    }
}
