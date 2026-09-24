# ==========================================
# Etapa 1: Compilación y Publicación
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["SegundaApi.csproj", "./"]
RUN dotnet restore "./SegundaApi.csproj"

# Copiar todo el código 
COPY . .

# Compilar en modo Release
RUN dotnet publish "SegundaApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ==========================================
# Etapa 2: Entorno de Ejecución
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "SegundaApi.dll"]
