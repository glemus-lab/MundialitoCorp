using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using MundialitoCorp.Api.Controllers;
using MundialitoCorp.Api.Filters;
using MundialitoCorp.Application.Common;
using MundialitoCorp.Application.Features.Partidos.Commands.CreatePartido;
using MundialitoCorp.Application.Features.Partidos.Commands.DeletePartido;
using MundialitoCorp.Application.Features.Partidos.Commands.RegistrarResultado;
using MundialitoCorp.Application.Features.Partidos.Commands.UpdateFechaPartido;
using MundialitoCorp.Application.Features.Partidos.Queries.GetHistorialPartidos;
using MundialitoCorp.Application.Features.Partidos.Queries.GetPartidoById;
using MundialitoCorp.Application.Features.Partidos.Queries.GetPartidosPendientes;
using MundialitoCorp.Application.Models;
using MundialitoCorp.Domain.Common;

namespace MundialitoCorp.UnitTests.Api.Controllers
{
    public class PartidosControllerTests : ApiTestBase
    {
        public PartidosControllerTests(WebApplicationFactory<Program> factory) : base(factory) { }

        [Fact]
        public async Task GetById_DebeRetornarOk()
        {
            // Arrange
            var id = Guid.NewGuid();
            MediatorMock.Setup(m => m.Send(It.IsAny<GetPartidoByIdQuery>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Result<PartidoDetalleReadModel>.Success(new(Guid.NewGuid(), "Local", Guid.NewGuid(), "Visitante", 0, 0, DateOnly.FromDateTime(DateTime.Now), []), 200));

            // Act
            var response = await Client.GetAsync($"/api/partidos/{id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Create_DebeRetornarOk_CuandoComandoEsValido()
        {
            // Arrange
            var command = new CreatePartidoCommand(Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Now));
            MediatorMock.Setup(m => m.Send(It.IsAny<CreatePartidoCommand>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Result<Guid>.Success(Guid.NewGuid(), 200));

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/partidos")
            {
                Content = JsonContent.Create(command)
            };
            request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public void Create_DebeTenerAtributoIdempotencia()
        {
            // Arrange
            var metodo = typeof(PartidosController).GetMethods()
                .FirstOrDefault(m => m.Name == "Create" && m.GetCustomAttribute<HttpPostAttribute>() != null);
            
            // Act
            var atributo = metodo?.GetCustomAttribute<IdempotencyFilterAttribute>();

            // Assert
            metodo.Should().NotBeNull();
            atributo.Should().NotBeNull();
        }

        [Fact]
        public async Task Delete_DebeRetornarOk()
        {
            // Arrange
            var id = Guid.NewGuid();
            MediatorMock.Setup(m => m.Send(It.IsAny<DeletePartidoCommand>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Result.Success(200));

            // Act
            var response = await Client.DeleteAsync($"/api/partidos/{id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task RegistrarResultado_DebeRetornarOk()
        {
            // Arrange
            var command = new RegistrarResultadoCommand(Guid.NewGuid(), 2, 1, [], []);
            MediatorMock.Setup(m => m.Send(It.IsAny<RegistrarResultadoCommand>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Result.Success(200));

            // Act
            var response = await Client.PatchAsJsonAsync("/api/partidos/registrar-resultado", command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task UpdateFecha_DebeRetornarOk_CuandoIdsCoinciden()
        {
            // Arrange
            var id = Guid.NewGuid();
            var command = new UpdateFechaPartidoCommand(id, DateOnly.FromDateTime(DateTime.Now));
            MediatorMock.Setup(m => m.Send(It.IsAny<UpdateFechaPartidoCommand>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Result.Success(200));

            // Act
            var response = await Client.PatchAsJsonAsync($"/api/partidos/{id}/fecha", command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task UpdateFecha_DebeRetornarBadRequest_CuandoIdsNoCoinciden()
        {
            // Arrange
            var idUrl = Guid.NewGuid();
            var command = new UpdateFechaPartidoCommand(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Now));

            // Act
            var response = await Client.PatchAsJsonAsync($"/api/partidos/{idUrl}/fecha", command);
                        
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var result = await response.Content.ReadFromJsonAsync<dynamic>();
            string mensaje = result?.GetProperty("errorMessage").GetString()!;
            mensaje.Should().Be("El ID de la URL no coincide con el del cuerpo.");
        }

        [Fact]
        public async Task GetPendientes_DebeRetornarOk()
        {
            // Arrange
            MediatorMock.Setup(m => m.Send(It.IsAny<GetPartidosPendientesQuery>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Result<List<PartidoReadModel>>.Success([], 200));

            // Act
            var response = await Client.GetAsync("/api/partidos/pendientes");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetHistorial_DebeRetornarOk()
        {
            // Arrange
            MediatorMock.Setup(m => m.Send(It.IsAny<GetHistorialPartidosQuery>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Result<PagedList<PartidoReadModel>>.Success(new([], 0, 0, 0), 200));

            // Act
            var response = await Client.GetAsync("/api/partidos/historial?page=1&size=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}