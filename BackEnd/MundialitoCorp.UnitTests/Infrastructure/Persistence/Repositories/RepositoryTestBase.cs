using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MundialitoCorp.Infrastructure.Persistence;

public abstract class RepositoryTestBase : IDisposable
{
    protected readonly ApplicationDbContext Context;
    private readonly SqliteConnection _connection;

    protected RepositoryTestBase()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new ApplicationDbContext(options);
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}