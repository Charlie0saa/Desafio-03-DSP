
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
    [Authorize] // Requiere Token JWT
    public class CategoriasController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public CategoriasController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Categorias
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoriaDto>>> GetCategorias()
        {
            var categorias = await _context.Categorias.ToListAsync();
            return Ok(_mapper.Map<IEnumerable<CategoriaDto>>(categorias));
        }

        // GET: api/Categorias/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoriaDto>> GetCategoria(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);

            if (categoria == null)
            {
                return NotFound(new { mensaje = "Categoría no encontrada." });
            }

            return Ok(_mapper.Map<CategoriaDto>(categoria));
        }

        // POST: api/Categorias
        [HttpPost]
        [Authorize(Roles = "Admin")] // Caso 4: Si no es Admin responde 403 Forbidden
        public async Task<ActionResult<CategoriaDto>> PostCategoria(CategoriaDto categoriaDto)
        {
            // Nombre único -> 409 Conflict
            if (await _context.Categorias.AnyAsync(c => c.Nombre.ToLower() == categoriaDto.Nombre.ToLower()))
            {
                return Conflict(new { mensaje = "Ya existe una categoría con ese nombre." });
            }

            var categoria = _mapper.Map<Categoria>(categoriaDto);
            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            categoriaDto.Id = categoria.Id;
            return CreatedAtAction(nameof(GetCategoria), new { id = categoria.Id }, categoriaDto);
        }

        // PUT: api/Categorias/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutCategoria(int id, CategoriaDto categoriaDto)
        {
            if (id != categoriaDto.Id)
            {
                return BadRequest(new { mensaje = "El ID no coincide." });
            }

            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
            {
                return NotFound(new { mensaje = "Categoría no encontrada." });
            }

            // Nombre único ignorando la categoría actual
            if (await _context.Categorias.AnyAsync(c => c.Nombre.ToLower() == categoriaDto.Nombre.ToLower() && c.Id != id))
            {
                return Conflict(new { mensaje = "Ya existe otra categoría con ese nombre." });
            }

            _mapper.Map(categoriaDto, categoria);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Categorias/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategoria(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
            {
                return NotFound(new { mensaje = "Categoría no encontrada." });
            }

            // Verificar si tiene equipos asociados (DeleteBehavior.Restrict)
            var tieneEquipos = await _context.Equipos.AnyAsync(e => e.CategoriaId == id);
            if (tieneEquipos)
            {
                return Conflict(new { mensaje = "No se puede eliminar la categoría porque tiene equipos asociados." });
            }

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
