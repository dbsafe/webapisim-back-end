using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApiSim.Api.SimManager;
using WebApiSim.Domain.Models;

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
            return Execute(() => _applicationService.SelectApplicationIds());
        }

        [HttpPost("clear")]
        public IActionResult DeleteAllApplications()
        {
            return Execute(() => _applicationService.DeleteAllApplications());
        }

        [HttpPost("clear/{applicationId}")]
        public IActionResult DeleteApplication([FromRoute] string applicationId)
        {
            return Execute(() => _applicationService.DeleteApplication(applicationId));
        }

        [HttpPost("load")]
        public IActionResult Load(LoadRequest request)
        {
            return Execute(() => _applicationService.Load(request));
        }
    }
}