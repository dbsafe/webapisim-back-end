using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApiSim.Api.Contracts;

namespace WebApiSim.Api.Controllers
{
    public class AppSimController<TController> : ControllerBase
    {
        protected ILogger<TController> _logger;

        public AppSimController(ILogger<TController> logger)
        {
            _logger = logger;
        }

        protected IActionResult Execute<TResult>(Func<TResult> func)
            where TResult: ApiResponse
        {
            try
            {
                var result = func();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception: {ex.ToString()}");
                return StatusCode(500, ApiResponse.CreateFailed("Internal Server Error"));
            }
        }
    }
}