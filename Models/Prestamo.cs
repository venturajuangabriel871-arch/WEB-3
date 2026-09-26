using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryUVSucre.Models
{
    public class Prestamo
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Libro")]
        public int LibroId { get; set; }

        [ForeignKey(nameof(LibroId))]
        public Libro? Libro { get; set; }

        [Required]
        [Display(Name = "Usuario")]
        public string UsuarioId { get; set; } = string.Empty;

        [ForeignKey(nameof(UsuarioId))]
        public ApplicationUser? Usuario { get; set; }

        [Display(Name = "Fecha de solicitud")]
        public DateTime FechaSolicitud { get; set; } = DateTime.Now;

        [Display(Name = "Fecha de préstamo")]
        public DateTime? FechaPrestamo { get; set; }

        [Display(Name = "Fecha de devolución esperada")]
        public DateTime? FechaDevolucionEsperada { get; set; }

        [Display(Name = "Fecha de devolución real")]
        public DateTime? FechaDevolucionReal { get; set; }

        [Display(Name = "Estado")]
        public EstadoPrestamo Estado { get; set; } = EstadoPrestamo.Solicitado;
    }
}
