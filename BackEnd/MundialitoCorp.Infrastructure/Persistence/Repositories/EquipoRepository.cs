using MundialitoCorp.Domain.Entities;
using MundialitoCorp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MundialitoCorp.Infrastructure.Persistence.Repositories
{
    public class EquipoRepository : IEquipoRepository
    {
        private readonly ApplicationDbContext _context;

        public EquipoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Equipo?> GetByIdAsync(Guid id)
        {
            return await _context.Equipos
                .Include(e => e.Jugadores)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task AddAsync(Equipo equipo)
        {
            await _context.Equipos.AddAsync(equipo);
        }

        public void Update(Equipo equipo)
        {
            _context.Equipos.Update(equipo);
        }

        public async Task DeleteAsync(Guid id)
        {
            var equipo = await _context.Equipos.FindAsync(id);
            if (equipo != null)
            {
                _context.Equipos.Remove(equipo);
            }
        }

        public async Task<bool> ExistsAsync(string nombre)
        {
            return await _context.Equipos.AnyAsync(e => e.Nombre == nombre);
        }
    }
}
