using EquiposMVC.Models;
using EquiposMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EquiposMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiService _apiService;

        public AccountController(ApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = await _apiService.LoginAsync(model);

            if (response != null && !string.IsNullOrEmpty(response.Token))
            {
                // Guardar token y datos del usuario en Session (Requisito Rúbrica)
                HttpContext.Session.SetString("JWToken", response.Token);
                HttpContext.Session.SetString("Username", response.Username);
                HttpContext.Session.SetString("UserRole", response.Rol);

                return RedirectToAction("Index", "Equipos");
            }

            ModelState.AddModelError(string.Empty, "Credenciales incorrectas o servidor no disponible.");
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}