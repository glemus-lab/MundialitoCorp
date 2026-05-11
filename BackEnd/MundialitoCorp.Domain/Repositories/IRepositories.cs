using MundialitoCorp.Domain.Entities;

namespace MundialitoCorp.Domain.Repositories
{
    public interface IEquipoRepository
    {
        Task<Equipo?> GetByIdAsync(Guid id);
        Task AddAsync(Equipo equipo);
        void Update(Equipo equipo);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(string nombre);
    }

    public interface IPartidoRepository
    {
        Task<Partido?> GetByIdAsync(Guid id);
        Task AddAsync(Partido partido);
        void Update(Partido partido);
        Task DeleteAsync(Guid id);
    }

    public interface IJugadorRepository
    {
        Task<Jugador?> GetByIdAsync(Guid id);
        Task AddAsync(Jugador jugador);
        void Update(Jugador jugador);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<Jugador>> GetByEquipoIdAsync(Guid equipoId);
    }
}
