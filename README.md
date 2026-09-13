# SegundaApi — API REST de Usuarios y Productos (.NET + C#)

API REST construida con **.NET 10** y **C#**, que implementa dos módulos independientes —**Usuarios** y **Productos**— siguiendo una arquitectura en capas (Modelo → DTO → DAO → Mapper → Service → Controller) sobre **PostgreSQL** con **Entity Framework Core** y **Dapper**.

Este proyecto es la continuación de la práctica *PrimeraApi* (gestión de Usuarios), a la que se le agrega un segundo módulo de negocio completo: **Productos**.

---

## Tabla de contenidos

- [Objetivo](#objetivo)
- [Tecnologías](#tecnologías)
- [Estructura del proyecto](#estructura-del-proyecto)
- [Arquitectura](#arquitectura)
- [Requisitos previos](#requisitos-previos)
- [Configuración](#configuración)
- [Migraciones y base de datos](#migraciones-y-base-de-datos)
- [Ejecución](#ejecución)
- [Documentación con Swagger](#documentación-con-swagger)
- [Endpoints](#endpoints)
  - [Usuarios](#usuarios)
  - [Productos](#productos)

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
- **Swashbuckle / Swagger** (documentación interactiva)
- **DotNetEnv** (variables de entorno desde `.env`)

## Estructura del proyecto

```
SegundaApi/
├── Program.cs                     # Configuración y arranque de la app
├── Common/                        # Compartido entre Users y Products
│   ├── Exceptions/ApiResponse.cs         # Formato estándar de respuesta
│   └── Resources/MessageDictionary.cs    # Diccionario centralizado de mensajes
├── Migrations/                    # Migraciones de EF Core (Usuarios y Productos)
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
└── Products/                      # Módulo de Productos
    ├── Controladores/
    ├── DAO/
    ├── DATA/
    ├── DTO/
    ├── Exception/
    ├── Mappers/
    ├── Metodos/
    ├── Models/
    └── Services/
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

## Requisitos previos

- SDK de [.NET 10](https://dotnet.microsoft.com/)
- [PostgreSQL](https://www.postgresql.org/) instalado localmente, **o** Docker + Docker Compose
- Herramientas de EF Core:
  ```bash
  dotnet add package Microsoft.EntityFrameworkCore.Tools
  dotnet add package Microsoft.EntityFrameworkCore.Design
  dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
  ```

## Configuración

Crea un archivo `.env` en la raíz del proyecto (no se sube al repositorio) con el siguiente contenido:

```env
# Datos de la base de datos
DB_USER=tu_usuario
DB_PASSWORD=tu_contraseña
DB_NAME=apitienda_dbdos
DB_PORT=5432

# Cadenas de conexión para .NET
ConnectionStrings__DefaultConnectionUsers="Host=localhost;Port=5432;Database=apitienda_dbdos;Username=tu_usuario;Password=tu_contraseña"
ConnectionStrings__DefaultConnectionProducts="Host=localhost;Port=5432;Database=apitienda_dbdos;Username=tu_usuario;Password=tu_contraseña"
```

> ⚠️ El `.env` contiene credenciales y está incluido en `.gitignore`. Nunca debe subirse al repositorio.

## Migraciones y base de datos

Con la base de datos levantada (PostgreSQL local o vía Docker), aplica las migraciones para cada contexto:

```bash
# Migraciones de Usuarios
dotnet ef migrations add InitialCreateUsuarios --context DataContext --output-dir Migrations
dotnet ef database update --context DataContext

# Migraciones de Productos
dotnet ef migrations add InitialCreateProductos --context DataContextProduct --output-dir Migrations/DataContextProductMigrations
dotnet ef database update --context DataContextProduct
```

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

## Documentación con Swagger

Con la API corriendo, agrega `/swagger/index.html` a la URL para explorar y probar todos los endpoints desde una interfaz interactiva:

```
https://localhost:<puerto>/swagger/index.html
```

Swagger solo está disponible en el entorno de **Development**.

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

Base: `api/products`

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
