namespace tracksByPopularity.Application.DTOs;

/// <summary>
/// A standardized API response wrapper.
/// </summary>
/// <typeparam name="T">The type of the data returned.</typeparam>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
    public object? Errors { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null)
    {
        return new ApiResponse<T> { Success = true, Data = data, Message = message };
    }
}

/// <summary>
/// A standardized API response wrapper for endpoints that do not return data.
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Ok(string? message = null)
    {
        return new ApiResponse { Success = true, Message = message };
    }

    public static ApiResponse Fail(string error, object? errors = null)
    {
        return new ApiResponse { Success = false, Error = error, Errors = errors };
    }
}
