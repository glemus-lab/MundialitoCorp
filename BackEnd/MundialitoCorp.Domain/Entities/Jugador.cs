using MundialitoCorp.Domain.Common;

namespace MundialitoCorp.Domain.Entities
{
    public class Jugador : Entity
    {
        public string Nombre { get; private set; }
        public int GolesAnotados { get; private set; }
        public Guid EquipoId { get; private set; }

        private Jugador() { }

        private Jugador(string nombre, Guid equipoId)
        {
            Id = Guid.NewGuid();
            Nombre = nombre;
            EquipoId = equipoId;
            GolesAnotados = 0;
        }

        public static Result<Jugador> Create(string nombre, Guid equipoId)
        {
            if (equipoId == Guid.Empty)
                return Result<Jugador>.Failure("El Id del equipo es requerido.", 422);

            if (string.IsNullOrWhiteSpace(nombre))
                return Result<Jugador>.Failure("Error de validación.", 422, [new("Nombre", "El nombre del jugador no puede estar vacío.")]);

            if (nombre.Length > 150)
                return Result<Jugador>.Failure("Error de validación.", 422, [new("Nombre", "El nombre no puede contener más de 150 caracteres.")]);

            return Result<Jugador>.Success(new Jugador(nombre, equipoId), 200);
        }

        public Result CambiarNombre(string nuevoNombre)
        {
            if (string.IsNullOrWhiteSpace(nuevoNombre))
                return Result.Failure("Error al actualizar el nombre del jugador.", 422, [new("Nombre", "El nombre del jugador no puede estar vacío.")]);

            if (nuevoNombre.Length > 150)
                return Result.Failure("Error al actualizar el nombre del jugador.", 422, [new("Nombre", "El nombre no puede contener más de 150 caracteres.")]);

            Nombre = nuevoNombre;

            return Result.Success(200);
        }

        public void RegistrarGol() => GolesAnotados++;
    }
}
