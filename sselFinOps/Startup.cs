using LNF.Web;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(sselFinOps.Startup))]

namespace sselFinOps
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseDataAccess();
        }
    }
}