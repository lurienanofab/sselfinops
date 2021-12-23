using LNF.Web;
using System.Web;

[assembly: PreApplicationStartMethod(typeof(sselFinOps.PageInitializer), "Initialize")]

namespace sselFinOps
{
    public class PageInitializer : PageInitializerModule
    {
        public static void Initialize()
        {
            RegisterModule(typeof(PageInitializer));
        }
    }
}