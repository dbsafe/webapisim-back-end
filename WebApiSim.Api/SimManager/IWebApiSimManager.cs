using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace WebApiSim.Api.SimManager
{
    public interface IWebApiSimManager
    {
        Task<SimResponse> FindRuleByRequestAsync(HttpRequest request);
    }
}
