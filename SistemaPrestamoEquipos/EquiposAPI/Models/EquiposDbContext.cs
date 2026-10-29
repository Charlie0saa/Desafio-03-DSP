using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EquiposAPI.Models;

public partial class EquiposDbContext : DbContext
{
    public EquiposDbContext()
    {
    }

    public EquiposDbContext(DbContextOptions<EquiposDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Categoria> Categorias { get; set; }

    public virtual DbSet<Equipo> Equipos { get; set; }

    public virtual DbSet<Prestamo> Prestamos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-S9N1VV4\\SQLEXPRESS;Database=EquiposDb;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasIndex(e => e.Nombre, "IX_Categorias_Nombre").IsUnique();

            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<Equipo>(entity =>
        {
            entity.HasIndex(e => e.CategoriaId, "IX_Equipos_CategoriaId");

            entity.HasIndex(e => e.CodigoInventario, "IX_Equipos_CodigoInventario").IsUnique();

            entity.Property(e => e.CodigoInventario).HasMaxLength(50);
            entity.Property(e => e.Estado).HasMaxLength(20);
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Categoria).WithMany(p => p.Equipos)
                .HasForeignKey(d => d.CategoriaId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Prestamo>(entity =>
        {
            entity.HasIndex(e => e.EquipoId, "IX_Prestamos_EquipoId");

            entity.HasIndex(e => e.UsuarioId, "IX_Prestamos_UsuarioId");

            entity.HasOne(d => d.Equipo).WithMany(p => p.Prestamos)
                .HasForeignKey(d => d.EquipoId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Usuario).WithMany(p => p.Prestamos)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasIndex(e => e.Username, "IX_Usuarios_Username").IsUnique();

            entity.Property(e => e.Rol).HasMaxLength(20);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
