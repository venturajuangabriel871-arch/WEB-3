using LibraryUVSucre.Data;
using LibraryUVSucre.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryUVSucre.Controllers
{
    [Authorize(Roles = "Usuario")]
    public class UsuarioController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsuarioController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Dashboard del Usuario final: resumen de sus propios préstamos.
        public async Task<IActionResult> Dashboard()
        {
            var usuarioId = _userManager.GetUserId(User);
            ViewBag.TotalLibrosCatalogo = await _context.Libros.CountAsync();
            ViewBag.MisPrestamosActivos = await _context.Prestamos
                .CountAsync(p => p.UsuarioId == usuarioId && p.Estado == EstadoPrestamo.Aprobado);
            ViewBag.MisSolicitudesPendientes = await _context.Prestamos
                .CountAsync(p => p.UsuarioId == usuarioId && p.Estado == EstadoPrestamo.Solicitado);
            ViewBag.MisPrestamosRecientes = await _context.Prestamos
                .Include(p => p.Libro)
                .Where(p => p.UsuarioId == usuarioId)
                .OrderByDescending(p => p.FechaSolicitud)
                .Take(5)
                .ToListAsync();
            return View();
        }

        // Catálogo de libros disponible para consulta (solo lectura).
        public async Task<IActionResult> Catalogo(string? busqueda)
        {
            var query = _context.Libros.AsQueryable();
            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                query = query.Where(l => l.Titulo.Contains(busqueda) || l.Autor.Contains(busqueda) || l.Categoria.Contains(busqueda));
            }
            ViewBag.Busqueda = busqueda;
            return View(await query.OrderBy(l => l.Titulo).ToListAsync());
        }

        // El usuario solicita un préstamo desde el catálogo.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Solicitar(int libroId)
        {
            var libro = await _context.Libros.FindAsync(libroId);
            if (libro == null || libro.CantidadDisponible <= 0)
            {
                TempData["Error"] = "El libro no está disponible actualmente.";
                return RedirectToAction(nameof(Catalogo));
            }

            var usuarioId = _userManager.GetUserId(User)!;
            _context.Prestamos.Add(new Prestamo
            {
                LibroId = libroId,
                UsuarioId = usuarioId,
                FechaSolicitud = DateTime.Now,
                Estado = EstadoPrestamo.Solicitado
            });
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Solicitud de préstamo enviada. Espera la aprobación del bibliotecario.";
            return RedirectToAction(nameof(MisPrestamos));
        }

        // Listado de préstamos que el propio usuario solicitó.
        public async Task<IActionResult> MisPrestamos()
        {
            var usuarioId = _userManager.GetUserId(User);
            var prestamos = await _context.Prestamos
                .Include(p => p.Libro)
                .Where(p => p.UsuarioId == usuarioId)
                .OrderByDescending(p => p.FechaSolicitud)
                .ToListAsync();
            return View(prestamos);
        }
    }
}
