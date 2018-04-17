using LNF;
using LNF.Cache;
using LNF.Impl.DependencyInjection.Web;
using System;
using System.Security.Principal;
using System.Web.Security;

namespace sselFinOps
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            ServiceProvider.Current = IOC.Resolver.GetInstance<ServiceProvider>();

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
                CacheManager.Current.Logout = Application["AppServer"].ToString() + "login";
            else
                CacheManager.Current.Logout = "/login";
        }
    }
}