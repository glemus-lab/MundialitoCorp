# Etapa de compilación
# Usamos el SDK de .NET 10 para compilar
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar archivos de proyecto y restaurar dependencias
COPY ["MundialitoCorp.Api/MundialitoCorp.Api.csproj", "MundialitoCorp.Api/"]
COPY ["MundialitoCorp.Application/MundialitoCorp.Application.csproj", "MundialitoCorp.Application/"]
COPY ["MundialitoCorp.Infrastructure/MundialitoCorp.Infrastructure.csproj", "MundialitoCorp.Infrastructure/"]
COPY ["MundialitoCorp.Domain/MundialitoCorp.Domain.csproj", "MundialitoCorp.Domain/"]

# Ejecutar la restauración usando la ruta del proyecto API
RUN dotnet restore "MundialitoCorp.Api/MundialitoCorp.Api.csproj"

# Copiar todo el código fuente
COPY . .

# Cambiar el directorio de trabajo a donde está el proyecto API para compilar
WORKDIR "/src/MundialitoCorp.Api"
RUN dotnet build "MundialitoCorp.Api.csproj" -c Release -o /app/build

# Publicar la aplicación
FROM build AS publish
RUN dotnet publish "MundialitoCorp.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa final (Runtime)
# Usamos la imagen ligera de ASP.NET
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Variables de entorno por defecto
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "MundialitoCorp.Api.dll"]