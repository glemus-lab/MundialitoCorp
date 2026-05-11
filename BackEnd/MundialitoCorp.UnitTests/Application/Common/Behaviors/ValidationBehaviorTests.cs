using MundialitoCorp.Application.Common.Behaviors;
using MundialitoCorp.Application.Features.Equipos.Commands.CreateEquipo;
using MundialitoCorp.Application.Features.Equipos.Commands.UpdateEquipo;
using MundialitoCorp.Domain.Common;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;

namespace MundialitoCorp.UnitTests.Application.Common.Behaviors
{
    public class ValidationBehaviorTests
    {
        private readonly Mock<IValidator<CreateEquipoCommand>> _validatorMock;
        private readonly Mock<RequestHandlerDelegate<Result<Guid>>> _nextMock;
        private readonly CreateEquipoCommand _request;

        public ValidationBehaviorTests()
        {
            _validatorMock = new Mock<IValidator<CreateEquipoCommand>>();
            _nextMock = new Mock<RequestHandlerDelegate<Result<Guid>>>();
            _request = new CreateEquipoCommand("Real Madrid");
        }

        public class SimpleRequest : IRequest<string> { }

        [Fact]
        public async Task Handle_DebeLlamarAlSiguienteEnElPipeline_CuandoNoHayValidadores()
        {
            // Arrange
            var behavior = new ValidationBehavior<CreateEquipoCommand, Result<Guid>>(
                Enumerable.Empty<IValidator<CreateEquipoCommand>>());

            _nextMock.Setup(x => x()).ReturnsAsync(Result<Guid>.Success(Guid.NewGuid(), 200));

            // Act
            var result = await behavior.Handle(_request, _nextMock.Object, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _nextMock.Verify(x => x(), Times.Once);
        }

        [Fact]
        public async Task Handle_DebeLlamarAlSiguienteEnElPipeline_CuandoLaValidacionEsExitosa()
        {
            // Arrange
            _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<CreateEquipoCommand>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var behavior = new ValidationBehavior<CreateEquipoCommand, Result<Guid>>(new[] { _validatorMock.Object });

            _nextMock.Setup(x => x()).ReturnsAsync(Result<Guid>.Success(Guid.NewGuid(), 200));

            // Act
            var result = await behavior.Handle(_request, _nextMock.Object, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _nextMock.Verify(x => x(), Times.Once);
        }

        [Fact]
        public async Task Handle_DebeRetornarResultFailure_CuandoExistenErroresDeValidacion()
        {
            // Arrange
            var fallos = new List<ValidationFailure>
            {
                new("Nombre", "El nombre es obligatorio"),
                new("Nombre", "El nombre es muy corto")
            };

            _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<CreateEquipoCommand>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(fallos));

            var behavior = new ValidationBehavior<CreateEquipoCommand, Result<Guid>>(new[] { _validatorMock.Object });

            // Act
            var result = await behavior.Handle(_request, _nextMock.Object, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Code.Should().Be(422);
            result.Errors[0].PropertyName.Should().Contain("Nombre");
            result.Errors[0].Message.Should().Contain("El nombre es obligatorio");
            result.Errors[1].PropertyName.Should().Contain("Nombre");
            result.Errors[1].Message.Should().Contain("El nombre es muy corto");

            _nextMock.Verify(x => x(), Times.Never);
        }

        [Fact]
        public async Task Handle_DebeManejarMultiplesValidadores_Correctamente()
        {
            // Arrange
            var validatorMock2 = new Mock<IValidator<CreateEquipoCommand>>();

            _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<CreateEquipoCommand>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Prop1", "Error 1") }));

            validatorMock2.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<CreateEquipoCommand>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Prop2", "Error 2") }));

            var behavior = new ValidationBehavior<CreateEquipoCommand, Result<Guid>>(new[] { _validatorMock.Object, validatorMock2.Object });

            // Act
            var result = await behavior.Handle(_request, _nextMock.Object, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Se encontraron errores de validación.");
            result.Code.Should().Be(422);
            result.Errors.Count.Should().Be(2);
            result.Errors[0].PropertyName.Should().Be("Prop1");
            result.Errors[0].Message.Should().Be("Error 1");
            result.Errors[1].PropertyName.Should().Be("Prop2");
            result.Errors[1].Message.Should().Be("Error 2");
        }

        [Fact]
        public async Task Handle_DebeRetornarResultSimple_CuandoTResponseEsResultNoGenerico()
        {
            // Arrange
            var updateRequest = new UpdateEquipoCommand(Guid.NewGuid(), "Nombre");
            var updateValidatorMock = new Mock<IValidator<UpdateEquipoCommand>>();
            var updateNextMock = new Mock<RequestHandlerDelegate<Result>>();

            updateValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<UpdateEquipoCommand>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Nombre", "Error de Validación") }));

            var behavior = new ValidationBehavior<UpdateEquipoCommand, Result>(new[] { updateValidatorMock.Object });

            // Act
            var result = await behavior.Handle(updateRequest, updateNextMock.Object, CancellationToken.None);

            // Assert
            result.Should().BeOfType<Result>();
            result.IsSuccess.Should().BeFalse();
            result.Code.Should().Be(422);
            updateNextMock.Verify(x => x(), Times.Never);
        }

        [Fact]
        public async Task Handle_DebeLanzarValidationException_CuandoTResponseNoEsTipoResult()
        {
            // Arrange
            var simpleRequest = new SimpleRequest();
            var simpleValidatorMock = new Mock<IValidator<SimpleRequest>>();
            var simpleNextMock = new Mock<RequestHandlerDelegate<string>>();

            simpleValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<SimpleRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Prop", "Error") }));

            var behavior = new ValidationBehavior<SimpleRequest, string>(new[] { simpleValidatorMock.Object });

            // Act
            Func<Task> act = async () => await behavior.Handle(simpleRequest, simpleNextMock.Object, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationException>();
            simpleNextMock.Verify(x => x(), Times.Never);
        }
    }
}