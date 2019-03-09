using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using WebApiSim.Api.Contracts;

namespace WebApiSim.Api.SimManager
{
    #region IApplicationService

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

    #endregion

    #region IResponseService

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

    #endregion

    #region IRuleService

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

    #endregion

    public class ApplicationStorage : IResponseService, IApplicationService, IRuleService
    {
        private readonly object _lock = new object();
        private readonly Dictionary<string, Application> _applications = new Dictionary<string, Application>();

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

        public ApiResponse AddResponses(AddResponsesRequest request)
        {
            var application = GetOrCreateApplicationSafe(request.ApplicationId);
            application.AddResponses(request.Responses);
            return ApiResponse.CreateSucceed();
        }

        public ApiResponse ClearResponsesByApplicationId(ClearResponsesRequest request)
        {
            bool applicationFound = TryGetApplicationSafe(request.ApplicationId, out Application application);
            if (applicationFound)
            {
                application.ClearResponses();
            }

            return ApiResponse.CreateSucceed();
        }

        public ApiResponse<IEnumerable<SimResponse>> SelectResponsesByApplicationId(SelectResponsesByApplicationIdRequest request)
        {
            bool applicationFound = TryGetApplicationSafe(request.ApplicationId, out Application application);
            if (applicationFound)
            {
                return ApiResponse<IEnumerable<SimResponse>>.CreateSucceed(application.GetResponses());
            }
            else
            {
                return ApiResponse<IEnumerable<SimResponse>>.CreateSucceed(new SimResponse[0]);
            }
        }

        #endregion

        #region IRuleService

        public ApiResponse AddRules(AddRulesRequest request)
        {
            var application = GetOrCreateApplicationSafe(request.ApplicationId);
            var added = application.AddRules(request.Rules, out string message);
            if (added)
            {
                return ApiResponse.CreateSucceed();
            }
            else
            {
                return ApiResponse.CreateFailed(message);
            }
        }

        public ApiResponse<IEnumerable<SimRule>> SelectRulesByApplicationId(SelectRulesByApplicationIdRequest request)
        {
            bool applicationFound = TryGetApplicationSafe(request.ApplicationId, out Application application);
            if (applicationFound)
            {
                return ApiResponse<IEnumerable<SimRule>>.CreateSucceed(application.GetRules());
            }
            else
            {
                return ApiResponse<IEnumerable<SimRule>>.CreateSucceed(new SimRule[0]);
            }
        }

        public ApiResponse ClearRulesByApplicationId(ClearRulesByApplicationIdRequest request)
        {
            bool applicationFound = TryGetApplicationSafe(request.ApplicationId, out Application application);
            if (applicationFound)
            {
                application.ClearRules();
            }

            return ApiResponse.CreateSucceed();
        }

        #endregion

        private Application GetOrCreateApplicationSafe(string applicationId)
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

        private bool TryGetApplicationSafe(string applicationId, out Application application)
        {
            lock (_lock)
            {
                return _applications.TryGetValue(applicationId, out application);
            }
        }
    }

    public class Application
    {
        private readonly object _lock = new object();
        private readonly Dictionary<Guid, SimResponse> _responses = new Dictionary<Guid, SimResponse>();
        private readonly Dictionary<Guid, SimRule> _rules = new Dictionary<Guid, SimRule>();

        public string ApplicationId { get; private set; }

        public Application(string applicationId)
        {
            ApplicationId = applicationId;
        }

        public void AddResponses(IEnumerable<SimResponse> responses)
        {
            lock (_lock)
            {
                foreach(var response in responses)
                {
                    _responses[response.ResponseId] = response;
                }
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
                return _responses.Values.ToArray();
            }
        }

        public bool AddRules(IEnumerable<SimRule> rules, out string message)
        {
            lock (_lock)
            {
                var responsesNotFound = rules
                    .Where(a => !_responses.ContainsKey(a.ResponseId))
                    .Select(a => a.ResponseId)
                    .ToArray();
                if (responsesNotFound.Length > 0)
                {
                    message = $"One or more responses were not found: {string.Join(',', responsesNotFound)}";
                    return false;
                }

                foreach (var rule in rules)
                {
                    _rules[rule.RuleId] = rule;
                }
            }

            message = string.Empty;
            return true;
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
                return _rules.Values.ToArray();
            }
        }
    }

    public class SimResponse
    {
        public Guid ResponseId { get; set; }
        public object Body { get; set; }
        public int HttpCode { get; set; }
        public KeyValuePair<string, string[]>[] Headers { get; set; }
    }

    public class SimRule
    {
        public Guid RuleId { get; set; }
        public Guid ResponseId { get; set; }
        public string Method { get; set; }
        public KeyValuePair<string, string>? Header { get; set; }
        public KeyValuePair<string, string>? Property { get; set; }
    }
}
