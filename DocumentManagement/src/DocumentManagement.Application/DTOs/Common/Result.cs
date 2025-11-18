namespace DocumentManagement.Application.DTOs.Common
{
    public class Result<T>
    {
        public bool IsSuccess { get; init; }
        public T? Data { get; init; }
        public string? Error { get; init; }
        public int? StatusCode { get; init; }

        public static Result<T> Success(T data) => new()
        {
            IsSuccess = true,
            Data = data,
            StatusCode = 200
        };

        public static Result<T> Failure(string error, int statusCode = 400) => new()
        {
            IsSuccess = false,
            Error = error,
            StatusCode = statusCode
        };
    }

    public class Result
    {
        public bool IsSuccess { get; init; }
        public string? Error { get; init; }
        public int? StatusCode { get; init; }

        public static Result Success() => new()
        {
            IsSuccess = true,
            StatusCode = 200
        };

        public static Result Failure(string error, int statusCode = 400) => new()
        {
            IsSuccess = false,
            Error = error,
            StatusCode = statusCode
        };
    }
}
