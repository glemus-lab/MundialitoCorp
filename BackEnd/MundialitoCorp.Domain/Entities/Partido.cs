using MundialitoCorp.Domain.Common;
using MundialitoCorp.Domain.Events;
using MundialitoCorp.Domain.ValueObjects;

namespace MundialitoCorp.Domain.Entities
{
    public class Partido : Entity
    {
        public Guid EquipoLocalId { get; private set; }
        public Guid EquipoVisitanteId { get; private set; }
        public int? GolesLocal { get; private set; }
        public int? GolesVisitante { get; private set; }
        public DateOnly Fecha { get; private set; }
        public bool EstaFinalizado { get; private set; }

        private readonly List<PartidoGol> _golesDetalle = new();
        public IReadOnlyCollection<PartidoGol> GolesDetalle => _golesDetalle.AsReadOnly();

        private Partido() { }

        private Partido(Guid localId, Guid visitanteId, DateOnly fecha)
        {
            Id = Guid.NewGuid();
            EquipoLocalId = localId;
            EquipoVisitanteId = visitanteId;
            Fecha = fecha;
            EstaFinalizado = false;
        }

        public static Result<Partido> Create(Guid localId, Guid visitanteId, DateOnly fecha)
        {
            if (localId == visitanteId)
                return Result<Partido>.Failure("Los equipos rivales no puede ser el mismo.", 422);

            return Result<Partido>.Success(new Partido(localId, visitanteId, fecha), 200);
        }

        public Result RegistrarResultado(Resultado resultado, List<Guid> goleadoresL, List<Guid> goleadoresV)
        {
            if (Fecha > DateOnly.FromDateTime(DateTime.Now))
                return Result.Failure("No se pueden registrar resultado de un partido que no se ha disputado.", 422);

            if (resultado.GolesLocal != goleadoresL.Count)
                return Result.Failure("Los goles del local debe coincidir con los goleadores.", 422);

            if (resultado.GolesVisitante != goleadoresV.Count)
                return Result.Failure("Los goles del visitante debe coincidir con los goleadores.", 422);

            if (EstaFinalizado)
                return Result.Failure("El partido ya ha sido finalizado previamente.", 422);
            
            GolesLocal = resultado.GolesLocal;
            GolesVisitante = resultado.GolesVisitante;
            EstaFinalizado = true;

            var goleadores = goleadoresL.Concat(goleadoresV);

            _golesDetalle.Clear();
            foreach (var jugadorId in goleadores)
            {
                var resultPartidoGol = PartidoGol.Create(Id, jugadorId);

                if (resultPartidoGol.IsFailure)
                    return Result.Failure(resultPartidoGol.ErrorMessage, resultPartidoGol.Code, resultPartidoGol.Errors);

                _golesDetalle.Add(resultPartidoGol.Value!);
            }
                        
            AddDomainEvent(new ResultadoRegistradoEvent(Id, EquipoLocalId, EquipoVisitanteId, resultado.GolesLocal, resultado.GolesVisitante, goleadoresL, goleadoresV));

            return Result.Success(200);
        }

        public Result UpdateFecha(DateOnly nuevaFecha)
        {
            if (EstaFinalizado)
                return Result.Failure("No se puede modificar la fecha de un partido que ya finalizó.", 422);

            Fecha = nuevaFecha;
            return Result.Success(200);
        }
    }
}
