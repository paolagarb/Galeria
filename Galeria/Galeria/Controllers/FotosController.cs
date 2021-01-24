using Galeria.Data;
using Galeria.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Galeria.Controllers
{
    public class FotosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FotosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Fotos
        public async Task<IActionResult> Index()
        {
            //Usuário atual
            var usuario = User.Identity.Name;
            var usuarioId = (from user in _context.Usuarios
                             where user.UserName == usuario
                             select user.Id).Single();

            //Lista de álbuns para carregar em select list
            SelectList list = new SelectList(_context.Albuns
                                            .Where(c => c.IdentityUserId == usuarioId), "Id", "Nome");
            ViewBag.Albuns = list;

            //lista de fotos do usuário atual
            var fotos = (from c in _context.Fotos
                         where c.IdentityUserId.Equals(usuarioId)
                         select c.Id).ToList();
            ViewBag.Id = fotos;

            return View();
        }

        // GET: Fotos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var foto = await _context.Fotos
                .Include(f => f.Album)
                .Include(f => f.IdentityUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (foto == null)
            {
                return NotFound();
            }

            return View(foto);
        }

        // POST: Fotos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile foto, string legenda, string descricao, int album, bool capa)
        {
            if (ModelState.IsValid)
            {
                var usuario = User.Identity.Name;
                var usuarioId = (from user in _context.Usuarios
                                 where user.UserName == usuario
                                 select user.Id).Single();

                //Se nova foto for capa, validar se ja havia outra foto definida 
                //como capa no mesmo álbum para não dar conflito
                if (capa == true)
                {
                    Foto fotoCapaAtual = (from c in _context.Fotos
                                     join albuns in _context.Albuns
                                     on c.AlbumId equals albuns.Id
                                     where albuns.Id.Equals(album) &
                                     c.Capa == true
                                     select c).FirstOrDefault();

                    if (fotoCapaAtual != null)
                    {
                        fotoCapaAtual.Capa = false;
                        _context.Fotos.Update(fotoCapaAtual);
                        await _context.SaveChangesAsync();
                    }
                }

                if (foto != null)
                {
                    MemoryStream memoryStream = new MemoryStream();
                    foto.OpenReadStream().CopyTo(memoryStream);

                    Foto novaFoto = new Foto()
                    {
                        Descricao = descricao,
                        Legenda = legenda,
                        Dados = memoryStream.ToArray(),
                        ContentType = foto.ContentType,
                        AlbumId = album,
                        Capa = capa,
                        IdentityUserId = usuarioId
                    };

                    _context.Fotos.Add(novaFoto);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Fotos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var foto = await _context.Fotos.FindAsync(id);
            if (foto == null)
            {
                return NotFound();
            }
            ViewData["AlbumId"] = new SelectList(_context.Albuns, "Id", "Id", foto.AlbumId);
            ViewData["IdentityUserId"] = new SelectList(_context.Usuarios, "Id", "Id", foto.IdentityUserId);
            return View(foto);
        }

        // POST: Fotos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Descricao,Legenda,Dados,ContentType,Capa,AlbumId,IdentityUserId")] Foto foto)
        {
            if (id != foto.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(foto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FotoExists(foto.Id))
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
            ViewData["AlbumId"] = new SelectList(_context.Albuns, "Id", "Id", foto.AlbumId);
            ViewData["IdentityUserId"] = new SelectList(_context.Usuarios, "Id", "Id", foto.IdentityUserId);
            return View(foto);
        }

        // GET: Fotos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var foto = await _context.Fotos
                .Include(f => f.Album)
                .Include(f => f.IdentityUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (foto == null)
            {
                return NotFound();
            }

            return View(foto);
        }

        // POST: Fotos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var foto = await _context.Fotos.FindAsync(id);
            _context.Fotos.Remove(foto);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public FileStreamResult Imagem(int id)
        {
            var usuario = User.Identity.Name;
            var usuarioId = (from user in _context.Usuarios
                             where user.UserName == usuario
                             select user.Id).Single();

            var dados = (from c in _context.Fotos
                         where c.Id.Equals(id)
                         select c.Dados).FirstOrDefault();

            var contentType = (from c in _context.Fotos
                               where c.Id.Equals(id)
                               select c.ContentType).FirstOrDefault();

            MemoryStream memoryStream = new MemoryStream(dados);
            return new FileStreamResult(memoryStream, contentType);
        }

        public FileResult Download (int id)
        {
            var dados = (from c in _context.Fotos
                         where c.Id.Equals(id)
                         select c.Dados).FirstOrDefault();

            var contentType = (from c in _context.Fotos
                         where c.Id.Equals(id)
                         select c.ContentType).FirstOrDefault();

            string extensao = Path.GetFileNameWithoutExtension(contentType);
            return File(dados, contentType, "image."+extensao);
        }

        private bool FotoExists(int id)
        {
            return _context.Fotos.Any(e => e.Id == id);
        }
    }
}
