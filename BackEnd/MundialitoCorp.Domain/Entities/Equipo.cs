using MundialitoCorp.Domain.Common;
using MundialitoCorp.Domain.Events;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MundialitoCorp.Domain.Entities
{
    public class Equipo : Entity
    {
        public string Nombre { get; private set; }
        public int PartidosJugados { get; private set; }
        public int PartidosGanados { get; private set; }
        public int PartidosPerdidos { get; private set; }
        public int PartidosEmpatados { get; private set; }
        public int Puntos { get; private set; }
        public int GolesFavor { get; private set; }
        public int GolesContra { get; private set; }
        public int DiferenciaGoles { get; private set; }
        
        public List<Jugador> Jugadores { get; private set; } = new();

        private Equipo() { }

        private Equipo(string nombre)
        {
            
            Id = Guid.NewGuid();
            Nombre = nombre;

            AddDomainEvent(new EquipoCreadoEvent(Id, Nombre));
        }

        public static Result<Equipo> Create(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return Result<Equipo>.Failure("Error de validación.", 422, [new("Nombre", "El nombre del equipo no puede estar vacío.")]);

            if (nombre.Length > 100)
                return Result<Equipo>.Failure("Error de validación.", 422, [new("Nombre", "El nombre no puede superar los 100 caracteres.")]);

            return Result<Equipo>.Success(new Equipo(nombre), 200);
        }

        public Result CambiarNombre(string nuevoNombre)
        {
            if (string.IsNullOrWhiteSpace(nuevoNombre))
                return Result.Failure("Error de validación.", 422, [new("Nombre", "El nombre del equipo no puede estar vacío.")]);

            if (nuevoNombre.Length > 100)
                return Result<Equipo>.Failure("Error de validación.", 422, [new("Nombre", "El nombre no puede superar los 100 caracteres.")]);

            Nombre = nuevoNombre;

            return Result.Success(200);
        }

        public Result ActualizarEstadisticas(int favor, int contra)
        {
            if (favor < 0 || contra < 0)
                return Result.Failure("Los goles no pueden ser negativos.", 422);

            PartidosJugados++;
            GolesFavor += favor;
            GolesContra += contra;

            if (favor > contra)
            {
                PartidosGanados++;
                Puntos += 3; 
            }
            else if (favor == contra)
            {
                PartidosEmpatados++;
                Puntos += 1; 
            }
            else
            {
                PartidosPerdidos++;
            }

            DiferenciaGoles = GolesFavor - GolesContra;

            return Result.Success(200);
        }

        public Result AgregarJugador(string nombre)
        {
            if (Jugadores.Count >= 25)
                return Result.Failure("El equipo ya tiene el máximo de jugadores.", 422);

            var result = Jugador.Create(nombre, Id);

            if (result.IsFailure)
                return Result.Failure(result.ErrorMessage, result.Code, result.Errors);

            var nuevoJugador = result.Value!;
            Jugadores.Add(nuevoJugador);

            return Result.Success(200);
        }
    }
}
