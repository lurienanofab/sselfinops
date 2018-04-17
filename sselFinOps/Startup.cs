using LNF;
using LNF.Impl.DependencyInjection.Web;
using LNF.Web;
using Microsoft.Owin;
using Owin;
using System.Web.Routing;

[assembly: OwinStartup(typeof(sselFinOps.Startup))]

namespace sselFinOps
{
    public class Startup : OwinStartup
    {
        public override void ConfigureRoutes(RouteCollection routes)
        {
            // nothing to do here...
        }
    }
}