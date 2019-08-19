using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebApiSim.Domain.Contracts;
using WebApiSim.Domain.Models;

namespace WebApiSim.Api.SimManager
{
    public interface IApplicationService
    {
        ApiResponse DeleteAllApplications();

        ApiResponse DeleteApplication(string applicationId);

        ApiResponse<IEnumerable<string>> SelectApplicationIds();

        ApiResponse Load(LoadRequest request);
    }

    public class ApplicationRequest
    {
        [Required]
        [MaxLength(100)]
        [MinLength(5)]
        public string ApplicationId { get; set; }
    }
}
