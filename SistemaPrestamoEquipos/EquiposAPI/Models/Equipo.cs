using System;
using System.Collections.Generic;

namespace EquiposAPI.Models;

public partial class Equipo
{
    public int Id { get; set; }

    public string CodigoInventario { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public int CategoriaId { get; set; }

    public string Estado { get; set; } = null!;

    public byte[] RowVersion { get; set; } = null!;

    public virtual Categoria Categoria { get; set; } = null!;

    public virtual ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
}
