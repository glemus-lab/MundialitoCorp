
using MundialitoCorp.Application.Models;

namespace MundialitoCorp.Application.Interfaces
{
    public interface IPosicionesTorneoQueryService
    {
        Task<IEnumerable<TablaPosicionesReadModel>> GetTablaPosicionesAsync();
    }
}
