using System.ComponentModel.DataAnnotations;
using LibraryUVSucre.Models;

namespace LibraryUVSucre.ViewModels
{
    // Usado por el Bibliotecario/Administrador para registrar un préstamo directamente.
    public class PrestamoRegistroViewModel
    {
        [Required]
        [Display(Name = "Libro")]
        public int LibroId { get; set; }

        [Required]
        [Display(Name = "Usuario")]
        public string UsuarioId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Fecha de devolución esperada")]
        [DataType(DataType.Date)]
        public DateTime FechaDevolucionEsperada { get; set; } = DateTime.Now.AddDays(7);

        public List<Libro> LibrosDisponibles { get; set; } = new();
        public List<ApplicationUser> UsuariosDisponibles { get; set; } = new();
    }
}
