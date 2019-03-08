using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using WebApiSim.Api.Contracts;

namespace WebApiSim.Api.SimManager
{
    public class ApplicationRequest
    {
        [Required]
        [MaxLength(100)]
        [MinLength(5)]
        public string ApplicationId { get; set; }
    }

    public class AddResponsesRequest : ApplicationRequest
    {
        [Required]
        public SimResponse[] Responses { get; set; }
    }

    public class SelectResponsesByApplicationIdRequest : ApplicationRequest
    {
    }


    public class ClearResponsesRequest : ApplicationRequest
    {
    }

    public interface IResponseService
    {
        ApiResponse AddResponses(string applicationId, IEnumerable<SimResponse> responses);
        ApiResponse<IEnumerable<SimResponse>> SelectResponsesByApplicationId(string applicationId);
        ApiResponse ClearResponsesByApplicationId(string applicationId);
    }

    public interface IApplicationService
    {
        ApiResponse Clear();
        ApiResponse<IEnumerable<string>> SelectApplicationIds();
    }

    public class ApplicationStorage : IResponseService, IApplicationService
    {
        private readonly object _lock = new object();
        private readonly Dictionary<string, Application> _applications = new Dictionary<string, Application>();

        public ApiResponse AddResponses(string applicationId, IEnumerable<SimResponse> responses)
        {
            var application = GetOrCreateApplication(applicationId);
            application.AddResponses(responses);
            return ApiResponse.CreateSucceed();
        }

        #region IApplicationStore

        public ApiResponse Clear()
        {
            lock (_lock)
            {
                _applications.Clear();
            }

            return ApiResponse.CreateSucceed();
        }

        public ApiResponse<IEnumerable<string>> SelectApplicationIds()
        {
            IEnumerable<string> applications;
            lock (_lock)
            {
                applications = _applications.Keys.ToArray();
            }

            return ApiResponse<IEnumerable<string>>.CreateSucceed(applications);
        }

        #endregion

        #region IResponseService

        public ApiResponse ClearResponsesByApplicationId(string applicationId)
        {
            Application application;
            bool applicationFound = false;
            lock (_lock)
            {
                applicationFound = _applications.TryGetValue(applicationId, out application);
            }

            if (applicationFound)
            {
                application.ClearResponses();
            }

            return ApiResponse.CreateSucceed();
        }

        public ApiResponse<IEnumerable<SimResponse>> SelectResponsesByApplicationId(string applicationId)
        {
            Application application;
            bool applicationFound = false;
            lock (_lock)
            {
                applicationFound = _applications.TryGetValue(applicationId, out application);
            }

            if (applicationFound)
            {
                return ApiResponse<IEnumerable<SimResponse>>.CreateSucceed(application.GetResponses());
            }
            else
            {
                return ApiResponse<IEnumerable<SimResponse>>.CreateSucceed(new SimResponse[0]);
            }
        }

        private Application GetOrCreateApplication(string applicationId)
        {
            Application application;
            lock (_lock)
            {
                if (!_applications.TryGetValue(applicationId, out application))
                {
                    application = new Application(applicationId);
                    _applications.Add(applicationId, application);
                }
            }

            return application;
        }

        #endregion
    }

    public class Application
    {
        private readonly object _lock = new object();
        private readonly List<SimResponse> _responses = new List<SimResponse>();
        private readonly List<SimRule> _rules = new List<SimRule>();

        public string ApplicationId { get; private set; }

        public Application(string applicationId)
        {
            ApplicationId = applicationId;
        }

        public void AddResponses(IEnumerable<SimResponse> responses)
        {
            lock (_lock)
            {
                _responses.AddRange(responses);
            }
        }

        public void ClearResponses()
        {
            lock (_lock)
            {
                _responses.Clear();
            }
        }

        public IEnumerable<SimResponse> GetResponses()
        {
            lock (_lock)
            {
                return _responses.ToArray();
            }
        }

        public void AddRules(IEnumerable<SimRule> rules)
        {
            lock (_lock)
            {
                _rules.AddRange(rules);
            }
        }

        public void ClearRules()
        {
            lock (_lock)
            {
                _rules.Clear();
            }
        }

        public IEnumerable<SimRule> GetRules()
        {
            lock (_lock)
            {
                return _rules.ToArray();
            }
        }
    }

    public class SimResponse
    {
        public Guid ResponseId { get; set; }
        public object Body { get; set; }
        public string HttpCode { get; set; }
        public KeyValuePair<string, string[]>[] Headers { get; set; }
    }

    public class SimRule
    {
        public Guid RuleId { get; set; }
        public string Method { get; set; }
        public string Header { get; set; }
        public object Body { get; set; }
    }
}
