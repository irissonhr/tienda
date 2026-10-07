using Microsoft.EntityFrameworkCore;
using TiendaApp.Models;

namespace TiendaApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Producto> Productos => Set<Producto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Producto>().HasData(
            new Producto { Id = 1, Nombre = "Camiseta Básica", Descripcion = "Algodón 100%", Precio = 199, Emoji = "👕" },
            new Producto { Id = 2, Nombre = "Zapatos Deportivos", Descripcion = "Cómodos y ligeros", Precio = 599, Emoji = "👟" },
            new Producto { Id = 3, Nombre = "Mochila Escolar", Descripcion = "Resistente", Precio = 349, Emoji = "🎒" },
            new Producto { Id = 4, Nombre = "Gorra Clásica", Descripcion = "Ajustable unisex", Precio = 149, Emoji = "🧢" },
            new Producto { Id = 5, Nombre = "Reloj Digital", Descripcion = "Resistente al agua", Precio = 799, Emoji = "⌚" },
            new Producto { Id = 6, Nombre = "Lentes de Sol", Descripcion = "Protección UV400", Precio = 249, Emoji = "🕶️" },
            new Producto { Id = 7, Nombre = "Audífonos Bluetooth", Descripcion = "Sonido HD 20h", Precio = 899, Emoji = "🎧" },
            new Producto { Id = 8, Nombre = "Botella Térmica", Descripcion = "Frío/caliente 12h", Precio = 279, Emoji = "🍶" }
        );
    }
}
