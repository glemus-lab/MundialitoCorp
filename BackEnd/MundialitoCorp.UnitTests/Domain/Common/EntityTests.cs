using MundialitoCorp.Domain.Common;
using FluentAssertions;
using System.Linq;
using Xunit;

namespace MundialitoCorp.UnitTests.Domain.Common
{
    public class TestEntity : Entity
    {
        public void TestAddEvent(object @event) => AddDomainEvent(@event);
    }

    public class EntityTests
    {
        [Fact]
        public void AddDomainEvent_DebeAcumularEventosEnLaLista()
        {
            // Arrange
            var entity = new TestEntity();
            var evento = new { Name = "TestEvent" };

            // Act
            entity.TestAddEvent(evento);

            // Assert
            entity.DomainEvents.Should().HaveCount(1);
            entity.DomainEvents.First().Should().Be(evento);
        }

        [Fact]
        public void ClearDomainEvents_DebeVaciarLaLista_CuandoSeLlama()
        {
            // Arrange
            var entity = new TestEntity();
            entity.TestAddEvent(new { });

            // Act
            entity.ClearDomainEvents();

            // Assert
            entity.DomainEvents.Should().BeEmpty();
        }
    }
}