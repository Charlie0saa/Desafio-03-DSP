using System.ComponentModel.DataAnnotations;

namespace EquiposAPI.DTOs
{
    public class CrearEquipoDto
    {
        [Required]
        [StringLength(50)]
        public string CodigoInventario { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        [Required]
        public int CategoriaId { get; set; }

        public string Estado { get; set; } = "Disponible";
    }
}