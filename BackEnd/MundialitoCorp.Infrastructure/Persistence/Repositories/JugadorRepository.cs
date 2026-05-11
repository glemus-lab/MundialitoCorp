using MundialitoCorp.Domain.Entities;
using MundialitoCorp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MundialitoCorp.Infrastructure.Persistence.Repositories
{
    public class JugadorRepository : IJugadorRepository
    {
        private readonly ApplicationDbContext _context;

        public JugadorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Jugador?> GetByIdAsync(Guid id)
        {
            return await _context.Jugadores.FirstOrDefaultAsync(j => j.Id == id);
        }

        public async Task AddAsync(Jugador jugador)
        {
            await _context.Jugadores.AddAsync(jugador);
        }

        public void Update(Jugador jugador)
        {
            _context.Jugadores.Update(jugador);
        }

        public async Task DeleteAsync(Guid id)
        {
            var jugador = await _context.Jugadores.FindAsync(id);
            if (jugador != null)
            {
                _context.Jugadores.Remove(jugador);
            }
        }

        public async Task<IEnumerable<Jugador>> GetByEquipoIdAsync(Guid equipoId)
        {
            return await _context.Jugadores
                .Where(j => j.EquipoId == equipoId)
                .ToListAsync();
        }
    }
}
