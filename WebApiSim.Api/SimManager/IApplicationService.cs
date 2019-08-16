using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebApiSim.Api.Contracts;

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

    public class LoadRequest
    {
        public string ApplicationId { get; set; }

        public RuleWithResponse[] RuleWithResponses { get; set; }
    }

    public class RuleWithResponse
    {
        public LoadRule Rule { get; set; }

        public LoadResponse Response { get; set; }
    }

    public class LoadRule
    {
        public KeyValuePair<string, string>? Header { get; set; }

        public string Method { get; set; }

        public KeyValuePair<string, string>? Parameter { get; set; }

        public KeyValuePair<string, string>? Property { get; set; }

        public string Url { get; set; }

    }

    public class LoadResponse
    {
        public object Body { get; set; }

        public KeyValuePair<string, string[]>[] Headers { get; set; }

        public int StatusCode { get; set; }
    }
}
