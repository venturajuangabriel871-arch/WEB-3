using Microsoft.AspNetCore.Identity;

namespace LibraryUVSucre.Models
{
    // Extiende el usuario de Identity con datos propios de la biblioteca.
    public class ApplicationUser : IdentityUser
    {
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;

        public string NombreCompleto => $"{Nombres} {Apellidos}".Trim();

        public ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
    }
}
