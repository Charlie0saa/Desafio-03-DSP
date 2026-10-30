using System.ComponentModel.DataAnnotations;

namespace EquiposAPI.DTOs
{
    public class EquipoDto
    {
        public int Id { get; set; }
        public string CodigoInventario { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int CategoriaId { get; set; }
        public string NombreCategoria { get; set; } = string.Empty; // <-- Esta es la propiedad que faltaba
        public string Estado { get; set; } = string.Empty;
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
