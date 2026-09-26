using LibraryUVSucre.Data;
using LibraryUVSucre.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryUVSucre.Controllers
{
    // Administrador y Bibliotecario gestionan el catálogo de libros (CRUD).
    [Authorize(Roles = "Administrador,Bibliotecario")]
    public class LibrosController : Controller
    {
        private readonly AppDbContext _context;

        public LibrosController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? busqueda)
        {
            var query = _context.Libros.AsQueryable();
            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                query = query.Where(l => l.Titulo.Contains(busqueda) || l.Autor.Contains(busqueda) || l.ISBN.Contains(busqueda));
            }
            ViewBag.Busqueda = busqueda;
            return View(await query.OrderBy(l => l.Titulo).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var libro = await _context.Libros.FirstOrDefaultAsync(l => l.Id == id);
            if (libro == null) return NotFound();
            return View(libro);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Titulo,Autor,ISBN,Editorial,Categoria,AnioPublicacion,CantidadTotal,CantidadDisponible")] Libro libro)
        {
            if (!ModelState.IsValid) return View(libro);
            _context.Add(libro);
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Libro registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var libro = await _context.Libros.FindAsync(id);
            if (libro == null) return NotFound();
            return View(libro);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titulo,Autor,ISBN,Editorial,Categoria,AnioPublicacion,CantidadTotal,CantidadDisponible")] Libro libro)
        {
            if (id != libro.Id) return NotFound();
            if (!ModelState.IsValid) return View(libro);

            try
            {
                _context.Update(libro);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Libro actualizado correctamente.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Libros.Any(e => e.Id == id)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var libro = await _context.Libros.FirstOrDefaultAsync(l => l.Id == id);
            if (libro == null) return NotFound();
            return View(libro);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var libro = await _context.Libros.FindAsync(id);
            if (libro != null)
            {
                var tienePrestamos = await _context.Prestamos.AnyAsync(p => p.LibroId == id);
                if (tienePrestamos)
                {
                    TempData["Error"] = "No se puede eliminar: el libro tiene préstamos asociados.";
                    return RedirectToAction(nameof(Index));
                }
                _context.Libros.Remove(libro);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Libro eliminado.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
