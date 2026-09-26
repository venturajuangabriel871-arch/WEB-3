using LibraryUVSucre.Data;
using LibraryUVSucre.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryUVSucre.Controllers
{
    [Authorize(Roles = "Bibliotecario")]
    public class BibliotecarioController : Controller
    {
        private readonly AppDbContext _context;

        public BibliotecarioController(AppDbContext context)
        {
            _context = context;
        }

        // Dashboard del Bibliotecario: libros y préstamos (no gestiona usuarios).
        public async Task<IActionResult> Dashboard()
        {
            ViewBag.TotalLibros = await _context.Libros.CountAsync();
            ViewBag.PrestamosPendientes = await _context.Prestamos.CountAsync(p => p.Estado == EstadoPrestamo.Solicitado);
            ViewBag.PrestamosActivos = await _context.Prestamos.CountAsync(p => p.Estado == EstadoPrestamo.Aprobado);
            ViewBag.SolicitudesRecientes = await _context.Prestamos
                .Include(p => p.Libro)
                .Include(p => p.Usuario)
                .Where(p => p.Estado == EstadoPrestamo.Solicitado)
                .OrderByDescending(p => p.FechaSolicitud)
                .Take(5)
                .ToListAsync();
            return View();
        }
    }
}
