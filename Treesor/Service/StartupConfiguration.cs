using NLog;
using Owin;
using Swashbuckle.Application;
using System.IO;
using System.Reflection;
using System.Web.Http;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

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
                    c.IncludeXmlComments(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Treesor.Xml"));
                    //c.CustomProvider(defaultProvider => new ODataSwaggerProvider(defaultProvider, c, config));
                })
                .EnableSwaggerUi();

            //config.EnsureInitialized();
            //config.Routes.ToList().ForEach(r => log.Info().Message($"Route: {r.GetVirtualPath().Route}").Write());
            appBuilder.UseWebApi(config);
        }
    }
}