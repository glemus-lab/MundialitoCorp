using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MundialitoCorp.Infrastructure.Logging
{
    public class EventLogger<T>
    {
        private readonly ILogger<T> _logger;

        public EventLogger(ILogger<T> logger) => _logger = logger;

        public virtual void LogDomainEvent(object domainEvent, string traceId)
        {
            var eventName = domainEvent.GetType().Name;
            var payload = JsonConvert.SerializeObject(domainEvent);

            _logger.LogInformation("Event: {EventName} | TraceId: {TraceId} | Payload: {Payload}",
                eventName, traceId, payload);
        }
    }
}