using LibraryUVSucre.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LibraryUVSucre.Data
{
    // Crea los 3 roles del sistema, un usuario Administrador por defecto y algunos libros de ejemplo.
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var context = services.GetRequiredService<AppDbContext>();

            await context.Database.MigrateAsync();

            string[] roles = { "Administrador", "Bibliotecario", "Usuario" };
            foreach (var rol in roles)
            {
                if (!await roleManager.RoleExistsAsync(rol))
                {
                    await roleManager.CreateAsync(new IdentityRole(rol));
                }
            }

            // Usuario Administrador por defecto.
            var adminEmail = "admin@uvsucre.edu.bo";
            if (await userManager.FindByEmailAsync(adminEmail) is null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Nombres = "Administrador",
                    Apellidos = "Sistema",
                    EmailConfirmed = true
                };
                var resultado = await userManager.CreateAsync(admin, "Admin123$");
                if (resultado.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Administrador");
                }
            }

            // Usuario Bibliotecario por defecto.
            var bibliotecarioEmail = "bibliotecario@uvsucre.edu.bo";
            if (await userManager.FindByEmailAsync(bibliotecarioEmail) is null)
            {
                var bibliotecario = new ApplicationUser
                {
                    UserName = bibliotecarioEmail,
                    Email = bibliotecarioEmail,
                    Nombres = "Juana",
                    Apellidos = "Bibliotecaria",
                    EmailConfirmed = true
                };
                var resultado = await userManager.CreateAsync(bibliotecario, "Biblio123$");
                if (resultado.Succeeded)
                {
                    await userManager.AddToRoleAsync(bibliotecario, "Bibliotecario");
                }
            }

            // Usuario final de ejemplo.
            var usuarioEmail = "usuario@uvsucre.edu.bo";
            if (await userManager.FindByEmailAsync(usuarioEmail) is null)
            {
                var usuario = new ApplicationUser
                {
                    UserName = usuarioEmail,
                    Email = usuarioEmail,
                    Nombres = "Carlos",
                    Apellidos = "Estudiante",
                    EmailConfirmed = true
                };
                var resultado = await userManager.CreateAsync(usuario, "Usuario123$");
                if (resultado.Succeeded)
                {
                    await userManager.AddToRoleAsync(usuario, "Usuario");
                }
            }

            // Libros de ejemplo, solo si la tabla está vacía.
            if (!context.Libros.Any())
            {
                context.Libros.AddRange(
                    new Libro { Titulo = "Clean Code", Autor = "Robert C. Martin", ISBN = "9780132350884", Editorial = "Prentice Hall", Categoria = "Ingeniería de Software", AnioPublicacion = 2008, CantidadTotal = 5, CantidadDisponible = 5 },
                    new Libro { Titulo = "Cien años de soledad", Autor = "Gabriel García Márquez", ISBN = "9780307474728", Editorial = "Sudamericana", Categoria = "Literatura", AnioPublicacion = 1967, CantidadTotal = 3, CantidadDisponible = 3 },
                    new Libro { Titulo = "Bases de Datos", Autor = "Raghu Ramakrishnan", ISBN = "9780072465631", Editorial = "McGraw-Hill", Categoria = "Ciencias de la Computación", AnioPublicacion = 2003, CantidadTotal = 4, CantidadDisponible = 4 }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
