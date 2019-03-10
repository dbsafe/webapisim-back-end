using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using WebApiSim.Api.SimManager;

namespace WebApiSim.Api.Middleware.WebApiSim
{
    public static class WebApiSimMiddlewareExtensions
    {
        public static IApplicationBuilder UseWebApiSim(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<WebApiSimMiddleware>();
        }
    }

    public class WebApiSimMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly IWebApiSimManager _webApiSimManager;

        public WebApiSimMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, IWebApiSimManager webApiSimManager)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<WebApiSimMiddleware>();
            _webApiSimManager = webApiSimManager;
        }

        public async Task Invoke(HttpContext context)
        {
            var usingSimulatedResponse = await BuildSimulatedResponseAsync(context);
            if (!usingSimulatedResponse)
            {
                await _next(context);
            }
        }

        private async Task<bool> BuildSimulatedResponseAsync(HttpContext context)
        {
            var path = context.Request.Path.HasValue ? context.Request.Path.Value : string.Empty;
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            var isSimulatorRequest = path.Contains($"/{ApplicationStorage.ApiSimRouteToken}/");
            if (!isSimulatorRequest)
            {
                return false;
            }

            var simResponse = await _webApiSimManager.FindRuleByRequestAsync(context.Request);

            context.Response.StatusCode = simResponse.StatusCode;
            if (simResponse.Headers != null)
            {
                foreach (var header in simResponse.Headers)
                {
                    context.Response.Headers.Add(header.Key, header.Value);
                }
            }

            if (simResponse.Body != null)
            {
                var json = JsonConvert.SerializeObject(simResponse.Body);
                await context.Response.WriteAsync(json);
            }

            return true;
        }
    }
}
