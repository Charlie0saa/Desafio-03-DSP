using Microsoft.EntityFrameworkCore;
using EquiposAPI.Models; 

namespace EquiposReportes.Data
{
    public class ReportesDbContext : DbContext
    {
        public ReportesDbContext(DbContextOptions<ReportesDbContext> options) : base(options)
        {
        }

        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Equipo> Equipos { get; set; }
        public DbSet<Prestamo> Prestamos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuramos los nombres de las tablas para que coincidan con la base existente
            modelBuilder.Entity<Categoria>().ToTable("Categorias");
            modelBuilder.Entity<Equipo>().ToTable("Equipos");
            modelBuilder.Entity<Prestamo>().ToTable("Prestamos");
            modelBuilder.Entity<Usuario>().ToTable("Usuarios");
        }
    }
}