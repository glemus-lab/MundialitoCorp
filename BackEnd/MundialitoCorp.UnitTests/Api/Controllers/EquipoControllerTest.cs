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
using MundialitoCorp.Application.Features.Equipos.Commands.CreateEquipo;
using MundialitoCorp.Application.Features.Equipos.Commands.DeleteEquipo;
using MundialitoCorp.Application.Features.Equipos.Commands.UpdateEquipo;
using MundialitoCorp.Application.Features.Equipos.Queries.GetAllEquipos;
using MundialitoCorp.Application.Features.Equipos.Queries.GetEquipoById;
using MundialitoCorp.Application.Features.Equipos.Queries.GetEquiposPaged;
using MundialitoCorp.Application.Models;
using MundialitoCorp.Domain.Common;
using Xunit;

namespace MundialitoCorp.UnitTests.Api.Controllers
{
    public class EquiposControllerTests : ApiTestBase
    {
        public EquiposControllerTests(WebApplicationFactory<Program> factory) : base(factory) { }

        [Fact]
        public async Task GetAllPaged_DebeRetornarOk()
        {
            // Arrange
            MediatorMock.Setup(m => m.Send(It.IsAny<GetEquiposPagedQuery>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Result<PagedList<EquipoPagedReadModel>>.Success(new PagedList<EquipoPagedReadModel>([], 0, 0, 0), 200));

            // Act
            var response = await Client.GetAsync("/api/equipos?page=1&size=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetById_DebeRetornarOk_CuandoExiste()
        {
            // Arrange
            var id = Guid.NewGuid();
            MediatorMock.Setup(m => m.Send(It.IsAny<GetEquipoByIdQuery>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Result<EquipoReadModel>.Success(new EquipoReadModel(id, "Equipo"), 200));

            // Act
            var response = await Client.GetAsync($"/api/equipos/{id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Create_DebeRetornarOk_CuandoEsExitoso()
        {
            // Arrange
            var command = new CreateEquipoCommand("Equipo Test");
            MediatorMock.Setup(m => m.Send(It.IsAny<CreateEquipoCommand>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Result<Guid>.Success(Guid.NewGuid(), 200));

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/equipos")
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
            var tipoControlador = typeof(EquiposController);

            var metodoCreate = tipoControlador.GetMethods()
                .FirstOrDefault(m => m.Name == "Create" && m.GetCustomAttribute<HttpPostAttribute>() != null);

            // Act
            var tieneAtributo = metodoCreate?.GetCustomAttribute<IdempotencyFilterAttribute>();

            // Assert
            metodoCreate.Should().NotBeNull("El método Create debería existir en el controlador.");
            tieneAtributo.Should().NotBeNull("El endpoint POST de creación debe tener el filtro de idempotencia por seguridad.");
        }

        [Fact]
        public async Task Update_DebeRetornarOk_CuandoIdsCoinciden()
        {
            // Arrange
            var id = Guid.NewGuid();
            var command = new UpdateEquipoCommand(id, "Nombre Editado");
            MediatorMock.Setup(m => m.Send(It.IsAny<UpdateEquipoCommand>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Result.Success(200));

            // Act
            var response = await Client.PutAsJsonAsync($"/api/equipos/{id}", command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Update_DebeRetornarBadRequest_CuandoIdsNoCoinciden()
        {
            // Arrange
            var idUrl = Guid.NewGuid();
            var command = new UpdateEquipoCommand(Guid.NewGuid(), "Nombre Editado");

            // Act
            var response = await Client.PutAsJsonAsync($"/api/equipos/{idUrl}", command);
            
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
            MediatorMock.Setup(m => m.Send(It.IsAny<DeleteEquipoCommand>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Result.Success(200));

            // Act
            var response = await Client.DeleteAsync($"/api/equipos/{id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetCatalogo_DebeRetornarOk()
        {
            // Arrange
            MediatorMock.Setup(m => m.Send(It.IsAny<GetAllEquiposQuery>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Result<IEnumerable<EquipoReadModel>>.Success(new List<EquipoReadModel>(), 200));

            // Act
            var response = await Client.GetAsync("/api/equipos/catalogo");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}