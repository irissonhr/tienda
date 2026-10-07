using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaApp.Data;
using TiendaApp.Models;
using TiendaApp.Services;

namespace TiendaApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TokenService _token;

    public AuthController(AppDbContext db, TokenService token)
    {
        _db = db;
        _token = token;
    }

    [HttpPost("registro")]
    public async Task<IActionResult> Registro([FromBody] RegistroDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new { mensaje = "Datos incompletos" });

        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return BadRequest(new { mensaje = "El nombre es obligatorio" });

        var email = dto.Email.Trim().ToLower();

        if (dto.Password.Length < 6)
            return BadRequest(new { mensaje = "La contraseña debe tener al menos 6 caracteres" });

        if (await _db.Usuarios.AnyAsync(u => u.Email == email))
            return BadRequest(new { mensaje = "Este correo ya está registrado" });

        var usuario = new Usuario
        {
            Nombre = dto.Nombre.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            IsAdmin = false
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        return Ok(new { mensaje = "Registro exitoso" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new { mensaje = "Datos incompletos" });

        var email = dto.Email.Trim().ToLower();
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

        if (usuario == null || !BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash))
            return Unauthorized(new { mensaje = "Correo o contraseña incorrectos" });

        var token = _token.GenerarToken(usuario);

        return Ok(new AuthResponseDto
        {
            Token = token,
            Nombre = usuario.Nombre,
            Email = usuario.Email,
            Rol = usuario.IsAdmin ? "Admin" : "Usuario",
            EsAdmin = usuario.IsAdmin
        });
    }
}
