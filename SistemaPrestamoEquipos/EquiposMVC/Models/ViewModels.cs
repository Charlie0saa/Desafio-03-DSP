using System.ComponentModel.DataAnnotations;

namespace EquiposMVC.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El usuario es obligatorio")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseModel
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
    }

    public class CategoriaViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la categoría es obligatorio")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;
    }

    public class EquipoViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El código de inventario es obligatorio")]
        public string CodigoInventario { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del equipo es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar una categoría")]
        public int CategoriaId { get; set; }
        public string? CategoriaNombre { get; set; }

        public string Estado { get; set; } = "Disponible";

        // RowVersion para manejo de concurrencia optimista en el cliente
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    public class PrestamoViewModel
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

    public class CrearPrestamoViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un equipo")]
        public int EquipoId { get; set; }

        public int? UsuarioId { get; set; }

        [Required(ErrorMessage = "La fecha de vencimiento es obligatoria")]
        [DataType(DataType.Date)]
        public DateTime FechaVencimiento { get; set; } = DateTime.Now.AddDays(7);

        public string? Observaciones { get; set; }
    }
}