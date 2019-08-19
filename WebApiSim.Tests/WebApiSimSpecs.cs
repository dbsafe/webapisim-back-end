using Microsoft.VisualStudio.TestTools.UnitTesting;
using Specflow.Steps.WebApi;
using System.IO;
using System.Reflection;
using TechTalk.SpecFlow;
using WebApiSim.Loader;

namespace WebApiSim.Tests
{
    [Binding]
    [Scope(Feature = "ApplicationController")]
    [Scope(Feature = "AppSimLogic")]
    public class WebApiSimSpecs : WebApiSpecs
    {
        private static readonly WebApiSpecsConfig _config = new WebApiSpecsConfig
        {
            BaseUrl = "http://localhost:9000"
        };

        public WebApiSimSpecs(TestContext testContext) : base(testContext, _config) { }

        [Given(@"the api is initialized")]
        public void InitializeApi()
        {
            var webApiSimLoaderConfig = new WebApiSimLoaderConfig { Url = $"{_config.BaseUrl}/api/application/load" };
            var loader = new WebApiSimLoader(webApiSimLoaderConfig);

            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var pathToJsonFile = Path.Combine(path, "webapisim-test-data-application-1.json");
            loader.LoadAsync(pathToJsonFile).Wait();

            pathToJsonFile = Path.Combine(path, "webapisim-test-data-application-2.json");
            loader.LoadAsync(pathToJsonFile).Wait();
        }

        [Then(@"the request should succeed")]
        public void AssertRequestSucceeded()
        {
            AssertStatusCode(200);
            AssertTextProperty("type", "Succeed");
        }
    }
}
