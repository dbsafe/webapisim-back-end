using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApiSim.Api.Contracts;
using WebApiSim.Api.SimManager;

namespace WebApiSim.Api.Controllers
{
    [Route("api/responses")]
    [ApiController]
    public class ResponseController : AppSimController<ResponseController>
    {
        public ResponseController(ILogger<ResponseController> logger)
            : base(logger)
        {
        }

        [HttpPost("addResponses")]
        public IActionResult Add([FromBody] AddResponsesRequest request)
        {
            return Execute(() =>
            {
                // throw new Exception("error");
                return ApiResponse.CreateSucceed();
            });
        }

        [HttpPost("responsesByApplicationId")]
        public IActionResult Select([FromBody] SelectResponsesByApplicationIdRequest request)
        {
            return Ok(new SimResponse[0]);
        }

        [HttpPost("clearResponses")]
        public ActionResult Clear([FromBody] ClearResponsesRequest request)
        {
            return Ok();
        }
    }
}