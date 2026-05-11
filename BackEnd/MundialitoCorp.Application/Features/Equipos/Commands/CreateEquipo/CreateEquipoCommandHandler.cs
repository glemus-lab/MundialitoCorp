using MundialitoCorp.Domain.Common;
using MundialitoCorp.Domain.Entities;
using MundialitoCorp.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MundialitoCorp.Application.Features.Equipos.Commands.CreateEquipo
{
    public class CreateEquipoCommandHandler : IRequestHandler<CreateEquipoCommand, Result<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEquipoRepository _equipoRepository;
        private readonly ILogger<CreateEquipoCommandHandler> _logger;

        public CreateEquipoCommandHandler(IUnitOfWork unitOfWork, IEquipoRepository equipoRepository, ILogger<CreateEquipoCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _equipoRepository = equipoRepository;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(CreateEquipoCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Iniciando creación del equipo: '{Nombre}'", request.Nombre);

            var existe = await _equipoRepository.ExistsAsync(request.Nombre);

            if (existe)
            {
                _logger.LogWarning("Duplicidad nombre de equipo: '{Nombre}'", request.Nombre);
                return Result<Guid>.Failure("Error de validación.", 409, [new("Nombre", "Ya existe un equipo registrado con ese nombre.")]);
            }

            var result = Equipo.Create(request.Nombre);

            if (result.IsFailure)
            {
                _logger.LogWarning("Error en la creacion de la entidad equipo. ErrorMessage: '{ErrorMessage}'.", result.ErrorMessage);
                return Result<Guid>.Failure(result.ErrorMessage, result.Code, result.Errors);
            }


            Equipo equipo = result.Value!;

            await _equipoRepository.AddAsync(equipo);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Finalización de la creación del equipo: {Nombre} con Id: '{Id}'.", request.Nombre, equipo.Id);

            return Result<Guid>.Success(equipo.Id, 201);
        }
    }
}
