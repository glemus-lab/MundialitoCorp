
namespace MundialitoCorp.Application.Models
{
    public record EquipoReadModel(Guid Id, string Nombre);
    public record EquipoPagedReadModel(Guid Id, string Nombre, int Puntos, int PartidosJugados);

    public record JugadorReadModel(Guid Id, string Nombre, Guid EquipoId, string EquipoNombre, int GolesAnotados);

    public record PartidoReadModel(Guid Id, string Local, string Visitante, Guid EquipoLocalId, Guid EquipoVisitanteId, int? GolesL, int? GolesV, DateOnly Fecha, bool EstaFinalizado);

    public record PartidoDetalleReadModel(Guid EquipoLocalId, string EquipoLocal, Guid EquipoVisitanteId, string EquipoVisitante, int GolesLocal, int GolesVisitante, DateOnly Fecha, List<GoleadorReadModel> Goleadores);

    public record GoleadorReadModel(Guid JugadorId, string NombreJugador, Guid EquipoId);

    public record TablaPosicionesReadModel(Guid Id, string Nombre, int PartidosJugados, int PartidosGanados, int PartidosPerdidos, int PartidosEmpatados, int Puntos, int GolesFavor, int GolesContra, int DiferenciaGoles);
}
