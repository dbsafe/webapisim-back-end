using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApiSim.Api.SimManager;

namespace WebApiSim.Api.Controllers
{
    [Route("api/response")]
    [ApiController]
    public class ResponseController : AppSimController<ResponseController>
    {
        private IResponseService _responseService;

        public ResponseController(ILogger<ResponseController> logger, IResponseService responseService)
            : base(logger)
        {
            _responseService = responseService;
        }

        [HttpPost("add")]
        public IActionResult Add([FromBody] AddResponsesRequest request)
        {
            return Execute(() =>
            {
                return _responseService.AddResponses(request.ApplicationId, request.Responses);
            });
        }

        [HttpPost("responsesByApplicationId")]
        public IActionResult Select([FromBody] SelectResponsesByApplicationIdRequest request)
        {
            return Execute(() =>
            {
                return _responseService.SelectResponsesByApplicationId(request.ApplicationId);
            });
        }

        [HttpPost("clear")]
        public IActionResult Clear([FromBody] ClearResponsesRequest request)
        {
            return Execute(() =>
            {
                return _responseService.ClearResponsesByApplicationId(request.ApplicationId);
            });
        }
    }
}