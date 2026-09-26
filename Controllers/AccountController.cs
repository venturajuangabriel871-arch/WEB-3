using LibraryUVSucre.Models;
using LibraryUVSucre.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LibraryUVSucre.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToDashboard();
            }
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var resultado = await _signInManager.PasswordSignInAsync(modelo.Email, modelo.Password, modelo.RememberMe, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                var usuario = await _userManager.FindByEmailAsync(modelo.Email);
                return await RedirectToDashboardAsync(usuario!);
            }

            ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private async Task<IActionResult> RedirectToDashboardAsync(ApplicationUser usuario)
        {
            var roles = await _userManager.GetRolesAsync(usuario);
            if (roles.Contains("Administrador")) return RedirectToAction("Dashboard", "Administrador");
            if (roles.Contains("Bibliotecario")) return RedirectToAction("Dashboard", "Bibliotecario");
            return RedirectToAction("Dashboard", "Usuario");
        }

        private IActionResult RedirectToDashboard()
        {
            if (User.IsInRole("Administrador")) return RedirectToAction("Dashboard", "Administrador");
            if (User.IsInRole("Bibliotecario")) return RedirectToAction("Dashboard", "Bibliotecario");
            return RedirectToAction("Dashboard", "Usuario");
        }
    }
}
