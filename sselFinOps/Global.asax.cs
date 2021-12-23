using LNF;
using LNF.Impl.DependencyInjection;
using LNF.Web;
using System;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Web.Compilation;
using System.Web.Security;

namespace sselFinOps
{
    public class Global : System.Web.HttpApplication
    {
        public static WebApp WebApp { get; private set; }

        protected void Application_Start(object sender, EventArgs e)
        {
            WebApp = new WebApp();

            // setup up dependency injection container
            WebApp.Context.EnablePropertyInjection();

            var wcc = new WebContainerConfiguration(WebApp.Context);
            wcc.RegisterAllTypes();

            // setup web dependency injection
            Assembly[] assemblies = BuildManager.GetReferencedAssemblies().Cast<Assembly>().ToArray();
            WebApp.Bootstrap(assemblies);

            if (ServiceProvider.Current.IsProduction())
                Application["AppServer"] = "http://" + Environment.MachineName + ".eecs.umich.edu/";
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            if (Request.IsAuthenticated)
            {
                FormsIdentity ident = (FormsIdentity)User.Identity;
                string[] roles = ident.Ticket.UserData.Split('|');
                Context.User = new GenericPrincipal(ident, roles);
            }
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            if (ServiceProvider.Current.IsProduction())
                Session["Logout"] = Convert.ToString(Application["AppServer"]) + ServiceProvider.Current.LoginUrl();
            else
                Session["Logout"] = ServiceProvider.Current.LoginUrl();
        }
    }
}