using MundialitoCorp.Domain.Entities;
using MundialitoCorp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MundialitoCorp.Infrastructure.Persistence.Repositories
{
    public class PartidoRepository : IPartidoRepository
    {
        private readonly ApplicationDbContext _context;

        public PartidoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Partido?> GetByIdAsync(Guid id)
        {
            return await _context.Partidos.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddAsync(Partido partido)
        {
            await _context.Partidos.AddAsync(partido);
        }

        public void Update(Partido partido)
        {
            _context.Partidos.Update(partido);
        }

        public async Task DeleteAsync(Guid id)
        {
            var partido = await _context.Partidos.FindAsync(id);
            if (partido != null)
            {
                _context.Partidos.Remove(partido);
            }
        }
    }
}
