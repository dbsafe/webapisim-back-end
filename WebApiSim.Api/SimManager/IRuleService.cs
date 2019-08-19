using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebApiSim.Domain.Contracts;

namespace WebApiSim.Api.SimManager
{
    public interface IRuleService
    {
        ApiResponse AddRules(AddRulesRequest request);
        ApiResponse<IEnumerable<SimRule>> SelectRulesByApplicationId(SelectRulesByApplicationIdRequest request);
        ApiResponse ClearRulesByApplicationId(ClearRulesByApplicationIdRequest request);
    }

    public class AddRulesRequest : ApplicationRequest
    {
        [Required]
        public SimRule[] Rules { get; set; }
    }

    public class SelectRulesByApplicationIdRequest : ApplicationRequest { }

    public class ClearRulesByApplicationIdRequest : ApplicationRequest { }
}
