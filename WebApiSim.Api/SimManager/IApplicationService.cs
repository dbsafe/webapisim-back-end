using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebApiSim.Api.Contracts;

namespace WebApiSim.Api.SimManager
{
    public interface IApplicationService
    {
        ApiResponse Clear();
        ApiResponse<IEnumerable<string>> SelectApplicationIds();
    }

    public class ApplicationRequest
    {
        [Required]
        [MaxLength(100)]
        [MinLength(5)]
        public string ApplicationId { get; set; }
    }
}
