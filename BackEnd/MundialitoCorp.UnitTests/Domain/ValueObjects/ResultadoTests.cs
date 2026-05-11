using MundialitoCorp.Domain.ValueObjects;
using FluentAssertions;

namespace MundialitoCorp.UnitTests.Domain.ValueObjects
{
    public class ResultadoTests
    {
        [Fact]
        public void Resultado_AsignaValoresDeGoles()
        {
            var result = Resultado.Create(3, 1);
            var resultado = result.Value!;
            result.IsSuccess.Should().BeTrue();
            resultado.GolesLocal.Should().Be(3);
            resultado.GolesVisitante.Should().Be(1);
        }

        [Fact]
        public void Resultado_DebeIdentificarGanadorLocal()
        {
            var resultado = Resultado.Create(3, 1).Value!;
            resultado.GanoLocal.Should().BeTrue();
            resultado.EsEmpate.Should().BeFalse();
            resultado.GanoVisitante.Should().BeFalse();
        }

        [Fact]
        public void Resultado_DebeIdentificarGanadorVisitante()
        {
            var resultado = Resultado.Create(1, 3).Value!;
            resultado.GanoLocal.Should().BeFalse();
            resultado.EsEmpate.Should().BeFalse();
            resultado.GanoVisitante.Should().BeTrue();
        }

        [Fact]
        public void Resultado_DebeIdentificarEmpate()
        {
            var resultado = Resultado.Create(1, 1).Value!;
            resultado.GanoLocal.Should().BeFalse();
            resultado.EsEmpate.Should().BeTrue();
            resultado.GanoVisitante.Should().BeFalse();
        }

        [Theory]
        [InlineData(0, -1)]
        [InlineData(-1, 0)]
        public void Resultado_DebeRegresarResultFailure_CuandoGolesSonNegativos(int golesL, int golesV)
        {
            var result = Resultado.Create(golesL, golesV);
            
            result.IsFailure.Should().BeTrue();
            result.ErrorMessage.Should().Be("Los goles no pueden ser negativos.");
            result.Code.Should().Be(422);
        }
    }
}
