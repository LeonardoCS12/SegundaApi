# SegundaApi — API REST de Usuarios y Productos (.NET + C#)

API REST construida con **.NET 10** y **C#**, que implementa dos módulos independientes —**Usuarios** y **Productos**— siguiendo una arquitectura en capas (Modelo → DTO → DAO → Mapper → Service → Controller) sobre **PostgreSQL** con **Entity Framework Core** y **Dapper**.

el proyecto incorpora autenticación con **JWT**, **logging estructurado** con Serilog, un módulo de **auditoría** (en desarrollo) y un proyecto de **pruebas unitarias** con xUnit.

---

## Tabla de contenidos

- [Objetivo](#objetivo)
- [Tecnologías](#tecnologías)
- [Estructura del proyecto](#estructura-del-proyecto)
- [Arquitectura](#arquitectura)
- [Requisitos previos](#requisitos-previos)
- [Docker](#docker)
- [Configuración](#configuración)
  - [CORS (orígenes permitidos)](#cors-orígenes-permitidos)
- [Migraciones y base de datos](#migraciones-y-base-de-datos)
- [Ejecución](#ejecución)
- [Documentación con Swagger](#documentación-con-swagger)
- [Autenticación (JWT)](#autenticación-jwt)
- [Logging](#logging)
- [Auditoría](#auditoría)
- [Endpoints](#endpoints)
  - [Usuarios](#usuarios)
  - [Productos](#productos)
- [Pruebas](#pruebas)

---

## Objetivo

Desarrollar una API REST que permita gestionar **usuarios** y **productos** mediante operaciones CRUD completas (crear, consultar, actualizar y eliminar), aplicando la arquitectura MVC y buenas prácticas de separación de responsabilidades entre las capas de datos, negocio y presentación.

## Tecnologías

- **.NET 10** / ASP.NET Core Web API
- **C#**
- **PostgreSQL** como motor de base de datos
- **Entity Framework Core** (ORM) + **Npgsql**
- **Dapper** (consultas SQL dinámicas para filtros y paginación)
- **BCrypt.Net-Next** (cifrado de contraseñas)
- **JWT** (`Microsoft.AspNetCore.Authentication.JwtBearer` + `System.IdentityModel.Tokens.Jwt`) — autenticación del módulo de Productos
- **Serilog** (consola + archivo JSON con rotación diaria) — logging estructurado
- **Swashbuckle / Swagger** (documentación interactiva)
- **DotNetEnv** (variables de entorno desde `.env`)
- **xUnit / Moq / EF Core InMemory** (pruebas unitarias)
- **Docker / Docker Compose** (contenedores de PostgreSQL y de la API)

## Estructura del proyecto

```
SegundaApi/
├── Program.cs                     # Configuración y arranque de la app
├── Dockerfile                     # Build multi-stage de la API (SDK → runtime)
├── docker-compose.yml             # Contenedores de PostgreSQL y de la API
├── .env.example                   # Plantilla de variables de entorno
├── Common/                        # Compartido entre Users y Products
│   ├── Exceptions/ApiResponse.cs         # Formato estándar de respuesta
│   └── Resources/MessageDictionary.cs    # Diccionario centralizado de mensajes
├── Migrations/                    # Migraciones de EF Core (Usuarios y Productos)
├── Token/                         # Módulo de autenticación JWT
│   ├── AuthService/               # AuthController + AuthService (login/refresh)
│   ├── DTOs/                      # LoginRequestDTO, RefreshRequestDTO, TokenResponse
│   ├── Models/                    # token_blacklist
│   └── Service/                   # JwtService (emisión/validación de tokens)
├── logsauditoria/                 # Módulo de auditoría (en desarrollo, ver sección Auditoría)
│   ├── Models/                    # Auditoria
│   └── Services/                  # AuditoriaService
├── Users/                         # Módulo de Usuarios
│   ├── Controllers/
│   ├── DAO/
│   ├── Data/
│   ├── DTO/
│   ├── Exceptions/
│   ├── Mappers/
│   ├── Methods/
│   ├── Models/
│   └── Services/
├── Products/                      # Módulo de Productos
│   ├── Controladores/
│   ├── DAO/
│   ├── DATA/
│   ├── DTO/
│   ├── Exception/
│   ├── Mappers/
│   ├── Metodos/
│   ├── Models/
│   └── Services/
└── ApiTienda.Tests/                # Pruebas unitarias (xUnit + Moq + EF Core InMemory)
```

## Arquitectura

Cada entidad (Usuario y Producto) implementa el mismo patrón de capas, de forma independiente entre sí:

```
Controller → Service → DAO → DataContext (EF Core) → PostgreSQL
                │
                └── Mapper (traduce entre Modelo y DTO)
```

- **Model**: representa la tabla tal como vive en la base de datos.
- **DTO**: define qué datos se exponen al cliente (entrada/salida), sin acoplarse al modelo interno.
- **DAO**: única capa que ejecuta consultas contra la base de datos (EF Core y Dapper).
- **Mapper**: convierte entre Modelo y DTO en ambas direcciones.
- **Service**: contiene la lógica de negocio (validaciones, reglas, orquestación).
- **Controller**: expone los endpoints HTTP y delega en el Service.

Usuarios y Productos son módulos de negocio **totalmente independientes**: no existe relación ni comunicación entre sus Services o DAOs.

Los módulos **Token** (autenticación) y **logsauditoria** (auditoría) son transversales: no manejan una entidad de negocio propia, así que no siguen el mismo patrón de capas completo (por ejemplo, `logsauditoria` solo tiene Model + Service, sin Controller ni DAO propios).

## Requisitos previos

- SDK de [.NET 10](https://dotnet.microsoft.com/)
- [Docker](https://www.docker.com/) y [Docker Compose](https://docs.docker.com/compose/) (recomendado), **o** PostgreSQL instalado localmente
- Herramientas de EF Core:
  ```bash
  dotnet add package Microsoft.EntityFrameworkCore.Tools
  dotnet add package Microsoft.EntityFrameworkCore.Design
  dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
  ```

## Docker

El proyecto incluye un `docker-compose.yml` con **dos servicios**:

```yaml
services:
  db:
    image: postgres:15
    container_name: postgres_dbusers
    restart: always
    env_file:
      - .env
    environment:
      POSTGRES_USER: ${DB_USER}
      POSTGRES_PASSWORD: ${DB_PASSWORD}
      POSTGRES_DB: ${DB_NAME}
    ports:
      - "${DB_PORT}:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

  web_api:
    build:
      context: .
      dockerfile: Dockerfile
    restart: always
    ports:
      - "5001:8080"
    environment:
      ASPNETCORE_ENVIRONMENT: Development
    depends_on:
      - db
    env_file:
      - .env

volumes:
  postgres_data:
```

- **`db`**: PostgreSQL 15. Internamente siempre escucha en el puerto `5432`; hacia afuera se publica en el puerto que definas en `DB_PORT` (ver [Configuración](#configuración)).
- **`web_api`**: compila y corre la API a partir del `Dockerfile` (build multi-stage: `sdk:10.0` para compilar, `aspnet:10.0` para ejecutar). Queda disponible en `http://localhost:5001`, con `ASPNETCORE_ENVIRONMENT=Development` para que Swagger siga habilitado.
- Ambos servicios leen sus variables desde el mismo archivo `.env` (`env_file`).

Antes de levantar los contenedores asegúrate de tener el `.env` creado en la raíz del proyecto (ver [Configuración](#configuración)).

Para levantar todo (Postgres + API):

```bash
docker compose up -d --build
```

Para levantar solo la base de datos y correr la API localmente con `dotnet run`:

```bash
docker compose up -d db
```

Para verificar que están corriendo:

```bash
docker ps
```

Para detener los contenedores:

```bash
docker compose down
```

> El volumen `postgres_data` persiste los datos aunque los contenedores se detengan o reinicien. Si necesitas borrar todo y empezar de cero: `docker compose down -v`.

## Configuración

Crea un archivo `.env` en la raíz del proyecto (no se sube al repositorio) con el siguiente contenido. Puedes basarte en `.env.example`:

```env
# --- CONFIGURACIÓN DE BASE DE DATOS (DOCKER) ---
# Estos valores los usa docker-compose.yml para crear el contenedor de PostgreSQL
DB_USER=tu_usuario
DB_PASSWORD=tu_contraseña
DB_NAME=apitienda_db
DB_PORT=5433
# ^ puerto publicado en el HOST; dentro del contenedor Postgres siempre escucha en 5432

# --- CADENAS DE CONEXIÓN (.NET) ---
# Usa 'localhost' si la API corre fuera de Docker (dotnet run).
# Si la API corre dentro de docker-compose, usa el nombre del servicio ('db') como Host.
ConnectionStrings__DefaultConnectionUsers="Host=localhost;Port=5433;Database=apitienda_db;Username=tu_usuario;Password=tu_contraseña"
ConnectionStrings__DefaultConnectionProducts="Host=localhost;Port=5433;Database=apitienda_db;Username=tu_usuario;Password=tu_contraseña"
```
L.

La configuración de JWT (`Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:AccessTokenMinutes`, `Jwt:RefreshTokenDays`) vive en `appsettings.json`. El valor de ejemplo de `Jwt:Key` sirve para desarrollo local; en cualquier entorno real conviene sobreescribirlo mediante variables de entorno o *user-secrets* en lugar de dejarlo en el archivo versionado.

### CORS (orígenes permitidos)

Los orígenes permitidos se leen de la sección `CorsOrigins` en `appsettings.Development.json`:

```json
"CorsOrigins": [
  "http://localhost:3000",
  "http://localhost:5173",
  "http://192.168.1.50:3000"
]
```

Si esa sección no está presente, `Program.cs` usa como valores por defecto `http://localhost:5173`, `http://localhost:3000` y `http://192.168.1.50:3000`. Agrega aquí cualquier origen adicional (por ejemplo, el dominio de tu frontend) antes de desplegar.

## Migraciones y base de datos

Con la base de datos levantada (vía Docker Compose o PostgreSQL local), aplica las migraciones para cada contexto:

```bash
# Migraciones de Usuarios
dotnet ef migrations add InitialCreateUsuarios --context DataContext --output-dir Migrations
dotnet ef database update --context DataContext

# Migraciones de Productos
dotnet ef migrations add InitialCreateProductos --context DataContextProduct --output-dir Migrations/DataContextProductMigrations
dotnet ef database update --context DataContextProduct

# Migración de la tabla token_blacklist (autenticación JWT)
dotnet ef migrations add AddTokenBlacklist --context DataContext --output-dir Migrations
dotnet ef database update --context DataContext
```

> La tabla `auditoria` (módulo de auditoría) todavía no tiene migración generada — ver [Auditoría](#auditoría).

## Ejecución

```bash
dotnet run
```

La terminal mostrará la URL en la que corre la API, por ejemplo:

```
Now listening on: https://localhost:7168
```

Al abrir esa URL en el navegador verás el endpoint raíz:

```
Hola mundo! Nuestra primera API usando C#
```

Alternativamente, puedes correr todo en contenedores con `docker compose up -d --build` (ver [Docker](#docker)); en ese caso la API queda disponible en `http://localhost:5001`.

## Documentación con Swagger

Con la API corriendo, agrega `/swagger/index.html` a la URL para explorar y probar todos los endpoints desde una interfaz interactiva:

```
https://localhost:<puerto>/swagger/index.html
```

Swagger solo está disponible en el entorno de **Development**.

## Autenticación (JWT)

La API protege el módulo de **Productos** mediante autenticación con **JSON Web Tokens**. El flujo es:

1. `POST /auth/login` con `email` y `password` → devuelve un `access token` (corta duración, 15 min) y un `refresh token` (larga duración, 7 días).
2. En Swagger, botón **Authorize** → pegar el `access token` → todos los endpoints con candado quedan habilitados.
3. Cuando el `access token` expira, `POST /auth/refresh` con el `refresh token` genera un nuevo par de tokens (y revoca el `refresh token` anterior guardándolo en la tabla `token_blacklist`).

Los tiempos de expiración y la clave de firma se configuran en `appsettings.json` bajo la sección `Jwt` (ver [Configuración](#configuración)).

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/auth/login` | Inicia sesión y genera access + refresh token |
| POST | `/auth/refresh` | Renueva los tokens a partir de un refresh token válido |

## Logging

El proyecto usa **Serilog** para logging estructurado, configurado en dos etapas:

- Un *bootstrap logger* mínimo en consola, activo desde el arranque de `Program.cs` (útil para capturar errores de configuración antes de que la app termine de inicializar).
- La configuración completa, leída desde la sección `Serilog` de `appsettings.json`, que:
  - escribe en **consola** en formato JSON;
  - escribe en **archivo**, con rotación diaria, en `logs/log-tecnico-.json`;
  - enriquece cada entrada con el nombre de máquina y el hilo (`WithMachineName`, `WithThreadId`).
- `app.UseSerilogRequestLogging()` registra automáticamente cada solicitud HTTP entrante.

Si necesitas más o menos verbosidad, ajusta `Serilog:MinimumLevel` o los `WriteTo` en `appsettings.json`.

## Auditoría


El proyecto incluye un módulo `logsauditoria/` pensado para registrar quién hizo qué, sobre qué tabla y desde qué IP:

- **Modelo** `Auditoria` (tabla `auditoria`): `Usuario`, `Accion`, `Tabla`, `RegistroId`, `Detalle`, `Fecha`, `IpAddress`.
- **`IAuditoriaService` / `AuditoriaService`**: expone `RegistrarLog(accion, tabla, registroId, detalle)`, que toma el usuario autenticado y la IP del request actual y guarda el registro en la base de datos.
- El servicio ya está registrado en el contenedor de dependencias (`builder.Services.AddScoped<IAuditoriaService, AuditoriaService>()` en `Program.cs`).

Lo que falta para que quede operativo:

1. Generar la migración de la tabla `auditoria` (aún no existe):
   ```bash
   dotnet ef migrations add AddAuditoria --context DataContext --output-dir Migrations
   dotnet ef database update --context DataContext
   ```
2. Inyectar `IAuditoriaService` en los Services de Usuarios/Productos (o en un middleware/filtro) y llamar a `RegistrarLog(...)` en las operaciones que se quieran auditar (crear, actualizar, eliminar, etc.).

## Endpoints

### Usuarios

Base: `api/users`

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/` | Lista usuarios con paginación, orden y filtros |
| GET | `/{id}` | Obtiene un usuario por ID |
| POST | `/` | Crea un usuario nuevo |
| PUT | `/{id}` | Actualiza completamente un usuario |
| PATCH | `/{id}` | Actualiza parcialmente un usuario |
| DELETE | `/{id}` | Elimina un usuario (borrado lógico) |
| PATCH | `/{id}/restore` | Restaura un usuario eliminado |
| PATCH | `/{id}/activate` | Activa un usuario |
| PATCH | `/{id}/deactivate` | Desactiva un usuario |
| POST | `/{id}/verify-email` | Verifica el correo del usuario |
| POST | `/{id}/change-password` | Cambia la contraseña (requiere la actual) |
| POST | `/request-password-reset` | Inicia el flujo de recuperación de contraseña |
| POST | `/reset-password` | Completa la recuperación con el token recibido |

### Productos

Base: `api/products` — todos los endpoints requieren autenticación **Bearer** (ver [Autenticación (JWT)](#autenticación-jwt)).

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/products` | Lista productos con paginación, orden y filtros |
| GET | `/products/{id}` | Obtiene un producto por ID |
| POST | `/products` | Crea un producto nuevo |
| PUT | `/products/{id}` | Actualiza completamente un producto |
| PATCH | `/products/{id}` | Actualiza parcialmente un producto |
| DELETE | `/products/{id}` | Elimina un producto (borrado lógico) |
| PATCH | `/products/{id}/restore` | Restaura un producto eliminado |
| GET | `/products/search` | Busca productos por nombre o descripción |
| GET | `/products/{min_price}/{max_price}` | Filtra productos por rango de precio |
| PATCH | `/products/{id}/update-image` | Actualiza la imagen del producto |
| PATCH | `/products/{id}/deactivate` | Desactiva un producto |
| PATCH | `/products/{id}/activate` | Activa un producto |

## Pruebas

El proyecto incluye `ApiTienda.Tests`, un proyecto de pruebas unitarias con **xUnit**, **Moq** y **EF Core InMemory**, que cubre la lógica de `UsuarioService` y `ProductService`.

```bash
dotnet test
```
