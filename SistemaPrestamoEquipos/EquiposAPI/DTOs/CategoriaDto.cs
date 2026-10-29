using System.ComponentModel.DataAnnotations;

namespace EquiposAPI.DTOs
{
    public class CategoriaDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la categoría es obligatorio")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;
    }
}
