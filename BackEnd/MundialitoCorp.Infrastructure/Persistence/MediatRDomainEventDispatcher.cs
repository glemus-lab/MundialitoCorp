using MundialitoCorp.Application.Interfaces;
using MundialitoCorp.Domain.Common;
using MundialitoCorp.Infrastructure.Logging;
using MediatR;

namespace MundialitoCorp.Infrastructure.Persistence
{
    public class MediatRDomainEventDispatcher
    {
        private readonly IPublisher _publisher;
        private readonly EventLogger<MediatRDomainEventDispatcher> _logger;
        private readonly ICorrelationIdGenerator _correlationIdGenerator;

        public MediatRDomainEventDispatcher(IPublisher publisher, EventLogger<MediatRDomainEventDispatcher> logger, ICorrelationIdGenerator correlationIdGenerator)
        {
            _publisher = publisher;
            _logger = logger;
            _correlationIdGenerator = correlationIdGenerator;
        }

        public async Task DispatchEventsAsync(ApplicationDbContext context)
        {
            var traceId = _correlationIdGenerator.Get();

            var entities = context.ChangeTracker
                .Entries<Entity>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents.Any())
                .ToList();

            foreach (var entity in entities)
            {
                var events = entity.DomainEvents.ToList();
                entity.ClearDomainEvents();

                foreach (var domainEvent in events)
                {
                    _logger.LogDomainEvent(domainEvent, traceId);
                    await _publisher.Publish(domainEvent);
                }
            }
        }
    }
}
