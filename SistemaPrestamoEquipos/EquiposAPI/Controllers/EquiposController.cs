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
    public class EquiposController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public EquiposController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Equipos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EquipoDto>>> GetEquipos([FromQuery] bool soloDisponibles = false)
        {
            var query = _context.Equipos.Include(e => e.Categoria).AsQueryable();

            if (soloDisponibles)
            {
                query = query.Where(e => e.Estado == "Disponible");
            }

            var equipos = await query.ToListAsync();
            return Ok(_mapper.Map<IEnumerable<EquipoDto>>(equipos));
        }

        // GET: api/Equipos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EquipoDto>> GetEquipo(int id)
        {
            var equipo = await _context.Equipos.Include(e => e.Categoria).FirstOrDefaultAsync(e => e.Id == id);

            if (equipo == null)
            {
                return NotFound(new { mensaje = "Equipo no encontrado." });
            }

            return Ok(_mapper.Map<EquipoDto>(equipo));
        }

        // POST: api/Equipos
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<EquipoDto>> CrearEquipo(CrearEquipoDto dto)
        {
            var existeCodigo = await _context.Equipos.AnyAsync(e => e.CodigoInventario == dto.CodigoInventario);
            if (existeCodigo)
            {
                return BadRequest(new { mensaje = "El código de inventario ya existe." });
            }

            var equipo = _mapper.Map<Equipo>(dto);

            if (string.IsNullOrEmpty(equipo.Estado))
            {
                equipo.Estado = "Disponible";
            }

            try
            {
                _context.Equipos.Add(equipo);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var mensajeDetallado = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return BadRequest(new { mensaje = $"Error de SQL: {mensajeDetallado}" });
            }

            var equipoCreado = await _context.Equipos
                .Include(e => e.Categoria)
                .FirstOrDefaultAsync(e => e.Id == equipo.Id);

            return CreatedAtAction(nameof(GetEquipo), new { id = equipo.Id }, _mapper.Map<EquipoDto>(equipoCreado));
        }

        // PUT: api/Equipos/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutEquipo(int id, ActualizarEquipoDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest(new { mensaje = "El ID no coincide." });
            }

            var equipoBd = await _context.Equipos.FindAsync(id);
            if (equipoBd == null)
            {
                return NotFound(new { mensaje = "Equipo no encontrado." });
            }

            if (await _context.Equipos.AnyAsync(e => e.CodigoInventario.ToLower() == dto.CodigoInventario.ToLower() && e.Id != id))
            {
                return Conflict(new { mensaje = "El código de inventario ya pertenece a otro equipo." });
            }

            // Asignar OriginalValue de RowVersion para control de concurrencia optimista
            _context.Entry(equipoBd).Property(e => e.RowVersion).OriginalValue = dto.RowVersion;

            equipoBd.Nombre = dto.Nombre;
            equipoBd.CodigoInventario = dto.CodigoInventario;
            equipoBd.Descripcion = dto.Descripcion;
            equipoBd.CategoriaId = dto.CategoriaId;
            equipoBd.Estado = dto.Estado;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new { mensaje = "El equipo fue modificado por otro usuario. Por favor recargue la página e intente de nuevo." });
            }

            return NoContent();
        }

        // DELETE: api/Equipos/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteEquipo(int id)
        {
            var equipo = await _context.Equipos.FindAsync(id);
            if (equipo == null)
            {
                return NotFound(new { mensaje = "Equipo no encontrado." });
            }

            var tienePrestamos = await _context.Prestamos.AnyAsync(p => p.EquipoId == id);
            if (tienePrestamos)
            {
                return Conflict(new { mensaje = "No se puede eliminar un equipo que posee préstamos registrados históricamente." });
            }

            _context.Equipos.Remove(equipo);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

// Gestiona las solicitudes HTTP para el registro, consulta, actualización y eliminación de los equipos del sistema // 