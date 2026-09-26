using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryUVSucre.Controllers
{
    // Punto de entrada: reenvía a cada usuario a su dashboard según su rol.
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (User.IsInRole("Administrador")) return RedirectToAction("Dashboard", "Administrador");
            if (User.IsInRole("Bibliotecario")) return RedirectToAction("Dashboard", "Bibliotecario");
            return RedirectToAction("Dashboard", "Usuario");
        }
    }
}
