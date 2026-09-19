using apitienda.Data;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using productos.Data;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;




/// <summary>
/// Carga las variables de entorno.
/// </summary>
Env.Load();
var builder = WebApplication.CreateBuilder(args);


/// <summary>
/// Obtiene la cadena de conexión a la base de datos desde las variables de entorno o configuración.
/// </summary>


var connectionStringUser = builder.Configuration.GetConnectionString("DefaultConnectionUsers");
var connectionStringProducts = builder.Configuration.GetConnectionString("DefaultConnectionProducts");


/// <summary>
/// Configura el contexto de datos con la base de datos PostgreSQL.
/// </summary>
builder.Services.AddDbContext<DataContext>(
    options => options.UseNpgsql(connectionStringUser)
);

/// <summary>
/// Configura el contexto de datos con la base de datos PostgreSQL.
/// </summary>
builder.Services.AddDbContext<DataContextProduct>(
    options => options.UseNpgsql(connectionStringProducts)
);




/// <summary>
/// Registra los servicios de acceso a datos, mapeo y lógica de negocio en la inyección de dependencias.
/// </summary>
builder.Services.AddScoped<IUsuarioDAO,UsuarioDAO>();
builder.Services.AddScoped<UsuarioMapper>();
builder.Services.AddScoped<CreateUserMapper>();
builder.Services.AddScoped<IUsuarioService,UsuarioService>(); // Aquí agregamos UsuarioService
builder.Services.AddScoped<PasswordResetEmail>();

builder.Services.AddScoped<IProductDAO, ProductDAO>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ProductMapper>();
builder.Services.AddScoped<CreateProductMapper>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
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
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
        )
    };
});


/// <summary>
/// Agrega los servicios necesarios para habilitar los controladores y el modelo MVC.
/// </summary>
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); // Habilita la exploración de los puntos finales para que Swagger pueda generar documentación
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Mi API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa el token JWT así: Bearer token"
    });

   options.AddSecurityRequirement(document =>
{
    var requirement = new OpenApiSecurityRequirement
    {
        {
                new OpenApiSecuritySchemeReference("Bearer", document),
                new List<string>()
        }
    };
    return requirement;
  });
});

var app = builder.Build();

/// <summary>
/// Verifica si el entorno es de desarrollo.
/// </summary>
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Habilita Swagger en el entorno de desarrollo para generar la documentación interactiva
    app.UseSwaggerUI(); // Habilita la interfaz de usuario de Swagger para explorar la API
}


/// <summary>
/// Habilita la redirección HTTPS.
/// </summary>
//app.UseHttpsRedirection();

/// <summary>
/// Habilita la autenticación y autorización.
/// </summary>
app.UseAuthentication();
app.UseAuthorization();

/// <summary>
/// Mapea los controladores.
/// </summary>
app.MapControllers();

/// <summary>
/// Define una ruta GET para la raíz de la API.
/// </summary>
app.MapGet("/", () => "Hola mundo! Nuestra primera API usando C#");
app.Run();