using MundialitoCorp.Application.Common;
using MundialitoCorp.Application.Interfaces;
using MundialitoCorp.Application.Models;
using MundialitoCorp.Infrastructure.Persistence.Dapper;
using Dapper;

namespace MundialitoCorp.Infrastructure.Persistence.Queries
{
    public class PartidoQueryService : IPartidoQueryService
    {
        private readonly IDapperContext _dapper;

        public PartidoQueryService(IDapperContext dapper)
        {
            _dapper = dapper;
        }

        public async Task<PagedList<PartidoReadModel>> GetPartidosPagedAsync(int page, int size, string? sortBy, string? sortDirection, DateTime? fecha, Guid? equipoId, bool? finalizado)
        {
            using var connection = _dapper.CreateConnection();

            var validColumns = new Dictionary<string, string>
            {
                { "fecha", "p.Fecha" },
                { "local", "eL.Nombre" },
                { "visitante", "eV.Nombre" },
                { "golesl", "p.GolesLocal" },
                { "golesv", "p.GolesVisitante" },
                { "finalizado", "p.EstaFinalizado" }
            };

            string orderColumn = validColumns.TryGetValue(sortBy ?? "", out var column) ? column : "p.Fecha";
            string direction = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";

            string sql = $@"
                SELECT p.Id, eL.Nombre as Local, eV.Nombre as Visitante, 
                       p.GolesLocal as GolesL, p.GolesVisitante as GolesV, 
                       p.Fecha, p.EstaFinalizado as Finalizado
                FROM Partidos p
                INNER JOIN Equipos eL ON p.EquipoLocalId = eL.Id
                INNER JOIN Equipos eV ON p.EquipoVisitanteId = eV.Id
                WHERE (@Fecha IS NULL OR CAST(p.Fecha AS DATE) = @Fecha)
                  AND (@EquipoId IS NULL OR p.EquipoLocalId = @EquipoId OR p.EquipoVisitanteId = @EquipoId)
                  AND (@Finalizado IS NULL OR p.EstaFinalizado = @Finalizado)
                ORDER BY {orderColumn} {direction}
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT COUNT(*) 
                FROM Partidos p
                WHERE (@Fecha IS NULL OR CAST(p.Fecha AS DATE) = @Fecha)
                  AND (@EquipoId IS NULL OR p.EquipoLocalId = @EquipoId OR p.EquipoVisitanteId = @EquipoId)
                  AND (@Finalizado IS NULL OR p.EstaFinalizado = @Finalizado);";

            using var multi = await connection.QueryMultipleAsync(sql, new
            {
                Fecha = fecha?.Date,
                EquipoId = equipoId,
                Finalizado = finalizado,
                Offset = (page - 1) * size,
                PageSize = size
            });

            var items = (await multi.ReadAsync<PartidoReadModel>()).ToList();
            var total = await multi.ReadFirstAsync<int>();

            return new PagedList<PartidoReadModel>(items, total, page, size);
        }

        public async Task<PartidoDetalleReadModel?> GetByIdAsync(Guid partidoId)
        {
            using var connection = _dapper.CreateConnection();
            const string sql = @"
            SELECT p.EquipoLocalId, el.Nombre as EquipoLocal, 
                   p.EquipoVisitanteId, ev.Nombre as EquipoVisitante, 
                   ISNULL(p.GolesLocal,0) as GolesLocal, ISNULL(p.GolesVisitante,0) as GolesVisitante, p.Fecha
            FROM Partidos p
            JOIN Equipos el ON p.EquipoLocalId = el.Id
            JOIN Equipos ev ON p.EquipoVisitanteId = ev.Id
            WHERE p.Id = @Id;

            SELECT g.JugadorId, j.Nombre as NombreJugador, j.EquipoId
            FROM PartidoGoles g
            JOIN Jugadores j ON g.JugadorId = j.Id
            WHERE g.PartidoId = @Id";

            using var multi = await connection.QueryMultipleAsync(sql, new { Id = partidoId });
            var partido = await multi.ReadFirstOrDefaultAsync<dynamic>();
            if (partido == null) return null!;

            var goleadores = (await multi.ReadAsync<GoleadorReadModel>()).ToList();

            return new PartidoDetalleReadModel(
                            (Guid)partido.EquipoLocalId,
                            (string)partido.EquipoLocal,
                            (Guid)partido.EquipoVisitanteId,
                            (string)partido.EquipoVisitante,
                            (int)partido.GolesLocal,
                            (int)partido.GolesVisitante,
                            DateOnly.FromDateTime((DateTime)partido.Fecha),
                            goleadores
                    );
        }

        public async Task<bool> ExisteConflictoFechaAsync(Guid equipoId, DateOnly fecha)
        {
            using var connection = _dapper.CreateConnection();

            var inicioDia = fecha;
            var finDia = inicioDia.AddDays(1);

            const string sql = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM Partidos 
                WHERE (EquipoLocalId = @Id OR EquipoVisitanteId = @Id)
                  AND Fecha >= @Inicio AND Fecha <= @Fin
            ) THEN 1 ELSE 0 END";

            return await connection.ExecuteScalarAsync<bool>(sql, new
            {
                Id = equipoId,
                Inicio = inicioDia,
                Fin = finDia
            });
        }

        public async Task<IEnumerable<PartidoReadModel>> GetPartidosPendientesAsync()
        {
            using var connection = _dapper.CreateConnection();
            const string sql = @"
                    SELECT 
                        p.Id, 
                        el.Nombre AS Local, 
                        ev.Nombre AS Visitante,
                        p.EquipoLocalId,
                        p.EquipoVisitanteId,
                        p.GolesLocal AS GolesL, 
                        p.GolesVisitante AS GolesV,
                        p.Fecha,
                        p.EstaFinalizado
                    FROM Partidos p
                    INNER JOIN Equipos el ON p.EquipoLocalId = el.Id
                    INNER JOIN Equipos ev ON p.EquipoVisitanteId = ev.Id
                    WHERE p.EstaFinalizado = 0 
                    ORDER BY p.Fecha ASC";

            return await connection.QueryAsync<PartidoReadModel>(sql);
        }

        public async Task<PagedList<PartidoReadModel>> GetHistorialPartidosAsync(int pageNumber, int pageSize)
        {
            using var connection = _dapper.CreateConnection();
            const string sql = @"
                    SELECT 
                        p.Id, 
                        el.Nombre AS Local, 
                        ev.Nombre AS Visitante,
                        p.EquipoLocalId,
                        p.EquipoVisitanteId,
                        ISNULL(p.GolesLocal, 0) AS GolesL, 
                        ISNULL(p.GolesVisitante, 0) AS GolesV,
                        p.Fecha,
                        p.EstaFinalizado
                    FROM Partidos p
                    INNER JOIN Equipos el ON p.EquipoLocalId = el.Id
                    INNER JOIN Equipos ev ON p.EquipoVisitanteId = ev.Id
                    WHERE p.EstaFinalizado = 1
                    ORDER BY p.Fecha DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                    SELECT COUNT(*) FROM Partidos WHERE EstaFinalizado = 1;";

            var offset = (pageNumber - 1) * pageSize;
            using var multi = await connection.QueryMultipleAsync(sql, new { Offset = offset, PageSize = pageSize });

            var data = await multi.ReadAsync<PartidoReadModel>();
            var total = await multi.ReadFirstAsync<int>();

            return new PagedList<PartidoReadModel>(data.ToList(), total, pageNumber, pageSize);
        }
    }
}
