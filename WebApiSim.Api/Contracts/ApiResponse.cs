using System.ComponentModel.DataAnnotations;

namespace WebApiSim.Api.Contracts
{
    public enum ApiResponseType
    {
        Unassigned,
        Succeed,
        Failed
    }

    public class ApiResponse
    {
        public ApiResponseType Type { get; set; }
        public string Message { get; set; }

        public static ApiResponse CreateSucceed()
        {
            return new ApiResponse { Type = ApiResponseType.Succeed };
        }

        public static ApiResponse CreateFailed(string message)
        {
            return new ApiResponse
            {
                Type = ApiResponseType.Failed,
                Message = message
            };
        }
    }

    public class ApiResponse<TData> : ApiResponse
    {
        public TData Data { get; set; }

        public static ApiResponse<TData> CreateSucceed(TData data)
        {
            return new ApiResponse<TData>
            {
                Type = ApiResponseType.Succeed,
                Data = data
            };
        }

        public new static ApiResponse<TData> CreateFailed(string message)
        {
            return new ApiResponse<TData>
            {
                Type = ApiResponseType.Failed,
                Message = message
            };
        }
    }
}
