# MundialitoCorp - Plataforma de Gestión de Torneos

Solución integral para la gestión de torneos de fútbol, implementada con **Clean Architecture**, **CQRS**, y **Domain-Driven Design (DDD)**.

## Tecnologías Principales

- **Backend:** .NET 10, Entity Framework Core, Dapper, MediatR, FluentValidation.
- **Frontend:** Next.js 16, TypeScript, Tailwind CSS.
- **Base de Datos:** SQL Server (Producción/Docker) y SQLite (Tests).
- **Infraestructura:** Docker & Docker Compose.
- **Calidad:** xUnit, Moq, FluentAssertions.

## Arquitectura

El sistema sigue los principios de **Clean Architecture** dividiéndose en:
- **Domain:** Entidades, Objetos de Valor y Reglas de Negocio.
- **Application:** Casos de uso (Commands/Queries) y lógica de MediatR.
- **Infrastructure:** Implementación de persistencia (EF Core & Dapper) y servicios externos.
- **Api:** Controladores REST y Middleware de trazabilidad.

## Cómo Ejecutar (Docker)

La forma más rápida de levantar el entorno completo (Base de datos, API y Web) es mediante Docker Compose:

1. Clonar el repositorio.
2. En la raíz del proyecto, ejecutar:
   ```bash
   docker-compose up --build
   ```
3. Acceder a las aplicaciones:
   - **Frontend:** [http://localhost:3000](http://localhost:3000)
   - **Backend API:** [http://localhost:8080/api](http://localhost:8080/api)

## Pruebas

Para ejecutar la suite completa de pruebas unitarias y de integración:
```bash
dotnet test
```

## Postman

Se incluye una colección de Postman en la raíz del proyecto (`MundialitoCorp Api.postman_collection.json`) con todos los endpoints configurados para facilitar las pruebas de la API.

## Características Destacadas

- **Idempotencia:** Implementada mediante filtros de acción para prevenir duplicidad en comandos críticos.
- **Trazabilidad:** Middleware de manejo de excepciones y log de trazabilidad con `CorrelationId`.
- **CQRS:** Separación de lectura (Dapper) y escritura (EF Core) para máximo rendimiento.