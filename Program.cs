using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using TiendaApp.Data;
using TiendaApp.Models;
using TiendaApp.Services;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Falta configurar ConnectionStrings:DefaultConnection.");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<TokenService>();
builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var configuredKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(configuredKey))
{
    configuredKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
    builder.Configuration["Jwt:Key"] = configuredKey;
    Console.Error.WriteLine(
        "WARNING: Jwt:Key is not configured. A temporary signing key was generated; " +
        "existing login tokens will stop working after a restart. Set Jwt__Key for stable sessions.");
}

var key = Encoding.UTF8.GetBytes(configuredKey);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    var connection = db.Database.GetDbConnection();
    if (connection.State != ConnectionState.Open)
        connection.Open();

    var hasAdminColumn = false;
    using (var command = connection.CreateCommand())
    {
        command.CommandText = "PRAGMA table_info('Usuarios')";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            if (reader.GetString(1) == "IsAdmin")
            {
                hasAdminColumn = true;
                break;
            }
        }
    }

    if (!hasAdminColumn)
        db.Database.ExecuteSqlRaw("ALTER TABLE Usuarios ADD COLUMN IsAdmin INTEGER NOT NULL DEFAULT 0");

    var adminEmail = builder.Configuration["Admin:Email"]?.Trim().ToLowerInvariant();
    var adminPassword = builder.Configuration["Admin:Password"];
    if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
    {
        Console.Error.WriteLine(
            "WARNING: Admin:Email or Admin:Password is not configured. " +
            "The storefront will start, but no admin account will be created. " +
            "Set Admin__Email and Admin__Password to enable product management.");
    }
    else
    {
        if (adminPassword.Length < 12)
            throw new InvalidOperationException("Admin:Password debe tener al menos 12 caracteres.");

        var admin = db.Usuarios.FirstOrDefault(u => u.Email == adminEmail);
        if (admin == null)
        {
            db.Usuarios.Add(new Usuario
            {
                Nombre = "Administrador",
                Email = adminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                IsAdmin = true,
                FechaRegistro = DateTime.UtcNow
            });
            db.SaveChanges();
        }
        else if (!admin.IsAdmin)
        {
            admin.IsAdmin = true;
            db.SaveChanges();
        }
    }
}

app.UseCors("PermitirTodo");
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
