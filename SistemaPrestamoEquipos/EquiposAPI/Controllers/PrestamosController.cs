using System.Security.Claims;
using AutoMapper;
using EquiposAPI.Data;
using EquiposAPI.DTOs;
using EquiposAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EquiposAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PrestamosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public PrestamosController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Prestamos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PrestamoDto>>> GetPrestamos()
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
            {
                return Unauthorized(new { mensaje = "Token inválido." });
            }

            IQueryable<Prestamo> query = _context.Prestamos
                .Include(p => p.Equipo)
                .Include(p => p.Usuario);

            // Caso 10: Un Estudiante solo puede ver sus propios préstamos
            if (userRole == "Estudiante")
            {
                query = query.Where(p => p.UsuarioId == currentUserId);
            }

            var prestamos = await query.ToListAsync();
            return Ok(_mapper.Map<IEnumerable<PrestamoDto>>(prestamos));
        }

        // GET: api/Prestamos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PrestamoDto>> GetPrestamo(int id)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _ = int.TryParse(userIdClaim, out int currentUserId);

            var prestamo = await _context.Prestamos
                .Include(p => p.Equipo)
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prestamo == null)
            {
                return NotFound(new { mensaje = "Préstamo no encontrado." });
            }

            // Caso 10: Protección estricta si es Estudiante
            if (userRole == "Estudiante" && prestamo.UsuarioId != currentUserId)
            {
                return Forbid(); // 403 Forbidden
            }

            return Ok(_mapper.Map<PrestamoDto>(prestamo));
        }

        // POST: api/Prestamos
        [HttpPost]
        [Authorize(Roles = "Admin,Docente,Estudiante")]
        public async Task<ActionResult<PrestamoDto>> RegistrarPrestamo(CrearPrestamoDto dto)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _ = int.TryParse(userIdClaim, out int currentUserId);

            int usuarioPrestamoId;

            // Extraer UsuarioId del Claim obligatoriamente si es Estudiante
            if (userRole == "Estudiante")
            {
                usuarioPrestamoId = currentUserId;
            }
            else
            {
                usuarioPrestamoId = dto.UsuarioId ?? currentUserId;
            }

            // Regla 3 & Caso 7: FechaVencimiento no anterior a hoy ni superior a 7 días
            var hoy = DateTime.Now.Date;
            var fechaVencimientoDate = dto.FechaVencimiento.Date;

            if (fechaVencimientoDate < hoy || fechaVencimientoDate > hoy.AddDays(7))
            {
                return BadRequest(new { mensaje = "La fecha de vencimiento no puede ser anterior a hoy ni superar los 7 días desde la fecha de préstamo." });
            }

            // Regla 1 & Caso 6: Equipo disponible
            var equipo = await _context.Equipos.FindAsync(dto.EquipoId);
            if (equipo == null)
            {
                return NotFound(new { mensaje = "Equipo no encontrado." });
            }

            if (equipo.Estado != "Disponible")
            {
                return Conflict(new { mensaje = $"El equipo no se encuentra disponible para préstamo. Estado actual: {equipo.Estado}." });
            }

            // Regla 2: Transaccionalidad
            equipo.Estado = "Prestado";

            var prestamo = _mapper.Map<Prestamo>(dto);
            prestamo.UsuarioId = usuarioPrestamoId;
            prestamo.FechaPrestamo = DateTime.Now;

            _context.Prestamos.Add(prestamo);
            await _context.SaveChangesAsync();

            var prestamoCreado = await _context.Prestamos
                .Include(p => p.Equipo)
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(p => p.Id == prestamo.Id);

            return CreatedAtAction(nameof(GetPrestamo), new { id = prestamo.Id }, _mapper.Map<PrestamoDto>(prestamoCreado));
        }

        // POST: api/Prestamos/5/devolver
        [HttpPost("{id}/devolver")]
        [Authorize(Roles = "Admin,Docente")]
        public async Task<IActionResult> RegistrarDevolucion(int id, [FromBody] string? observaciones)
        {
            var prestamo = await _context.Prestamos
                .Include(p => p.Equipo)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prestamo == null)
            {
                return NotFound(new { mensaje = "Préstamo no encontrado." });
            }

            if (prestamo.FechaDevolucion != null)
            {
                return Conflict(new { mensaje = "El equipo de este préstamo ya fue devuelto anteriormente." });
            }

            // Regla 4: Registrar devolución y liberar equipo
            prestamo.FechaDevolucion = DateTime.Now;
            if (!string.IsNullOrEmpty(observaciones))
            {
                prestamo.Observaciones = observaciones;
            }

            if (prestamo.Equipo != null)
            {
                prestamo.Equipo.Estado = "Disponible";
            }

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Devolución registrada con éxito." });
        }
    }
}

// Gestiona el registro, consulta y control de las solicitudes de préstamos de equipos //