namespace MundialitoCorp.Infrastructure.Persistence.Idempotency
{
    public class IdempotencyKey
    {
        public Guid Key { get; set; }
        public string RequestPath { get; set; } = string.Empty;
        public string ResponseBody { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
