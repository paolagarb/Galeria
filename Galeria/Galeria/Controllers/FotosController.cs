using Galeria.Data;
using Galeria.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        [Route("fotos")]
        // GET: Fotos
        public async Task<IActionResult> Index()
        {
            try
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
            catch (Exception)
            {
                return RedirectToAction(nameof(Error), new { message = "Ocorreu um erro ao carregar as fotos" });
            }
        }

        [Route("fotos/detalhes")]
        // GET: Fotos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction(nameof(Error), new { message = "Ocorreu um erro ao carregar detalhaes da foto" });
            }

            var user = User.Identity.Name;

            var foto = await _context.Fotos
                .Include(f => f.Album)
                .Include(f => f.IdentityUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (foto == null)
            {
                return RedirectToAction(nameof(Error), new { message = "Ocorreu um erro ao carregar detalhaes da foto" });
            }

            return View(foto);
        }

        [Route("fotos/nova")]
        // POST: Fotos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile foto, string legenda, string descricao, int album, bool capa)
        {
            try
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
            catch (Exception)
            {
                return RedirectToAction(nameof(Error), new { message = "Ocorreu um erro adicionar a foto" });
            }
        }

        [Route("fotos/editar")]
        // GET: Fotos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction(nameof(Error), new { message = "Ocorreu um erro editar a foto" });
            }

            try
            {
                var user = User.Identity.Name;

                var foto = (from c in _context.Fotos
                            where c.Id.Equals(id)
                            select c).FirstOrDefault();

                ViewBag.Descricao = (_context.Fotos
                                    .Where(c => c.Id.Equals(id))
                                    .Select(c => c.Descricao))
                                    .FirstOrDefault();

                ViewBag.Legenda = (_context.Fotos
                                    .Where(c => c.Id.Equals(id))
                                    .Select(c => c.Legenda))
                                    .FirstOrDefault();

                int albumAtualId = (from c in _context.Albuns
                                    join fotos in _context.Fotos
                                    on c.Id equals fotos.AlbumId
                                    where fotos.Id.Equals(id)
                                    select c.Id).FirstOrDefault();

                ViewBag.Album = from c in _context.Albuns
                                where c.IdentityUser.UserName == user
                                select new SelectListItem
                                {
                                    Selected = (c.Id == albumAtualId),
                                    Text = (c.Nome),
                                    Value = (c.Id.ToString()),
                                };

                ViewBag.Capa = (from c in _context.Fotos
                                where c.Id.Equals(id)
                                select c.Capa).FirstOrDefault();

                return View(foto);
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(Error), new { message = "Ocorreu um erro editar a foto" });
            }
        }

        [Route("fotos/editar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string descricao, string legenda, int album, bool capa)
        {
            try
            {
                var foto = (from c in _context.Fotos
                            where c.Id.Equals(id)
                            select c).FirstOrDefault();

                if (foto.Descricao != descricao)
                {
                    foto.Descricao = descricao;
                }

                if (foto.Legenda != legenda)
                {
                    foto.Legenda = legenda;
                }

                if (foto.AlbumId != album)
                {
                    foto.AlbumId = album;
                }

                if (foto.Capa != capa)
                {
                    if (capa == true)
                    {
                        var capaAtual = (from c in _context.Fotos
                                         where c.AlbumId == album &
                                         c.Capa == true
                                         select c).FirstOrDefault();
                        if (capaAtual != null)
                        {
                            capaAtual.Capa = false;
                            _context.Fotos.Update(capaAtual);
                            await _context.SaveChangesAsync();
                        }

                        foto.Capa = true;
                    }

                    foto.Capa = capa;
                }

                _context.Fotos.Update(foto);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(Error), new { message = "Ocorreu um erro editar a foto" });

            }
        }

        [Route("fotos/deletar")]
        // GET: Fotos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction(nameof(Error), new { message = "Ocorreu um erro deletar a foto" });
            }

            var foto = await _context.Fotos
                .Include(f => f.Album)
                .Include(f => f.IdentityUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (foto == null)
            {
                return RedirectToAction(nameof(Error), new { message = "Ocorreu um erro deletar a foto" });
            }

            return View(foto);
        }

        [Route("fotos/deletar")]
        // POST: Fotos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var foto = await _context.Fotos.FindAsync(id);
                _context.Fotos.Remove(foto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(Error), new { message = "Ocorreu um erro deletar a foto" });
            }
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

        public FileResult Download(int id)
        {
            var dados = (from c in _context.Fotos
                         where c.Id.Equals(id)
                         select c.Dados).FirstOrDefault();

            var contentType = (from c in _context.Fotos
                               where c.Id.Equals(id)
                               select c.ContentType).FirstOrDefault();

            string extensao = Path.GetFileNameWithoutExtension(contentType);
            return File(dados, contentType, "image." + extensao);
        }

        private bool FotoExists(int id)
        {
            return _context.Fotos.Any(e => e.Id == id);
        }
        public IActionResult Error(string message)
        {
            var viewModel = new ErrorViewModel
            {
                Message = message,
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };
            return View(viewModel);
        }
    }
}
