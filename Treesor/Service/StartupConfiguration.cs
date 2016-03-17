using NLog;
using NLog.Fluent;
using Owin;
using Swashbuckle.Application;
using System.Linq;
using System.Web.Http;

namespace Treesor.Service

{
    public sealed class StartupConfiguration
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public void Configuration(IAppBuilder appBuilder)
        {
            HttpConfiguration config = new HttpConfiguration();

            //ODataModelBuilder builder = new ODataConventionModelBuilder();
            //builder.EntitySet<Entity>("Entities");

            config.MapHttpAttributeRoutes();

            //config.MapODataServiceRoute(
            //    routeName: "ODataRoute",
            //    routePrefix: "odata",
            //    model: builder.GetEdmModel());

            config
                .EnableSwagger(c =>
                {
                    c.SingleApiVersion("v1", "Treesor");
                    //c.CustomProvider(defaultProvider => new ODataSwaggerProvider(defaultProvider, c, config));
                })
                .EnableSwaggerUi();

            //config.EnsureInitialized();
            //config.Routes.ToList().ForEach(r => log.Info().Message($"Route: {r.GetVirtualPath().Route}").Write());
            appBuilder.UseWebApi(config);
        }
    }
}