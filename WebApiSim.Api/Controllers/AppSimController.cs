using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApiSim.Api.SimManager;
using WebApiSim.Domain.Contracts;

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
            where TResult : ApiResponse
        {
            try
            {
                var result = func();
                return Ok(result);
            }
            catch (BadRequestException ex)
            {
                _logger.LogError($"Exception: {ex.ToString()}");
                return StatusCode((int)HttpStatusCode.BadRequest, ApiResponse.CreateFailed(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception: {ex.ToString()}");
                return StatusCode((int)HttpStatusCode.OK, ApiResponse.CreateFailed("Internal Server Error"));
            }
        }
    }
}