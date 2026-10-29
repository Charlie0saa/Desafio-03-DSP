using System.Net;
using EquiposMVC.Models;
using EquiposMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EquiposMVC.Controllers
{
    public class PrestamosController : Controller
    {
        private readonly ApiService _apiService;

        public PrestamosController(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            var (prestamos, error, status) = await _apiService.GetAsync<IEnumerable<PrestamoViewModel>>("api/Prestamos");

            if (status == HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!string.IsNullOrEmpty(error))
            {
                TempData["ErrorMessage"] = error;
            }

            return View(prestamos ?? new List<PrestamoViewModel>());
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var (equipos, _, status) = await _apiService.GetAsync<IEnumerable<EquipoViewModel>>("api/Equipos?soloDisponibles=true");

            if (status == HttpStatusCode.Unauthorized) return RedirectToAction("Login", "Account");

            ViewBag.Equipos = equipos ?? new List<EquipoViewModel>();
            return View(new CrearPrestamoViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CrearPrestamoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var (eqs, _, _) = await _apiService.GetAsync<IEnumerable<EquipoViewModel>>("api/Equipos?soloDisponibles=true");
                ViewBag.Equipos = eqs ?? new List<EquipoViewModel>();
                return View(model);
            }

            var (success, error, status) = await _apiService.PostAsync("api/Prestamos", model);

            if (status == HttpStatusCode.Unauthorized) return RedirectToAction("Login", "Account");

            if (success)
            {
                TempData["SuccessMessage"] = "Préstamo registrado exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, error ?? "No se pudo registrar el préstamo.");
            var (equipos, _, _) = await _apiService.GetAsync<IEnumerable<EquipoViewModel>>("api/Equipos?soloDisponibles=true");
            ViewBag.Equipos = equipos ?? new List<EquipoViewModel>();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Devolver(int id, string? observaciones)
        {
            var (success, error, status) = await _apiService.PostAsync($"api/Prestamos/{id}/devolver", observaciones ?? "");

            if (status == HttpStatusCode.Unauthorized) return RedirectToAction("Login", "Account");

            if (success)
            {
                TempData["SuccessMessage"] = "Devolución registrada con éxito.";
            }
            else
            {
                TempData["ErrorMessage"] = error ?? "No se pudo registrar la devolución.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}