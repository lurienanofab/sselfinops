using LNF;
using LNF.Cache;
using LNF.Impl.Context;
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
            var ctx = new WebContext(new WebContextFactory());
            var ioc = new IOC(ctx);
            ServiceProvider.Current = ioc.Resolver.GetInstance<ServiceProvider>();

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
                Session["Logout"] = Convert.ToString(Application["AppServer"]) + ServiceProvider.Current.Context.LoginUrl;
            else
                Session["Logout"] = ServiceProvider.Current.Context.LoginUrl;
        }
    }
}