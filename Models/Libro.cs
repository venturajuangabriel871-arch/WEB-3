using System.ComponentModel.DataAnnotations;

namespace LibraryUVSucre.Models
{
    public class Libro
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(200)]
        [Display(Name = "Título")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El autor es obligatorio")]
        [StringLength(150)]
        public string Autor { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "ISBN")]
        public string ISBN { get; set; } = string.Empty;

        [StringLength(100)]
        public string Editorial { get; set; } = string.Empty;

        [StringLength(80)]
        public string Categoria { get; set; } = string.Empty;

        [Display(Name = "Año de publicación")]
        [Range(1400, 2100)]
        public int AnioPublicacion { get; set; }

        [Required]
        [Range(0, 10000)]
        [Display(Name = "Cantidad total")]
        public int CantidadTotal { get; set; }

        [Required]
        [Range(0, 10000)]
        [Display(Name = "Ejemplares disponibles")]
        public int CantidadDisponible { get; set; }

        public ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
    }
}
