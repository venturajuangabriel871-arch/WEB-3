using LibraryUVSucre.Data;
using LibraryUVSucre.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryUVSucre.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdministradorController : Controller
    {
        private readonly AppDbContext _context;

        public AdministradorController(AppDbContext context)
        {
            _context = context;
        }

        // Dashboard principal del Administrador: libros, usuarios y préstamos.
        public async Task<IActionResult> Dashboard()
        {
            ViewBag.TotalLibros = await _context.Libros.CountAsync();
            ViewBag.TotalUsuarios = await _context.Users.CountAsync();
            ViewBag.TotalPrestamos = await _context.Prestamos.CountAsync();
            ViewBag.PrestamosPendientes = await _context.Prestamos.CountAsync(p => p.Estado == EstadoPrestamo.Solicitado);
            ViewBag.UltimosPrestamos = await _context.Prestamos
                .Include(p => p.Libro)
                .Include(p => p.Usuario)
                .OrderByDescending(p => p.FechaSolicitud)
                .Take(5)
                .ToListAsync();
            return View();
        }
    }
}
