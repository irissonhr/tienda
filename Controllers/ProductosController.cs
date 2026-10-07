using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaApp.Data;
using TiendaApp.Models;

namespace TiendaApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly AppDbContext _db;
    public ProductosController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var productos = await _db.Productos.OrderBy(p => p.Id).ToListAsync();
        return Ok(productos);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var producto = await _db.Productos.FindAsync(id);
        if (producto == null)
            return NotFound(new { mensaje = "Producto no encontrado" });

        return Ok(producto);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Crear([FromBody] CrearProductoDto dto)
    {
        if (dto == null)
            return BadRequest(new { mensaje = "Datos incompletos" });

        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return BadRequest(new { mensaje = "El nombre del producto es obligatorio" });

        if (dto.Precio < 0)
            return BadRequest(new { mensaje = "El precio no puede ser negativo" });

        var producto = new Producto
        {
            Nombre = dto.Nombre.Trim(),
            Descripcion = dto.Descripcion.Trim(),
            Precio = dto.Precio,
            Emoji = string.IsNullOrWhiteSpace(dto.Emoji) ? "📦" : dto.Emoji.Trim()
        };

        _db.Productos.Add(producto);
        await _db.SaveChangesAsync();

        return Ok(producto);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarProductoDto dto)
    {
        if (dto == null)
            return BadRequest(new { mensaje = "Datos incompletos" });

        var producto = await _db.Productos.FindAsync(id);
        if (producto == null)
            return NotFound(new { mensaje = "Producto no encontrado" });

        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return BadRequest(new { mensaje = "El nombre del producto es obligatorio" });

        if (dto.Precio < 0)
            return BadRequest(new { mensaje = "El precio no puede ser negativo" });

        producto.Nombre = dto.Nombre.Trim();
        producto.Descripcion = dto.Descripcion.Trim();
        producto.Precio = dto.Precio;
        producto.Emoji = string.IsNullOrWhiteSpace(dto.Emoji) ? "📦" : dto.Emoji.Trim();

        await _db.SaveChangesAsync();

        return Ok(producto);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var producto = await _db.Productos.FindAsync(id);
        if (producto == null)
            return NotFound(new { mensaje = "Producto no encontrado" });

        _db.Productos.Remove(producto);
        await _db.SaveChangesAsync();

        return Ok(new { mensaje = "Producto eliminado", id });
    }
}
