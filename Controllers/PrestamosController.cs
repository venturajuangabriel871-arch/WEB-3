using LibraryUVSucre.Data;
using LibraryUVSucre.Models;
using LibraryUVSucre.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryUVSucre.Controllers
{
    // Administrador y Bibliotecario gestionan todos los préstamos: aprobar, rechazar y registrar devoluciones.
    [Authorize(Roles = "Administrador,Bibliotecario")]
    public class PrestamosController : Controller
    {
        private readonly AppDbContext _context;

        public PrestamosController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(EstadoPrestamo? estado)
        {
            var query = _context.Prestamos.Include(p => p.Libro).Include(p => p.Usuario).AsQueryable();
            if (estado.HasValue)
            {
                query = query.Where(p => p.Estado == estado);
            }
            ViewBag.EstadoFiltro = estado;
            return View(await query.OrderByDescending(p => p.FechaSolicitud).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var prestamo = await _context.Prestamos
                .Include(p => p.Libro)
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (prestamo == null) return NotFound();
            return View(prestamo);
        }

        // El bibliotecario/administrador también puede registrar un préstamo directamente (ya aprobado).
        public async Task<IActionResult> Create()
        {
            var modelo = new PrestamoRegistroViewModel
            {
                LibrosDisponibles = await _context.Libros.Where(l => l.CantidadDisponible > 0).OrderBy(l => l.Titulo).ToListAsync(),
                UsuariosDisponibles = await _context.Users.OrderBy(u => u.Nombres).ToListAsync()
            };
            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PrestamoRegistroViewModel modelo)
        {
            var libro = await _context.Libros.FindAsync(modelo.LibroId);
            if (libro == null || libro.CantidadDisponible <= 0)
            {
                ModelState.AddModelError(string.Empty, "El libro seleccionado no tiene ejemplares disponibles.");
            }

            if (!ModelState.IsValid)
            {
                modelo.LibrosDisponibles = await _context.Libros.Where(l => l.CantidadDisponible > 0).OrderBy(l => l.Titulo).ToListAsync();
                modelo.UsuariosDisponibles = await _context.Users.OrderBy(u => u.Nombres).ToListAsync();
                return View(modelo);
            }

            _context.Prestamos.Add(new Prestamo
            {
                LibroId = modelo.LibroId,
                UsuarioId = modelo.UsuarioId,
                FechaSolicitud = DateTime.Now,
                FechaPrestamo = DateTime.Now,
                FechaDevolucionEsperada = modelo.FechaDevolucionEsperada,
                Estado = EstadoPrestamo.Aprobado
            });
            libro!.CantidadDisponible -= 1;
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Préstamo registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // Aprueba una solicitud hecha por un Usuario desde el catálogo.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aprobar(int id)
        {
            var prestamo = await _context.Prestamos.Include(p => p.Libro).FirstOrDefaultAsync(p => p.Id == id);
            if (prestamo == null) return NotFound();

            if (prestamo.Libro!.CantidadDisponible <= 0)
            {
                TempData["Error"] = "No hay ejemplares disponibles para aprobar este préstamo.";
                return RedirectToAction(nameof(Index));
            }

            prestamo.Estado = EstadoPrestamo.Aprobado;
            prestamo.FechaPrestamo = DateTime.Now;
            prestamo.FechaDevolucionEsperada ??= DateTime.Now.AddDays(7);
            prestamo.Libro.CantidadDisponible -= 1;
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Préstamo aprobado.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rechazar(int id)
        {
            var prestamo = await _context.Prestamos.FindAsync(id);
            if (prestamo == null) return NotFound();
            prestamo.Estado = EstadoPrestamo.Rechazado;
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Solicitud rechazada.";
            return RedirectToAction(nameof(Index));
        }

        // Registra la devolución física del libro.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Devolver(int id)
        {
            var prestamo = await _context.Prestamos.Include(p => p.Libro).FirstOrDefaultAsync(p => p.Id == id);
            if (prestamo == null) return NotFound();

            prestamo.Estado = EstadoPrestamo.Devuelto;
            prestamo.FechaDevolucionReal = DateTime.Now;
            prestamo.Libro!.CantidadDisponible += 1;
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Devolución registrada.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var prestamo = await _context.Prestamos.Include(p => p.Libro).Include(p => p.Usuario).FirstOrDefaultAsync(p => p.Id == id);
            if (prestamo == null) return NotFound();
            return View(prestamo);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var prestamo = await _context.Prestamos.FindAsync(id);
            if (prestamo != null)
            {
                _context.Prestamos.Remove(prestamo);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Registro de préstamo eliminado.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
