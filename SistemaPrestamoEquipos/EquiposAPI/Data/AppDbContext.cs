using Microsoft.EntityFrameworkCore;
using EquiposAPI.Models;

namespace EquiposAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Tablas
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Equipo> Equipos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Prestamo> Prestamos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Índices Únicos (Requerimiento de la rúbrica)
            modelBuilder.Entity<Categoria>()
                .HasIndex(c => c.Nombre)
                .IsUnique();

            modelBuilder.Entity<Equipo>()
                .HasIndex(e => e.CodigoInventario)
                .IsUnique();

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // 2. Relaciones con DeleteBehavior.Restrict
            modelBuilder.Entity<Equipo>()
                .HasOne(e => e.Categoria)
                .WithMany(c => c.Equipos)
                .HasForeignKey(e => e.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Prestamo>()
                .HasOne(p => p.Equipo)
                .WithMany(e => e.Prestamos)
                .HasForeignKey(p => p.EquipoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Prestamo>()
                .HasOne(p => p.Usuario)
                .WithMany(u => u.Prestamos)
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. Datos Semilla (Seed Data) únicamente para Categorías
            modelBuilder.Entity<Categoria>().HasData(
                new Categoria { Id = 1, Nombre = "Laptops" },
                new Categoria { Id = 2, Nombre = "Proyectores" },
                new Categoria { Id = 3, Nombre = "Tarjetas de Desarrollo" },
                new Categoria { Id = 4, Nombre = "Cámaras y Sensorica" }
            );
        }
    }
}
