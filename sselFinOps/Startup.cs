using LNF.Impl;
using Microsoft.Owin;
using System.Web.Routing;

[assembly: OwinStartup(typeof(sselFinOps.Startup))]

namespace sselFinOps
{
    public class Startup : OwinStartup
    {
        public override void ConfigureRoutes(RouteCollection routes)
        {
            //nothing to do here...
        }
    }
}