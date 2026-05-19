using System.Net;

namespace MoodPlaylistApi.Utilities
{
    public class ApiResponse<T>
    {
        //private static readonly JsonSerializerOptions s_jsonOptions = new()
        //{
        //    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        //};

        public HttpStatusCode StatusCode { get; set; }
        public bool Successful { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ErrorDetails { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 1;
        public T? Data { get; set; }

        //public override string ToString()
        //{
        //    return JsonSerializer.Serialize(this, s_jsonOptions);
        //}

        // Success constructor for single item or non paged collections
        private ApiResponse(
            HttpStatusCode statusCode,
            string message,
            T? data)
        {
            StatusCode = statusCode;
            Successful = true;
            Message = message;
            PageNumber = 1;
            PageSize = 1;
            Data = data;
        }

        // Success constructor for single item or non paged collections
        private ApiResponse(
            HttpStatusCode statusCode,
            string message,
            T? data,
            int pageNumber,
            int pageSize)
        {
            StatusCode = statusCode;
            Successful = true;
            Message = message;
            PageNumber = pageNumber;
            PageSize = pageSize;
            Data = data;
        }

        // Failure constructor
        private ApiResponse(
            HttpStatusCode statusCode,
            string message,
            string? errorDetails = null)
        {
            StatusCode = statusCode;
            Successful = false;
            Message = message;
            ErrorDetails = errorDetails ?? string.Empty;
            PageNumber = 1;
            PageSize = 0;
            Data = default;
        }

        // Success helper for single responses
        public static ApiResponse<T> Success(
            HttpStatusCode statusCode,
            string message = "success",
            T? data = default)
            => new
            (
                statusCode,
                message,
                data
            );

        // Success helper for collections
        public static ApiResponse<List<T>> SuccessList(
            HttpStatusCode statusCode,
            List<T>? data = default,
            string message = "success",
            int pageNumber = 1)
            => new            (
                statusCode,
                message,
                data,
                pageNumber,
                data != null ? data.Count : 0
            );

        // Error helper for both
        // single and collection Responses
        public static ApiResponse<T> Error(
            HttpStatusCode statusCode,
            string message,
            string? errorDetails = null)
            => new
            (
                statusCode,
                message,
                errorDetails
            );
    }
}
