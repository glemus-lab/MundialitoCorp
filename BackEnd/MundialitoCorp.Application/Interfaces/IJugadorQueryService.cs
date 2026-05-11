
using MundialitoCorp.Application.Common;
using MundialitoCorp.Application.Models;

namespace MundialitoCorp.Application.Interfaces
{
    public interface IJugadorQueryService
    {
        Task<PagedList<JugadorReadModel>> GetJugadoresPagedAsync(int page, int size, string? nombreFilter, Guid? equipoId);
        Task<JugadorReadModel?> GetByIdAsync(Guid id);
        Task<PagedList<JugadorReadModel>> GetRankingGoleadoresAsync(int pageNumber, int pageSize);
        Task<bool> ExistenTodosLosJugadoresAsync(List<Guid> ids);
    }
}
