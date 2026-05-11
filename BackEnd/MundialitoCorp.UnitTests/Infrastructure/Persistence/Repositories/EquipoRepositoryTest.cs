using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MundialitoCorp.Domain.Entities;
using MundialitoCorp.Infrastructure.Persistence.Repositories;

public class EquipoRepositoryTests : RepositoryTestBase
{
    private readonly EquipoRepository _repository;

    public EquipoRepositoryTests()
    {
        _repository = new EquipoRepository(Context);
    }

    [Fact]
    public async Task AddAsync_DebeGuardarEquipoCorrectamente()
    {
        // Arrange
        var equipo = Equipo.Create("Real Madrid").Value;

        // Act
        await _repository.AddAsync(equipo!);
        await Context.SaveChangesAsync();

        // Assert
        var dbEquipo = await Context.Equipos.FindAsync(equipo!.Id);
        dbEquipo.Should().NotBeNull();
        dbEquipo!.Nombre.Should().Be("Real Madrid");
    }

    [Fact]
    public async Task GetByIdAsync_DebeRetornarEquipoConJugadores()
    {
        // Arrange
        var equipo = Equipo.Create("FC Barcelona").Value;
        equipo!.AgregarJugador("Messi");
        Context.Equipos.Add(equipo);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(equipo.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Nombre.Should().Be("FC Barcelona");
        result.Jugadores.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_DebeRetornarNull_SiEquipoNoExiste()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_DebeRetornarTrue_SiNombreExiste()
    {
        // Arrange
        var nombre = "Liverpool";
        Context.Equipos.Add(Equipo.Create(nombre).Value!);
        await Context.SaveChangesAsync();

        // Act
        var exists = await _repository.ExistsAsync(nombre);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task Update_DebeModificarDatosExistentes()
    {
        // Arrange
        var equipo = Equipo.Create("Milan").Value;
        Context.Equipos.Add(equipo!);
        await Context.SaveChangesAsync();

        // Act
        equipo!.CambiarNombre("AC Milan");
        _repository.Update(equipo);
        await Context.SaveChangesAsync();

        // Assert
        var dbEquipo = await Context.Equipos.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == equipo.Id);
        dbEquipo!.Nombre.Should().Be("AC Milan");
    }

    [Fact]
    public async Task DeleteAsync_DebeEliminarEquipo_SiExiste()
    {
        // Arrange
        var equipo = Equipo.Create("Chelsea").Value;
        Context.Equipos.Add(equipo!);
        await Context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(equipo!.Id);
        await Context.SaveChangesAsync();

        // Assert
        var exists = await Context.Equipos.AnyAsync(e => e.Id == equipo.Id);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_NoDebeLanzarExcepcion_SiEquipoNoExiste()
    {
        // Act & Assert
        Func<Task> act = async () => await _repository.DeleteAsync(Guid.NewGuid());
        await act.Should().NotThrowAsync();
    }
}