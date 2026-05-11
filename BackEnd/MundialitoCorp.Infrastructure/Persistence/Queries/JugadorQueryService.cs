using MundialitoCorp.Application.Common;
using MundialitoCorp.Application.Interfaces;
using MundialitoCorp.Application.Models;
using MundialitoCorp.Infrastructure.Persistence.Dapper;
using Dapper;

namespace MundialitoCorp.Infrastructure.Persistence.Queries
{
    public class JugadorQueryService : IJugadorQueryService
    {
        private readonly IDapperContext _dapper;

        public JugadorQueryService(IDapperContext dapper)
        {
            _dapper = dapper;
        }

        public async Task<PagedList<JugadorReadModel>> GetJugadoresPagedAsync(int page, int size, string? nombreFilter, Guid? equipoId)
        {
            using var connection = _dapper.CreateConnection();

            string sql = $@"
                SELECT j.Id, j.Nombre, j.EquipoId, e.Nombre AS EquipoNombre, j.GolesAnotados
                FROM Jugadores j
                INNER JOIN Equipos e ON j.EquipoId = e.Id
                WHERE (@Nombre IS NULL OR j.Nombre LIKE @Nombre)
                  AND (@EquipoId IS NULL OR j.EquipoId = @EquipoId)
                ORDER BY j.Nombre ASC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT COUNT(*) FROM Jugadores j WHERE (@Nombre IS NULL OR j.Nombre LIKE @Nombre) AND (@EquipoId IS NULL OR j.EquipoId = @EquipoId);";

            using var multi = await connection.QueryMultipleAsync(sql, new
            {
                Nombre = string.IsNullOrEmpty(nombreFilter) ? null : $"%{nombreFilter}%",
                EquipoId = equipoId,
                Offset = (page - 1) * size,
                PageSize = size
            });

            var items = (await multi.ReadAsync<JugadorReadModel>()).ToList();
            var total = await multi.ReadFirstAsync<int>();

            return new PagedList<JugadorReadModel>(items, total, page, size);
        }

        public async Task<JugadorReadModel?> GetByIdAsync(Guid id)
        {
            using var connection = _dapper.CreateConnection();

            const string sql = @"
            SELECT j.Id, j.Nombre, j.EquipoId, e.Nombre AS EquipoNombre, j.GolesAnotados
            FROM Jugadores j
            INNER JOIN Equipos e ON j.EquipoId = e.Id
            WHERE j.Id = @Id";

            return await connection.QueryFirstOrDefaultAsync<JugadorReadModel>(sql, new { Id = id });
        }

        public async Task<PagedList<JugadorReadModel>> GetRankingGoleadoresAsync(int pageNumber, int pageSize)
        {
            using var connection = _dapper.CreateConnection();
            const string sql = @"
                    SELECT j.Id, j.Nombre, j.EquipoId, e.Nombre AS EquipoNombre, j.GolesAnotados
                    FROM Jugadores j
                    INNER JOIN Equipos e ON j.EquipoId = e.Id
                    WHERE j.GolesAnotados >= 0
                    ORDER BY j.GolesAnotados DESC, j.Nombre ASC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                    SELECT COUNT(*) FROM Jugadores WHERE GolesAnotados >= 0;";

            var offset = (pageNumber - 1) * pageSize;
            using var multi = await connection.QueryMultipleAsync(sql, new { Offset = offset, PageSize = pageSize });

            var data = await multi.ReadAsync<JugadorReadModel>();
            var total = await multi.ReadFirstAsync<int>();

            return new PagedList<JugadorReadModel>(data.ToList(), total, pageNumber, pageSize);
        }

        public async Task<bool> ExistenTodosLosJugadoresAsync(List<Guid> ids)
        {
            if (!ids.Any()) return true;

            using var connection = _dapper.CreateConnection();
            const string sql = "SELECT COUNT(1) FROM Jugadores WHERE Id IN @Ids";

            var distinctIds = ids.Distinct().ToList();
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Ids = distinctIds });

            return count == distinctIds.Count;
        }
    }
}
