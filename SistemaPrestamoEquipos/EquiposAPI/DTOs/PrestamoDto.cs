using System.ComponentModel.DataAnnotations;

namespace EquiposAPI.DTOs
{
    public class PrestamoDto
    {
        public int Id { get; set; }
        public int EquipoId { get; set; }
        public string? EquipoNombre { get; set; }
        public string? CodigoInventario { get; set; }

        public int UsuarioId { get; set; }
        public string? Username { get; set; }

        public DateTime FechaPrestamo { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public DateTime? FechaDevolucion { get; set; }
        public string? Observaciones { get; set; }
    }

    public class CrearPrestamoDto
    {
        [Required]
        public int EquipoId { get; set; }

        // Si es Estudiante, el UsuarioId se extrae del Token. Si es Admin/Docente, puede especificarse.
        public int? UsuarioId { get; set; }

        [Required]
        public DateTime FechaVencimiento { get; set; }

        public string? Observaciones { get; set; }
    }
}
