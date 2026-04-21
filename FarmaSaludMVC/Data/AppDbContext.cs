using Microsoft.EntityFrameworkCore;
using FarmaSaludMVC.Models;

namespace FarmaSaludMVC.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        

        // Tablas de la Base de Datos 
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Medicamento> Medicamentos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<ReservaDetalle> ReservasDetalles { get; set; }
        public DbSet<Reserva> Reservas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        // Ejemplo de índice único para el Email, similar al DNI del instructivo
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}