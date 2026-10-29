using System.Net;
using EquiposMVC.Models;
using EquiposMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EquiposMVC.Controllers
{
    public class EquiposController : Controller
    {
        private readonly ApiService _apiService;

        public EquiposController(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index(bool soloDisponibles = false)
        {
            var (equipos, error, status) = await _apiService.GetAsync<IEnumerable<EquipoViewModel>>($"api/Equipos?soloDisponibles={soloDisponibles}");

            if (status == HttpStatusCode.Unauthorized)
            {
                // Caso 11: Token expirado o ausente redirige automáticamente al Login
                return RedirectToAction("Login", "Account");
            }

            if (!string.IsNullOrEmpty(error))
            {
                TempData["ErrorMessage"] = error;
            }

            ViewBag.SoloDisponibles = soloDisponibles;
            return View(equipos ?? new List<EquipoViewModel>());
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                TempData["ErrorMessage"] = "No posee permisos para registrar equipos.";
                return RedirectToAction(nameof(Index));
            }

            var (categorias, _, _) = await _apiService.GetAsync<IEnumerable<CategoriaViewModel>>("api/Categorias");
            ViewBag.Categorias = categorias ?? new List<CategoriaViewModel>();

            return View(new EquipoViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(EquipoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var (categorias, _, _) = await _apiService.GetAsync<IEnumerable<CategoriaViewModel>>("api/Categorias");
                ViewBag.Categorias = categorias ?? new List<CategoriaViewModel>();
                return View(model);
            }

            var (success, error, status) = await _apiService.PostAsync("api/Equipos", model);

            if (status == HttpStatusCode.Unauthorized) return RedirectToAction("Login", "Account");

            if (success)
            {
                TempData["SuccessMessage"] = "Equipo creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, error ?? "Error al crear el equipo.");
            var (cats, _, _) = await _apiService.GetAsync<IEnumerable<CategoriaViewModel>>("api/Categorias");
            ViewBag.Categorias = cats ?? new List<CategoriaViewModel>();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var (equipo, error, status) = await _apiService.GetAsync<EquipoViewModel>($"api/Equipos/{id}");

            if (status == HttpStatusCode.Unauthorized) return RedirectToAction("Login", "Account");
            if (equipo == null) return RedirectToAction(nameof(Index));

            var (categorias, _, _) = await _apiService.GetAsync<IEnumerable<CategoriaViewModel>>("api/Categorias");
            ViewBag.Categorias = categorias ?? new List<CategoriaViewModel>();

            return View(equipo);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EquipoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var (categorias, _, _) = await _apiService.GetAsync<IEnumerable<CategoriaViewModel>>("api/Categorias");
                ViewBag.Categorias = categorias ?? new List<CategoriaViewModel>();
                return View(model);
            }

            var (success, error, status) = await _apiService.PutAsync($"api/Equipos/{model.Id}", model);

            if (status == HttpStatusCode.Unauthorized) return RedirectToAction("Login", "Account");

            if (success)
            {
                TempData["SuccessMessage"] = "Equipo actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }

            // Manejo de concurrencia optimista (Caso 9): Se conserva el modelo con los datos tipeados
            ModelState.AddModelError(string.Empty, error ?? "Error al actualizar el equipo.");
            var (cats, _, _) = await _apiService.GetAsync<IEnumerable<CategoriaViewModel>>("api/Categorias");
            ViewBag.Categorias = cats ?? new List<CategoriaViewModel>();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, error, status) = await _apiService.DeleteAsync($"api/Equipos/{id}");

            if (status == HttpStatusCode.Unauthorized) return RedirectToAction("Login", "Account");

            if (success)
            {
                TempData["SuccessMessage"] = "Equipo eliminado correctamente.";
            }
            else
            {
                TempData["ErrorMessage"] = error ?? "No se pudo eliminar el equipo.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}