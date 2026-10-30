namespace EquiposReportes.ViewModels
{
    // 1. Inventario por categoría
    public class InventarioPorCategoriaViewModel
    {
        public string CategoriaNombre { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }

    // 2. Préstamos vencidos
    public class PrestamoVencidoViewModel
    {
        public int PrestamoId { get; set; }
        public string EquipoNombre { get; set; } = string.Empty;
        public string CodigoInventario { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public DateTime FechaPrestamo { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public int DiasAtraso { get; set; }
    }

    // 3. Top 5 equipos más prestados
    public class TopEquipoViewModel
    {
        public string EquipoNombre { get; set; } = string.Empty;
        public string CodigoInventario { get; set; } = string.Empty;
        public string CategoriaNombre { get; set; } = string.Empty;
        public int TotalPrestamos { get; set; }
    }

    // 4. Historial por usuario
    public class HistorialUsuarioViewModel
    {
        public string Username { get; set; } = string.Empty;
        public string EquipoNombre { get; set; } = string.Empty;
        public DateTime FechaPrestamo { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public DateTime? FechaDevolucion { get; set; }
        public string? Observaciones { get; set; }
    }
}