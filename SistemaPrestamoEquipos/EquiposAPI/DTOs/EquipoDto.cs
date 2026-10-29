using System.ComponentModel.DataAnnotations;

namespace EquiposAPI.DTOs
{
    public class EquipoDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El código de inventario es obligatorio")]
        [StringLength(50)]
        public string CodigoInventario { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del equipo es obligatorio")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        [Required]
        public int CategoriaId { get; set; }
        public string? CategoriaNombre { get; set; }

        public string Estado { get; set; } = "Disponible";

        // Importante para la Concurrencia Optimista (Rúbrica)
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
