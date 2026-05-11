using MundialitoCorp.Application.Common;
using MundialitoCorp.Application.Interfaces;
using MundialitoCorp.Application.Models;
using MundialitoCorp.Infrastructure.Persistence.Dapper;
using Dapper;

namespace MundialitoCorp.Infrastructure.Persistence.Queries
{
    public class EquipoQueryService : IEquipoQueryService
    {
        private readonly IDapperContext _dapper;

        public EquipoQueryService(IDapperContext dapper)
        {
            _dapper = dapper;
        }

        public async Task<PagedList<EquipoPagedReadModel>> GetEquiposPagedAsync(int page, int size, string? sortBy, string? sortDirection, string? filter)
        {
            using var connection = _dapper.CreateConnection();

            var validColumns = new Dictionary<string, string>
            {
                { "nombre", "Nombre" },
                { "puntos", "Puntos" },
                { "partidosjugados", "PartidosJugados" }
            };

            string orderColumn = validColumns.TryGetValue(sortBy ?? "", out var column) ? column : "Nombre";
            string direction = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "ASC" : "ASC";

            string sql = $@"
            SELECT Id, Nombre,
            Puntos, PartidosJugados
            FROM Equipos 
            WHERE (@Filter IS NULL OR Nombre LIKE @Filter)
            ORDER BY {orderColumn} {direction}
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*) FROM Equipos WHERE (@Filter IS NULL OR Nombre LIKE @Filter);";

            using var multi = await connection.QueryMultipleAsync(sql, new
            {
                Filter = string.IsNullOrEmpty(filter) ? null : $"%{filter}%",
                Offset = (page - 1) * size,
                PageSize = size
            });

            var items = (await multi.ReadAsync<EquipoPagedReadModel>()).ToList();
            var total = await multi.ReadFirstAsync<int>();

            return new PagedList<EquipoPagedReadModel>(items, total, page, size);
        }

        public async Task<IEnumerable<EquipoReadModel>> GetAllAsync()
        {
            using var connection = _dapper.CreateConnection();

            string sql = $@"
            SELECT Id, Nombre
            FROM Equipos 
            ORDER BY Nombre;";

            return await connection.QueryAsync<EquipoReadModel>(sql);
        }

        public async Task<EquipoReadModel?> GetByIdAsync(Guid id)
        {
            using var connection = _dapper.CreateConnection();

            const string sql = @"
            SELECT Id, Nombre
            FROM Equipos 
            WHERE Id = @Id";

            return await connection.QueryFirstOrDefaultAsync<EquipoReadModel>(sql, new { Id = id });
        }
    }
}
