using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApiSim.Api.SimManager;

namespace WebApiSim.Api.Controllers
{
    [Route("api/rule")]
    [ApiController]
    public class RuleController : AppSimController<RuleController>
    {
        private readonly IRuleService _ruleService;

        public RuleController(ILogger<RuleController> logger, IRuleService ruleService)
            : base(logger)
        {
            _ruleService = ruleService;
        }

        [HttpPost("add")]
        public IActionResult Add([FromBody] AddRulesRequest request)
        {
            return Execute(() => _ruleService.AddRules(request));
        }

        [HttpPost("rulesByApplicationId")]
        public IActionResult Select([FromBody] AddRulesRequest request)
        {
            return Execute(() => _ruleService.AddRules(request));
        }

        [HttpPost("clear")]
        public IActionResult Clear([FromBody] ClearRulesByApplicationIdRequest request)
        {
            return Execute(() => _ruleService.ClearRulesByApplicationId(request));
        }
    }
}