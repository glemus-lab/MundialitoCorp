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
using MundialitoCorp.Application.Features.Jugadores.Commands.CreateJugador;
using MundialitoCorp.Application.Features.Jugadores.Commands.DeleteJugador;
using MundialitoCorp.Application.Features.Jugadores.Commands.UpdateJugador;
using MundialitoCorp.Application.Features.Jugadores.Queries.GetJugadorById;
using MundialitoCorp.Application.Features.Jugadores.Queries.GetJugadoresPaged;
using MundialitoCorp.Application.Features.Jugadores.Queries.GetRankingGoleadores;
using MundialitoCorp.Application.Models;
using MundialitoCorp.Domain.Common;

namespace MundialitoCorp.UnitTests.Api.Controllers
{
    public class JugadoresControllerTests : ApiTestBase
    {
        public JugadoresControllerTests(WebApplicationFactory<Program> factory) : base(factory) { }

        [Fact]
        public async Task GetAllPaged_DebeRetornarOk()
        {
            // Arrange
            MediatorMock.Setup(m => m.Send(It.IsAny<GetJugadoresPagedQuery>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Result<PagedList<JugadorReadModel>>.Success(new PagedList<JugadorReadModel>([], 0, 0, 0), 200));

            // Act
            var response = await Client.GetAsync("/api/jugadores?page=1&size=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetById_DebeRetornarOk_CuandoExiste()
        {
            // Arrange
            var id = Guid.NewGuid();
            MediatorMock.Setup(m => m.Send(It.IsAny<GetJugadorByIdQuery>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Result<JugadorReadModel>.Success(new JugadorReadModel(id, "Nombre", Guid.NewGuid(), "Equipo", 0), 200));

            // Act
            var response = await Client.GetAsync($"/api/jugadores/{id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Create_DebeRetornarOk_CuandoComandoEsValido()
        {
            // Arrange
            var command = new CreateJugadorCommand("Messi", Guid.NewGuid());
            MediatorMock.Setup(m => m.Send(It.IsAny<CreateJugadorCommand>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Result<Guid>.Success(Guid.NewGuid(), 200));

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/jugadores")
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
            // Arrange & Act
            var metodo = typeof(JugadoresController).GetMethods()
                .FirstOrDefault(m => m.Name == "Create" && m.GetCustomAttribute<HttpPostAttribute>() != null);
            var atributo = metodo?.GetCustomAttribute<IdempotencyFilterAttribute>();

            // Assert
            metodo.Should().NotBeNull();
            atributo.Should().NotBeNull();
        }

        [Fact]
        public async Task Update_DebeRetornarOk_CuandoIdsCoinciden()
        {
            // Arrange
            var id = Guid.NewGuid();
            var command = new UpdateJugadorCommand(id, "Nombre Editado");
            MediatorMock.Setup(m => m.Send(It.IsAny<UpdateJugadorCommand>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Result.Success(200));

            // Act
            var response = await Client.PutAsJsonAsync($"/api/jugadores/{id}", command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Update_DebeRetornarBadRequest_CuandoIdsNoCoinciden()
        {
            // Arrange
            var idUrl = Guid.NewGuid();
            var command = new UpdateJugadorCommand(Guid.NewGuid(), "Nombre Editado");

            // Act
            var response = await Client.PutAsJsonAsync($"/api/jugadores/{idUrl}", command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var result = await response.Content.ReadFromJsonAsync<dynamic>();
            string mensaje = result?.GetProperty("errorMessage").GetString()!;
            mensaje.Should().Be("El ID de la URL no coincide con el del cuerpo.");
        }

        [Fact]
        public async Task Delete_DebeRetornarOk()
        {
            // Arrange
            var id = Guid.NewGuid();
            MediatorMock.Setup(m => m.Send(It.IsAny<DeleteJugadorCommand>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Result.Success(200));

            // Act
            var response = await Client.DeleteAsync($"/api/jugadores/{id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetRanking_DebeRetornarOk()
        {
            // Arrange
            MediatorMock.Setup(m => m.Send(It.IsAny<GetRankingGoleadoresQuery>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Result<PagedList<JugadorReadModel>>.Success(new PagedList<JugadorReadModel>([], 0, 0, 0), 200));

            // Act
            var response = await Client.GetAsync("/api/jugadores/ranking?pageNumber=1&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}