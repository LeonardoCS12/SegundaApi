using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using apitienda.Models;
using DotNetEnv;

namespace apitienda.Data
{
/// <summary >
/// Contexto de datos para interactuar con la base de datos utilizando Entity
/// Framework Core.
/// </summary >
public class DataContext : DbContext
{
/// <summary >
/// Inicializa una nueva instancia de la clase <see cref="DataContext"/>.
/// </summary >
/// <param name="options">Las opciones de configuración para el contexto de
/// la base de datos.</param >
public DataContext(DbContextOptions <DataContext > options)
: base(options)
{
}

/// <summary >
/// Define una propiedad DbSet para acceder a la tabla de usuarios en la
/// base de datos.
/// </summary >
public DbSet <Usuario > Users { get; set; }

/// <summary >
/// Define una propiedad DbSet para acceder a la tabla de
/// PasswordResetToken en la base de datos.
/// </summary >
public DbSet <PasswordResetToken > passwordresettoken { get; set; }

/// <summary >
/// Define una propiedad DbSet para acceder a la tabla de TokenBlacklist en
/// la base de datos.
/// </summary >
public DbSet <token_blacklist > token_blacklist { get; set; }

/// <summary >
/// Configura la cadena de conexión para la base de datos PostgreSQL.
/// </summary >
/// <param name="optionsBuilder">El constructor de opciones para configurar
/// el contexto de la base de datos.</param >
protected override void OnConfiguring(DbContextOptionsBuilder
optionsBuilder)
{
//Lee el archivo .env
Env.Load();
//obtiene la cadena de coneccion
string connectionString = Env.GetString("CONNECTIONSTRINGS__DEFAULTCONNECTIONUSERS");
optionsBuilder.UseNpgsql(connectionString);
}
}
}
