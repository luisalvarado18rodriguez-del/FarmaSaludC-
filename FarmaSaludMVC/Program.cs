using FarmaSaludMVC.Data;
using FarmaSaludMVC.Interfaces;
using FarmaSaludMVC.Models;
using FarmaSaludMVC.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// (A) Confi para las rutas de Acceso para los roles
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Ruta a tu vista de Login
        options.AccessDeniedPath = "/Account/AccessDenied"; // Ruta si no tiene permisos
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Duración de la sesión
    });

// --- 1. CONFIGURACIÓN DE SERVICIOS (CONTENEDOR) ---
builder.Services.AddControllersWithViews();

// Base de Datos
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registro de servicios de lógica de negocio
builder.Services.AddScoped<IMedicamentoService, MedicamentoService>();
builder.Services.AddScoped<ICarritoService, CarritoService>();
builder.Services.AddScoped<IReservaService, ReservaService>();

// Requerido para acceder a la sesión desde el CarritoService
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// Configuración de Sesión y Caché
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();



//if (!app.Environment.IsDevelopment())    (Para visualizar errores y corregir descomenta el if)
//{
    app.UseExceptionHandler("/Home/Error");
//}

// Esto debe ir antes de app.UseRouting()
app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// IMPORTANTE: UseSession DEBE ir después de UseRouting y antes de UseAuthorization/MapControllerRoute
app.UseSession();

app.UseAuthentication();
app.UseAuthorization(); 
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();

    if (!context.Usuarios.Any(u => u.Rol == "SuperAdmin"))
    {
        context.Usuarios.Add(new Usuario
        {
            Email = "admin@farmasalud.com",
            Password = FarmaSaludMVC.security.SecurityHelper.EncriptarPassword("admin123"),
            Rol = "SuperAdmin",
            Activo = true
        });
        context.SaveChanges();
    }
}

app.Run();
