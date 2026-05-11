
using MundialitoCorp.Application.Common;
using MundialitoCorp.Application.Models;

namespace MundialitoCorp.Application.Interfaces
{
    public interface IEquipoQueryService
    {
        Task<PagedList<EquipoPagedReadModel>> GetEquiposPagedAsync(int page, int size, string? sortBy, string? sortDirection, string? filter);
        Task<IEnumerable<EquipoReadModel>> GetAllAsync();
        Task<EquipoReadModel?> GetByIdAsync(Guid id);
    }
}