using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using Moq;
using MundialitoCorp.Infrastructure.Persistence.Dapper;

namespace MundialitoCorp.UnitTests.Infrastructure.Persistence.Queries
{
    public abstract class QueryServiceTestBase : IDisposable
    {
        protected readonly SqliteConnection Connection;
        protected readonly Mock<IDapperContext> DapperContextMock;

        static QueryServiceTestBase()
        {
            SqlMapper.ResetTypeHandlers();
            SqlMapper.AddTypeHandler(new GuidTypeHandler());
            SqlMapper.AddTypeHandler(new IntHandler());
            SqlMapper.AddTypeHandler(new BoolHandler());
            SqlMapper.AddTypeHandler(new LongHandler());
            SqlMapper.AddTypeHandler(new DateOnlyHandler());
        }

        protected QueryServiceTestBase()
        {
            Connection = new("DataSource=:memory:");
            Connection.Open();

            DapperContextMock = new();
            DapperContextMock.Setup(x => x.CreateConnection()).Returns(Connection);

            CrearTablasEstandar();
        }

        private void CrearTablasEstandar()
        {
            Connection.Execute(@"
            CREATE TABLE Equipos (
                Id UNIQUEIDENTIFIER PRIMARY KEY, 
                Nombre TEXT,
                Puntos INTEGER,
                PartidosJugados INTEGER,
                PartidosGanados INTEGER,
                PartidosPerdidos INTEGER,
                PartidosEmpatados INTEGER,
                GolesFavor INTEGER,
                GolesContra INTEGER,
                DiferenciaGoles INTEGER
            );
            CREATE TABLE Jugadores (
                Id UNIQUEIDENTIFIER PRIMARY KEY, 
                Nombre TEXT,
                EquipoId UNIQUEIDENTIFIER,
                GolesAnotados INTEGER
            );
            CREATE TABLE Partidos (
                Id UNIQUEIDENTIFIER PRIMARY KEY,
                EquipoLocalId UNIQUEIDENTIFIER,
                EquipoVisitanteId UNIQUEIDENTIFIER,
                GolesLocal INTEGER,
                GolesVisitante INTEGER,
                Fecha TEXT,
                EstaFinalizado INTEGER
                );");
        }

        public void Dispose()
        {
            Connection.Close();
            Connection.Dispose();
        }
    }

    public class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
    {
        public override void SetValue(IDbDataParameter parameter, Guid value) => parameter.Value = value.ToString();
        public override Guid Parse(object value) => Guid.Parse(value.ToString());
    }

    public class IntHandler : SqlMapper.TypeHandler<int>
    {
        public override void SetValue(IDbDataParameter parameter, int value) => parameter.Value = value;
        public override int Parse(object value) => Convert.ToInt32(value);
    }

    public class BoolHandler : SqlMapper.TypeHandler<bool>
    {
        public override void SetValue(IDbDataParameter parameter, bool value) => parameter.Value = value ? 1 : 0;
        public override bool Parse(object value) => Convert.ToInt64(value) == 1;
    }

    public class LongHandler : SqlMapper.TypeHandler<long>
    {
        public override void SetValue(IDbDataParameter parameter, long value) => parameter.Value = value;
        public override long Parse(object value) => Convert.ToInt64(value);
    }

    public class DateOnlyHandler : SqlMapper.TypeHandler<DateOnly>
    {
        public override void SetValue(IDbDataParameter parameter, DateOnly value)
            => parameter.Value = value.ToString("yyyy-MM-dd");

        public override DateOnly Parse(object value)
            => DateOnly.Parse(value.ToString()!.Split(' ')[0]);
    }
}
