using Galeria.Data;
using Galeria.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Galeria.Controllers
{
    [Authorize]
    public class AlbunsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AlbunsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Route("albuns")]
        // GET: Albuns
        public async Task<IActionResult> Index()
        {
            var user = User.Identity.Name;

            var albuns = (from c in _context.Albuns
                          where c.IdentityUser.UserName == user
                          select c).ToList();

            List<Album> albumList = new List<Album>();
            List<bool> fotosList = new List<bool>();

            foreach (var album in albuns)
            {
                albumList.Add(album);

                var foto = FotoCapa(album.Id);

                if (foto == null)
                {
                    fotosList.Add(false);
                } else
                {
                    fotosList.Add(true);
                }
            }

            ViewBag.Album = albumList;
            ViewBag.Fotos = fotosList;

            return View();
        }

        [Route("albuns/detalhes")]
        // GET: Albuns/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var album = await _context.Albuns
                .Include(a => a.IdentityUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (album == null)
            {
                return NotFound();
            }

            return View(album);
        }

        [Route("albuns/editar")]
        // GET: Albuns/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var album = await _context.Albuns.FindAsync(id);
            if (album == null)
            {
                return NotFound();
            }
            return View(album);
        }

        [Route("albuns/editar")]
        // POST: Albuns/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,IdentityUserId")] Album album)
        {
            if (id != album.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(album);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlbumExists(album.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdentityUserId"] = new SelectList(_context.Usuarios, "Id", "Id", album.IdentityUserId);
            return View(album);
        }

        [Route("albuns/deletar")]
        // GET: Albuns/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var album = await _context.Albuns
                .Include(a => a.IdentityUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (album == null)
            {
                return NotFound();
            }

            return View(album);
        }

        [Route("albuns/deletar")]
        // POST: Albuns/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var fotos = (from c in _context.Fotos
                         join albuns in _context.Albuns
                         on c.AlbumId equals albuns.Id
                         where albuns.Id.Equals(id)
                         select c).ToList();
            foreach (var foto in fotos)
            {
                _context.Fotos.Remove(foto);
                await _context.SaveChangesAsync();
            }

            var album = await _context.Albuns.FindAsync(id);
            _context.Albuns.Remove(album);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Route("albuns/adicionar")]
        public async Task<IActionResult> NovoAlbum(string nome)
        {
            var usuario = User.Identity.Name;
            var userId = (from user in _context.Usuarios
                          where user.UserName == usuario
                          select user.Id).Single();

            if (!string.IsNullOrEmpty(nome))
            {
                Album album = new Album()
                {
                    Nome = nome,
                    IdentityUserId = userId
                };

                _context.Albuns.Add(album);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        public FileStreamResult FotoCapa(int id)
        {
            var user = User.Identity.Name;
            byte[] dados;
            string contentType;


            var fotoCapaId = (from c in _context.Albuns
                              join foto in _context.Fotos
                              on c.Id equals foto.AlbumId
                              where c.Id.Equals(id)
                              & foto.Capa == true
                              select foto.Id).FirstOrDefault();
          
            if (fotoCapaId != 0)
            {
                dados = (from c in _context.Fotos
                             join album in _context.Albuns
                             on c.AlbumId equals album.Id
                             where c.Id.Equals(fotoCapaId)
                             select c.Dados).FirstOrDefault();

                contentType = (from c in _context.Fotos
                                   join album in _context.Albuns
                                   on c.AlbumId equals album.Id
                                   where c.Id.Equals(fotoCapaId)
                                   select c.ContentType).FirstOrDefault();
            } else
            {
                //Caso não tenha uma foto definida como capa:
                var fotoId = (from c in _context.Albuns
                              join foto in _context.Fotos
                              on c.Id equals foto.AlbumId
                              where c.Id.Equals(id)
                              select foto.Id).FirstOrDefault();

                if (fotoId != 0)
                {
                    dados = (from c in _context.Fotos
                             join album in _context.Albuns
                             on c.AlbumId equals album.Id
                             where c.Id.Equals(fotoId)
                             select c.Dados).FirstOrDefault();

                    contentType = (from c in _context.Fotos
                                   join album in _context.Albuns
                                   on c.AlbumId equals album.Id
                                   where c.Id.Equals(fotoId)
                                   select c.ContentType).FirstOrDefault();
                }
                //Se o álbum não possuir fotos:
                else
                {
                    return null;
                }
            }

            MemoryStream memoryStream = new MemoryStream(dados);
            return new FileStreamResult(memoryStream, contentType);
        }

        [Route("albuns/fotos/{id}")]
        public async Task<IActionResult> Fotos(int id)
        {
            ViewBag.FotosId = await (from c in _context.Albuns
                           join fotos in _context.Fotos
                           on c.Id equals fotos.AlbumId
                           where c.Id.Equals(id)
                           select fotos.Id).ToListAsync();

            ViewBag.AlbumNome = await (from c in _context.Albuns
                                 where c.Id.Equals(id)
                                 select c.Nome).FirstOrDefaultAsync();

            ViewBag.AlbumId = await (from c in _context.Albuns
                               where c.Id.Equals(id)
                               select c.Id).FirstOrDefaultAsync();

            return View();
        }

        private bool AlbumExists(int id)
        {
            return _context.Albuns.Any(e => e.Id == id);
        }
    }
}
