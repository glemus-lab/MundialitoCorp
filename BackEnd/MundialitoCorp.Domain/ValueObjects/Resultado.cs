using System.Runtime.InteropServices;
using MundialitoCorp.Domain.Common;

namespace MundialitoCorp.Domain.ValueObjects
{
    public record Resultado
    {
        public int GolesLocal { get; }
        public int GolesVisitante { get; }

        private Resultado(int golesLocal, int golesVisitante)
        {
            GolesLocal = golesLocal;
            GolesVisitante = golesVisitante;
        }

        public static Result<Resultado> Create(int golesLocal, int golesVisitante)
        {
            if (golesLocal < 0 || golesVisitante < 0)
                return Result<Resultado>.Failure("Los goles no pueden ser negativos.", 422);

            return Result<Resultado>.Success(new Resultado(golesLocal, golesVisitante), 200);
        }

        public bool EsEmpate => GolesLocal == GolesVisitante;
        public bool GanoLocal => GolesLocal > GolesVisitante;
        public bool GanoVisitante => GolesVisitante > GolesLocal;
    }
}
