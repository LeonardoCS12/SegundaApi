using apitienda.Data;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using productos.Data;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using apitienda.Services;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    var corsOrigins = builder.Configuration.GetSection("CorsOrigins").Get<string[]>()
                      ?? new string[] { "http://localhost:5173", "http://localhost:3000", "http://192.168.1.50:3000" };

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("PermitirDominiosEspecificos", policy =>
        {
            policy.WithOrigins(corsOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    Env.Load();

    var connectionStringUser = builder.Configuration.GetConnectionString("DefaultConnectionUsers");
    var connectionStringProducts = builder.Configuration.GetConnectionString("DefaultConnectionProducts");

    builder.Services.AddDbContext<DataContext>(
        options => options.UseNpgsql(connectionStringUser)
    );

    builder.Services.AddDbContext<DataContextProduct>(
        options => options.UseNpgsql(connectionStringProducts)
    );

    builder.Services.AddHttpContextAccessor();

    builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();

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

    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger(); // Habilita Swagger en el entorno de desarrollo para generar la documentación interactiva
        app.UseSwaggerUI(); // Habilita la interfaz de usuario de Swagger para explorar la API
    }

    //app.UseHttpsRedirection();

    app.UseRouting();

    app.UseCors("PermitirDominiosEspecificos");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.MapGet("/", () => "Hola mundo! Nuestra primera API usando C#");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación falló al iniciar.");
}
finally
{
    Log.CloseAndFlush();
}
