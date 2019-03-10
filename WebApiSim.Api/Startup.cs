using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NLog.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;
using WebApiSim.Api.Middleware.Logging;
using WebApiSim.Api.Middleware.WebApiSim;
using WebApiSim.Api.SimManager;

namespace WebApiSim.Api
{
    public class Startup
    {
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, ILogger<Startup> logger, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;
            _logger = logger;
            _loggerFactory = loggerFactory;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
            .AddJsonOptions(o =>
            {
                //if (o.SerializerSettings.ContractResolver != null)
                //{
                //    var castedResolver = o.SerializerSettings.ContractResolver as Newtonsoft.Json.Serialization.DefaultContractResolver;
                //    castedResolver.NamingStrategy = null; // manipulate json serializer setting to use uppercase for prop names.
                //}

                o.SerializerSettings.Converters.Add(new StringEnumConverter());
                o.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            });

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "WebApiSim", Version = "v1" });
            });


            ConfigureDependencyInjection(services, _loggerFactory);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                _logger.LogInformation("In Development environment");
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            loggerFactory.AddNLog();
            app.UseRequestResponseLogging();
            app.UseWebApiSim();
            ConfigureSwagerUi(app);

            app.UseMvc();

            //app.Run(async (context) =>
            //{
            //    await context.Response.WriteAsync("Hello World!");
            //});
        }

        private void ConfigureSwagerUi(IApplicationBuilder app)
        {
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApiSim V1");
                c.RoutePrefix = string.Empty;
            });
        }

        private void ConfigureDependencyInjection(IServiceCollection services, ILoggerFactory loggerFactory)
        {
            var applicationStorage = new ApplicationStorage(loggerFactory);
            services.AddSingleton<IApplicationService>(applicationStorage);
            services.AddSingleton<IResponseService>(applicationStorage);
            services.AddSingleton<IRuleService>(applicationStorage);
            services.AddSingleton<IWebApiSimManager>(applicationStorage);
        }
    }
}
