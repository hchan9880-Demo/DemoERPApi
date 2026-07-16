using DemoERPApi.Models;

namespace DemoERPApi.Helpers;

public static class ApiResponseHelper
{

    public static ApiResponse<T> Success<T>(
        T data,
        string message,
        string traceId)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            TraceId = traceId
        };
    }


    public static ApiResponse<T> Failure<T>(
        string message,
        string traceId)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default,
            TraceId = traceId
        };
    }

}