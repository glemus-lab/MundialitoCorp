using MundialitoCorp.Application.Common;
using MundialitoCorp.Application.Models;

namespace MundialitoCorp.Application.Interfaces
{
    public interface IPartidoQueryService
    {
        Task<PagedList<PartidoReadModel>> GetPartidosPagedAsync(int page, int size, string? sortBy, string? sortDirection, DateTime? fecha, Guid? equipoId, bool? finalizado);
        Task<PartidoDetalleReadModel?> GetByIdAsync(Guid id);
        Task<bool> ExisteConflictoFechaAsync(Guid equipoId, DateOnly fecha);
        Task<IEnumerable<PartidoReadModel>> GetPartidosPendientesAsync();
        Task<PagedList<PartidoReadModel>> GetHistorialPartidosAsync(int pageNumber, int pageSize);
    }
}
