using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WebApiSim.Domain.Contracts;

namespace WebApiSim.Loader
{
    public class WebApiSimLoaderConfig
    {
        public string Url { get; set; }
    }

    public class WebApiSimLoader
    {
        private readonly WebApiSimLoaderConfig _config;

        public WebApiSimLoader(WebApiSimLoaderConfig config)
        {
            _config = config;
        }

        public async Task LoadAsync(string pathToJsonFile)
        {
            var jsonText = File.ReadAllText(pathToJsonFile);
            using (var client = new HttpClient())
            {
                var httpContent = GetHttpContent(jsonText);
                var httpResponse = await client.PostAsync(_config.Url, httpContent);
                await ProcessLoadResponse(httpResponse);
            }
        }

        private async Task ProcessLoadResponse(HttpResponseMessage httpResponse)
        {
            if (httpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Request failed with StatusCode: '{(int)httpResponse.StatusCode}'");
            }

            string content = null;
            if (httpResponse.Content != null)
            {
                content = await httpResponse.Content.ReadAsStringAsync();
            }

            if (string.IsNullOrEmpty(content))
            {
                throw new Exception("Request returned an empty content'");
            }

            try
            {
                var response = JsonConvert.DeserializeObject<ApiResponse>(content);

                if(response.Type != ApiResponseType.Succeed)
                {
                    throw new Exception($"Request failed with response Type: '{response.Type}'");
                }
            }
            catch (Exception ex)
            {
                var message = $"Unable to decode the response.";
                throw new Exception(message, ex);
            }
        }

        private StringContent GetHttpContent(string content)
        {
            if (content == null)
            {
                return new StringContent(string.Empty);
            }

            return new StringContent(content, null, "application/json");
        }
    }
}
