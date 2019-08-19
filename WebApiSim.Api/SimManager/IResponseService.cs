using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebApiSim.Domain.Contracts;

namespace WebApiSim.Api.SimManager
{
    public interface IResponseService
    {
        ApiResponse AddResponses(AddResponsesRequest request);
        ApiResponse<IEnumerable<SimResponse>> SelectResponsesByApplicationId(SelectResponsesByApplicationIdRequest request);
        ApiResponse ClearResponsesByApplicationId(ClearResponsesRequest request);
    }

    public class AddResponsesRequest : ApplicationRequest
    {
        [Required]
        public SimResponse[] Responses { get; set; }
    }

    public class SelectResponsesByApplicationIdRequest : ApplicationRequest { }

    public class ClearResponsesRequest : ApplicationRequest { }
}
