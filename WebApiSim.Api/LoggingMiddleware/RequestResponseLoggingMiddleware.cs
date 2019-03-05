using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace WebApiSim.Api.LoggingMiddleware
{
    public static class RequestResponseLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestResponseLoggingMiddleware>();
        }
    }

    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory
                      .CreateLogger<RequestResponseLoggingMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            _logger.LogInformation(await FormatRequest(context.Request));

            var originalBodyStream = context.Response.Body;
            using (var responseBody = new MemoryStream())
            {
                context.Response.Body = responseBody;

                await _next(context);

                _logger.LogInformation(await FormatResponse(context.Response));
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }

        private async Task<string> FormatRequest(HttpRequest request)
        {
            var bodyStream = new MemoryStream();
            await request.Body.CopyToAsync(bodyStream);
            bodyStream.Seek(0, SeekOrigin.Begin);
            var bodyText = new StreamReader(bodyStream).ReadToEnd();
            bodyStream.Seek(0, SeekOrigin.Begin);

            request.Body.Dispose();
            request.Body = bodyStream;

            var url = UriHelper.GetDisplayUrl(request);
            var headers = GetDisplayHeaders(request.Headers);
            return $"REQUEST\nMETHOD: {request.Method}\nURL: {url}\nHEADERS:\n{headers}\nBODY: {bodyText}";
        }

        private async Task<string> FormatResponse(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var bodyText = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            var headers = GetDisplayHeaders(response.Headers);
            return $"RESPONSE\nHEADERS:\n{headers}\nBODY: {bodyText}";
        }

        private string GetDisplayHeaders(IHeaderDictionary headers)
        {
            var sb = new StringBuilder();

            foreach (var header in headers)
            {
                sb.AppendLine($"{header.Key}:{header.Value}");
            }

            return sb.ToString();
        }
    }
}
