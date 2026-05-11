using MundialitoCorp.Domain.Common;
using FluentValidation;
using MediatR;

namespace MundialitoCorp.Application.Common.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!_validators.Any()) return await next();

            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(
                        _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

            if (failures.Count != 0)
            {
                var errors = failures.Select(f => new ValidationError(f.PropertyName, f.ErrorMessage)).ToList();
                string message = "Se encontraron errores de validación.";
                int statusCode = 422;

                if (typeof(TResponse) == typeof(Result))
                {
                    return (TResponse)(object)Result.Failure(message, statusCode, errors);
                }

                if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
                {
                    var resultType = typeof(TResponse).GetGenericArguments()[0];

                    var failureMethod = typeof(Result<>)
                        .MakeGenericType(resultType)
                        .GetMethod("Failure", new[] { typeof(string), typeof(int), typeof(List<ValidationError>) });

                    if (failureMethod != null)
                    {
                        var failureResult = failureMethod.Invoke(null, new object[] { message, statusCode, errors });
                        return (TResponse)failureResult!;
                    }
                }

                throw new ValidationException(failures);
            }

            return await next();
        }
    }
}
