using MundialitoCorp.Domain.Entities;
using MundialitoCorp.Domain.Repositories;
using MundialitoCorp.Domain.ValueObjects;

namespace MundialitoCorp.Infrastructure.Persistence.Seed
{
    public static class DbInitializer
    {
        public static async Task Seed(ApplicationDbContext context, IUnitOfWork unitOfWork)
        {
            context.Database.EnsureCreated();

            if (context.Equipos.Any()) return;

            var equipos = new List<Equipo>
            {
                Equipo.Create("Tech FC").Value!,
                Equipo.Create("Data United").Value!,
                Equipo.Create("Cloud City").Value!,
                Equipo.Create("Dev Ops Juniors").Value!
            };

            foreach (var equipo in equipos)
            {
                for (int i = 1; i <= 5; i++)
                {
                    equipo.Jugadores.Add(Jugador.Create($"{equipo.Nombre} - Jugador {i}", equipo.Id).Value!);
                }
            }

            context.Equipos.AddRange(equipos);
            await unitOfWork.SaveChangesAsync();

            var partidos = new List<Partido>
            {
                Partido.Create(equipos[0].Id, equipos[1].Id, DateOnly.FromDateTime(DateTime.Now.AddDays(-2))).Value!,
                Partido.Create(equipos[2].Id, equipos[3].Id, DateOnly.FromDateTime(DateTime.Now.AddDays(-2))).Value!,
                Partido.Create(equipos[0].Id, equipos[2].Id, DateOnly.FromDateTime(DateTime.Now.AddDays(-1))).Value!,
                Partido.Create(equipos[1].Id, equipos[3].Id, DateOnly.FromDateTime(DateTime.Now.AddDays(-1))).Value!,
                Partido.Create(equipos[0].Id, equipos[3].Id, DateOnly.FromDateTime(DateTime.Now)).Value!,
                Partido.Create(equipos[1].Id, equipos[2].Id, DateOnly.FromDateTime(DateTime.Now)).Value!
            };

            var goleadoresTech = equipos[0].Jugadores.Select(j => j.Id).Take(3).ToList();
            var goleadorData = equipos[1].Jugadores.Select(j => j.Id).Take(1).ToList();
            var goleadoresCloud = equipos[2].Jugadores.Select(j => j.Id).Take(2).ToList();

            var resultado1 = Resultado.Create(3, 1).Value!;
            var resultado2 = Resultado.Create(1, 1).Value!;
            var resultado3 = Resultado.Create(0, 2).Value!;
            partidos[0].RegistrarResultado(resultado1, goleadoresTech, goleadorData);
            partidos[1].RegistrarResultado(resultado2,
                new List<Guid> { equipos[2].Jugadores[0].Id },
                new List<Guid> { equipos[3].Jugadores[0].Id });
            partidos[2].RegistrarResultado(resultado3, new List<Guid>(), goleadoresCloud);

            context.Partidos.AddRange(partidos);
            await unitOfWork.SaveChangesAsync();
        }
    }
}
