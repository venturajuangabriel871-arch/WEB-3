using System.ComponentModel.DataAnnotations;

namespace LibraryUVSucre.ViewModels
{
    // Se usa tanto para crear como para editar cuentas desde el panel del Administrador.
    public class UsuarioFormViewModel
    {
        public string? Id { get; set; }

        [Required]
        [Display(Name = "Nombres")]
        public string Nombres { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Apellidos")]
        public string Apellidos { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = string.Empty;

        // Solo se exige al crear un nuevo usuario.
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string? Password { get; set; }

        [Required]
        [Display(Name = "Rol")]
        public string Rol { get; set; } = "Usuario";

        public List<string> RolesDisponibles { get; set; } = new() { "Administrador", "Bibliotecario", "Usuario" };
    }
}
