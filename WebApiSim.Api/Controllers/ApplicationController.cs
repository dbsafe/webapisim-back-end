using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApiSim.Api.SimManager;

namespace WebApiSim.Api.Controllers
{
    [Route("api/application")]
    [ApiController]
    public class ApplicationController : AppSimController<ApplicationController>
    {
        private IApplicationService _applicationService;

        public ApplicationController(ILogger<ApplicationController> logger, IApplicationService applicationService)
            : base(logger)
        {
            _applicationService = applicationService;
        }

        [HttpGet("applicationIds")]
        public IActionResult Select()
        {
            return Execute(() =>
            {
                return _applicationService.SelectApplicationIds();
            });
        }

        [HttpPost("clear")]
        public IActionResult Clear()
        {
            return Execute(() =>
            {
                return _applicationService.Clear();
            });
        }
    }
}