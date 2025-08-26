// MoneyTracker.API/Models/ApiResponse.cs
using System.Text.Json.Serialization;

namespace MoneyTracker.API.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    public string? Message { get; set; }

    public List<ValidationError>? Errors { get; set; }

    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("O");

    public int StatusCode { get; set; }

    public static ApiResponse<T> SuccessResult(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message,
            StatusCode = 200,
            Timestamp = DateTime.UtcNow.ToString("O")
        };
    }

    public static ApiResponse<T> ErrorResult(string message, List<ValidationError>? errors = null, int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors,
            StatusCode = statusCode,
            Timestamp = DateTime.UtcNow.ToString("O")
        };
    }
}

public class ValidationError
{
    public string PropertyName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public object? AttemptedValue { get; set; }
}

public class PaginatedResponse<T> : ApiResponse<List<T>>
{
    public PaginationMetadata? Pagination { get; set; }

    public new static PaginatedResponse<T> SuccessResult(
        List<T> data,
        PaginationMetadata pagination,
        string? message = null)
    {
        return new PaginatedResponse<T>
        {
            Success = true,
            Data = data,
            Pagination = pagination,
            Message = message,
            StatusCode = 200,
            Timestamp = DateTime.UtcNow.ToString("O")
        };
    }
}

public class PaginationMetadata
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public bool HasNext { get; set; }
    public bool HasPrevious { get; set; }
}