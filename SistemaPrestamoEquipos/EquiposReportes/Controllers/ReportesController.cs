using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EquiposReportes.Data;
using EquiposReportes.ViewModels;

namespace EquiposReportes.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportesController : Controller
    {
        private readonly ReportesDbContext _context;

        public ReportesController(ReportesDbContext context)
        {
            _context = context;
        }

        // 1. Inventario por categoría y estado
        public async Task<IActionResult> InventarioPorCategoria()
        {
            var resultado = await _context.Equipos
                .AsNoTracking()
                .Include(e => e.Categoria)
                .GroupBy(e => new { CategoriaNombre = e.Categoria.Nombre, e.Estado })
                .Select(g => new InventarioPorCategoriaViewModel
                {
                    CategoriaNombre = g.Key.CategoriaNombre,
                    Estado = g.Key.Estado,
                    Cantidad = g.Count()
                })
                .ToListAsync();

            return View(resultado);
        }

        // 2. Préstamos vencidos
        public async Task<IActionResult> PrestamosVencidos()
        {
            var hoy = DateTime.Now.Date;

            var resultado = await _context.Prestamos
                .AsNoTracking()
                .Include(p => p.Equipo)
                .Include(p => p.Usuario)
                .Where(p => p.FechaDevolucion == null && p.FechaVencimiento < hoy)
                .Select(p => new PrestamoVencidoViewModel
                {
                    PrestamoId = p.Id,
                    EquipoNombre = p.Equipo.Nombre,
                    CodigoInventario = p.Equipo.CodigoInventario,
                    NombreUsuario = p.Usuario.Username,
                    FechaPrestamo = p.FechaPrestamo,
                    FechaVencimiento = p.FechaVencimiento,
                    DiasAtraso = (hoy - p.FechaVencimiento.Date).Days
                })
                .ToListAsync();

            return View(resultado);
        }

        // 3. Top 5 equipos más prestados (con filtro opcional de fechas)
        public async Task<IActionResult> TopEquipos(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var inicio = fechaInicio ?? DateTime.MinValue;
            var fin = fechaFin ?? DateTime.MaxValue;

            var resultado = await _context.Prestamos
                .AsNoTracking()
                .Include(p => p.Equipo)
                .ThenInclude(e => e.Categoria)
                .Where(p => p.FechaPrestamo >= inicio && p.FechaPrestamo <= fin)
                .GroupBy(p => new { p.Equipo.Nombre, p.Equipo.CodigoInventario, CategoriaNombre = p.Equipo.Categoria.Nombre })
                .Select(g => new TopEquipoViewModel
                {
                    EquipoNombre = g.Key.Nombre,
                    CodigoInventario = g.Key.CodigoInventario,
                    CategoriaNombre = g.Key.CategoriaNombre,
                    TotalPrestamos = g.Count()
                })
                .OrderByDescending(t => t.TotalPrestamos)
                .Take(5)
                .ToListAsync();

            ViewBag.FechaInicio = fechaInicio?.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin?.ToString("yyyy-MM-dd");

            return View(resultado);
        }

        // 4. Historial por usuario (búsqueda por Username)
        public async Task<IActionResult> HistorialUsuario(string? username)
        {
            var query = _context.Prestamos
                .AsNoTracking()
                .Include(p => p.Equipo)
                .Include(p => p.Usuario)
                .AsQueryable();

            if (!string.IsNullOrEmpty(username))
            {
                query = query.Where(p => p.Usuario.Username.Contains(username));
            }

            var resultado = await query
                .Select(p => new HistorialUsuarioViewModel
                {
                    Username = p.Usuario.Username,
                    EquipoNombre = p.Equipo.Nombre,
                    FechaPrestamo = p.FechaPrestamo,
                    FechaVencimiento = p.FechaVencimiento,
                    FechaDevolucion = p.FechaDevolucion,
                    Observaciones = p.Observaciones
                })
                .ToListAsync();

            ViewBag.Username = username;
            return View(resultado);
        }
    }
}