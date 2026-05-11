using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MundialitoCorp.Domain.Common
{
    public class ValidationError
    {
        public ValidationError(string propertyName, string message)
        {
            PropertyName = propertyName;
            Message = message;
        }
        public string PropertyName { get; } = string.Empty;
        public string Message { get; } = string.Empty;
    }

    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string ErrorMessage { get; }
        public int Code { get; }
        public List<ValidationError> Errors { get; protected set; } = new();

        protected Result(bool isSuccess, int code, string error = null!, List<ValidationError>? errors = null)
        {
            IsSuccess = isSuccess;
            ErrorMessage = error;
            Code = code;
            Errors = errors ?? new();
        }

        public static Result Success(int code) 
            => new(true, code);
        public static Result Failure(string error, int code, List<ValidationError>? errors = null) 
            => new(false, code, error, errors);
    }

    public class Result<T> : Result
    {
        public T? Value { get; }
        protected Result(T? value, bool isSuccess, int code, string error = null!, List<ValidationError>? errors = null)
            : base(isSuccess, code, error, errors) => Value = value;

        public static Result<T> Success(T value, int code) => new(value, true, code);
        public static new Result<T> Failure(string error, int code, List<ValidationError>? errors = null) => new(default, false, code, error, errors);
    }
}
