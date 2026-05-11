using MundialitoCorp.Domain.Common;

namespace MundialitoCorp.Domain.Entities
{
    public class PartidoGol
    {
        public Guid Id { get; private set; }
        public Guid PartidoId { get; private set; }

        public Guid JugadorId { get; private set; }

        private PartidoGol() { }

        private PartidoGol(Guid partidoId, Guid jugadorId)
        {
            Id = Guid.NewGuid();
            PartidoId = partidoId;
            JugadorId = jugadorId;
        }

        public static Result<PartidoGol> Create(Guid partidoId, Guid jugadorId)
        {
            if (partidoId == Guid.Empty)
                return Result<PartidoGol>.Failure("El Id del partido es requerido.", 422);

            if (jugadorId == Guid.Empty)
                return Result<PartidoGol>.Failure("El Id del jugador es requerido.", 422);

            return Result<PartidoGol>.Success(new PartidoGol(partidoId, jugadorId), 200);
        }
    }
}
