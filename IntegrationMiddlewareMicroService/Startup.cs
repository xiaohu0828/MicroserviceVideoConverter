using System.Net.Http.Headers;
using System.Web.Http;
using Owin;

namespace IntegrationMiddlewareMicroService
{
    public  class Startup : IOwinAppBuilder
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var config = new HttpConfiguration();
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
            config.MapHttpAttributeRoutes();
            FormatterConfig.ConfigureFormatters(config.Formatters);
            appBuilder.UseWebApi(config);
        }
    }
}
