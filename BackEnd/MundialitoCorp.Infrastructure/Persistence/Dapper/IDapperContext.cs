using System.Data;

namespace MundialitoCorp.Infrastructure.Persistence.Dapper
{
    public interface IDapperContext
    {
        IDbConnection CreateConnection();
    }
}
