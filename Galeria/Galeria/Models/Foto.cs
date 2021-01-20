using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Galeria.Models
{
    public class Foto
    {
        public int Id { get; set; }
        [Display(Name="Descrição")]   
        public string Descricao { get; set; }
        public string Legenda { get; set; }
        public byte[] Dados { get; set; }
        public string ContentType { get; set; }
        public bool Capa { get; set; }
        public virtual Album Album { get; set; }
        [Display(Name = "Álbum")]
        public int AlbumId { get; set; }
        public virtual IdentityUser IdentityUser { get; set; }
        public string IdentityUserId { get; set; }
        public Foto()
        {

        }
    }
}
