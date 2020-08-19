using System.ServiceModel;
using EventsSpontaneousApi.Services;
using Hafslund.Configuration;
using Hafslund.Telemetry;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SoapCore;

namespace EventsSpontaneousApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddStandardHafslundNettTelemetryLogging(
                Configuration.EnsureHasValue("hes-extensions:kv:appinsights:hes-extensions:instrumentation-key"),
                true);

            services.AddSoapCore();
            services.AddMvc();
            services.AddSoapExceptionTransformer((ex) => ex.Message);
      
            services.AddSingleton<IEventHubService, EventHubService>(s => 
                    new EventHubService(
                        Configuration.EnsureHasValue("ami-timeseries:kv:eventhubnamespaces:ami-timeseries:eventhubs:alarmsandevents:authorizationrules:send:primary_connection_string"),
                        Configuration.EnsureHasValue("EventHub:eventHubName")
                        ));

            services.AddSingleton<IIECReceiveEvents, ReceiveEventsService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.UseSoapEndpoint<IIECReceiveEvents>("/event", new BasicHttpsBinding(), SoapSerializer.XmlSerializer);
            });
        }
    }
}
