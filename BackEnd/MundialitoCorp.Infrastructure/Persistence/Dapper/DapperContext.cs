using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace MundialitoCorp.Infrastructure.Persistence.Dapper
{
    public class DapperContext :IDapperContext
    {
        private readonly string _connectionString;

        public DapperContext(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
            SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        }

        public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}
