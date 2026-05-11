using MundialitoCorp.Application.Interfaces;
using MundialitoCorp.Application.Models;
using MundialitoCorp.Infrastructure.Persistence.Dapper;
using Dapper;

namespace MundialitoCorp.Infrastructure.Persistence.Queries
{
    public class PosicionesTorneoQueryService : IPosicionesTorneoQueryService
    {
        private readonly IDapperContext _dapper;

        public PosicionesTorneoQueryService(IDapperContext dapper) => _dapper = dapper;

        public async Task<IEnumerable<TablaPosicionesReadModel>> GetTablaPosicionesAsync()
        {            
            const string sql = @"
            SELECT
                Id,
                Nombre, 
                PartidosJugados,
                PartidosGanados,
                PartidosPerdidos,
                PartidosEmpatados,
                Puntos, 
                GolesFavor, 
                GolesContra, 
                DiferenciaGoles
            FROM Equipos
            ORDER BY Puntos DESC, DiferenciaGoles DESC, GolesFavor DESC";

            using var connection = _dapper.CreateConnection();
            return await connection.QueryAsync<TablaPosicionesReadModel>(sql);
        }
    }
}
