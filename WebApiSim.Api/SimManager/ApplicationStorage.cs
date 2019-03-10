using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using WebApiSim.Api.Contracts;

namespace WebApiSim.Api.SimManager
{
    public class ApplicationStorage : IResponseService, IApplicationService, IRuleService, IWebApiSimManager
    {
        private readonly object _lock = new object();
        private readonly Dictionary<string, Application> _applications = new Dictionary<string, Application>();
        private readonly ILogger _applicationLogger;

        private SimResponse CreateDefaultResponse() => new SimResponse
        {
            StatusCode = 200,
            Headers = new KeyValuePair<string, string[]>[] { new KeyValuePair<string, string[]>("simulated-response-type", new string[] { "default-response" }) }
        };

        private SimResponse CreateApplicationNotFoundResponse() => new SimResponse
        {
            StatusCode = 200,
            Headers = new KeyValuePair<string, string[]>[] { new KeyValuePair<string, string[]>("simulated-response-type", new string[] { "application-not-found" }) }
        };

        public const string ApiSimRouteToken = "api-sim";

        public ApplicationStorage(ILoggerFactory loggerFactory)
        {
            _applicationLogger = loggerFactory.CreateLogger<Application>();
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

        #region  IWebApiSimManager

        public async Task<SimResponse> FindRuleByRequestAsync(HttpRequest request)
        {
            if (!request.Path.HasValue)
            {
                throw new InvalidOperationException("Path must has a value");
            }

            // url format: /api-sim/{applicationId}/other-values}
            var pathParts = request.Path.Value.Split('/');
            var tokenIndex = pathParts.IndexOf(ApiSimRouteToken);
            if (tokenIndex == -1)
            {
                throw new InvalidOperationException($"Path must contain token '{ApiSimRouteToken}'");
            }

            if (pathParts.Length > tokenIndex + 1)
            {
                var applicationId = pathParts[tokenIndex + 1];
                var applicationFound = TryGetApplicationSafe(applicationId, out Application application);
                if (!applicationFound)
                {
                    return CreateApplicationNotFoundResponse();
                }

                var responseForApplication = await application.FindResponseByRequestAsync(request);
                if (responseForApplication != null)
                {
                    return responseForApplication;
                }
            }

            return CreateDefaultResponse();
        }

        #endregion

        private Application GetOrCreateApplicationSafe(string applicationId)
        {
            Application application;
            lock (_lock)
            {
                if (!_applications.TryGetValue(applicationId, out application))
                {
                    application = new Application(applicationId, _applicationLogger);
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
        private readonly ILogger _logger;

        public string ApplicationId { get; private set; }

        public Application(string applicationId, ILogger logger)
        {
            ApplicationId = applicationId;
            _logger = logger;
        }

        public void AddResponses(IEnumerable<SimResponse> responses)
        {
            lock (_lock)
            {
                foreach (var response in responses)
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

        public async Task<SimResponse> FindResponseByRequestAsync(HttpRequest request)
        {
            SimResponse FindResponse(Guid responseId)
            {
                if (!_responses.ContainsKey(responseId))
                {
                    throw new InvalidOperationException($"Response {responseId} not found");
                }

                return _responses[responseId];
            }

            var bodyText = string.Empty;
            using (var bodyStream = new MemoryStream())
            {
                await request.Body.CopyToAsync(bodyStream);
                bodyStream.Seek(0, SeekOrigin.Begin);
                bodyText = new StreamReader(bodyStream).ReadToEnd();
            }

            JObject bodyObject = string.IsNullOrEmpty(bodyText) ? null : JObject.Parse(bodyText);

            lock (_lock)
            {
                IEnumerable<SimRule> rules = _rules.Values.Where(rule => string.IsNullOrEmpty(rule.Method) || rule.Method == request.Method);
                rules = rules
                    .Where(rule => !rule.Header.HasValue ||
                        request.Headers.Any(requestHeader => requestHeader.Key == rule.Header.Value.Key && requestHeader.Value.Any(requestHeaderValue => requestHeaderValue == rule.Header.Value.Value)));

                if (bodyObject != null)
                {
                    rules = rules.Where(rule => !rule.Property.HasValue ||
                        bodyObject.ContainsKey(rule.Property.Value.Key) && bodyObject.GetValue(rule.Property.Value.Key).ToString() == rule.Property.Value.Value);
                }

                switch (rules.Count())
                {
                    case 0:
                        return null;
                    case 1:
                        return FindResponse(rules.First().ResponseId);                        
                    default:
                        _logger.LogWarning($"More than one rule found. Rules: {string.Join(',', rules.Select(a => a.RuleId))}");
                        return FindResponse(rules.First().ResponseId);
                }
            }
        }
    }

    public class SimResponse
    {
        public Guid ResponseId { get; set; }
        public object Body { get; set; }
        public int StatusCode { get; set; }
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
