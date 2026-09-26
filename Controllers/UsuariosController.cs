using LibraryUVSucre.Models;
using LibraryUVSucre.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LibraryUVSucre.Controllers
{
    // Solo el Administrador gestiona cuentas de usuario y sus roles.
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsuariosController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var usuarios = _userManager.Users.OrderBy(u => u.Nombres).ToList();
            var lista = new List<(ApplicationUser Usuario, string Rol)>();
            foreach (var usuario in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(usuario);
                lista.Add((usuario, roles.FirstOrDefault() ?? "Sin rol"));
            }
            return View(lista);
        }

        public IActionResult Create()
        {
            return View(new UsuarioFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsuarioFormViewModel modelo)
        {
            if (string.IsNullOrWhiteSpace(modelo.Password))
            {
                ModelState.AddModelError(nameof(modelo.Password), "La contraseña es obligatoria al crear un usuario.");
            }

            if (!ModelState.IsValid) return View(modelo);

            var usuario = new ApplicationUser
            {
                UserName = modelo.Email,
                Email = modelo.Email,
                Nombres = modelo.Nombres,
                Apellidos = modelo.Apellidos,
                EmailConfirmed = true
            };

            var resultado = await _userManager.CreateAsync(usuario, modelo.Password!);
            if (!resultado.Succeeded)
            {
                foreach (var error in resultado.Errors) ModelState.AddModelError(string.Empty, error.Description);
                return View(modelo);
            }

            await _userManager.AddToRoleAsync(usuario, modelo.Rol);
            TempData["Mensaje"] = "Usuario creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null) return NotFound();
            var roles = await _userManager.GetRolesAsync(usuario);

            var modelo = new UsuarioFormViewModel
            {
                Id = usuario.Id,
                Nombres = usuario.Nombres,
                Apellidos = usuario.Apellidos,
                Email = usuario.Email ?? string.Empty,
                Rol = roles.FirstOrDefault() ?? "Usuario"
            };
            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UsuarioFormViewModel modelo)
        {
            if (id != modelo.Id) return NotFound();
            if (!ModelState.IsValid) return View(modelo);

            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null) return NotFound();

            usuario.Nombres = modelo.Nombres;
            usuario.Apellidos = modelo.Apellidos;
            usuario.Email = modelo.Email;
            usuario.UserName = modelo.Email;
            await _userManager.UpdateAsync(usuario);

            if (!string.IsNullOrWhiteSpace(modelo.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
                await _userManager.ResetPasswordAsync(usuario, token, modelo.Password);
            }

            var rolesActuales = await _userManager.GetRolesAsync(usuario);
            if (!rolesActuales.Contains(modelo.Rol))
            {
                await _userManager.RemoveFromRolesAsync(usuario, rolesActuales);
                await _userManager.AddToRoleAsync(usuario, modelo.Rol);
            }

            TempData["Mensaje"] = "Usuario actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario != null)
            {
                await _userManager.DeleteAsync(usuario);
                TempData["Mensaje"] = "Usuario eliminado.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
