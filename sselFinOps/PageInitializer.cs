using LNF.Web;
using System.Web;

[assembly: PreApplicationStartMethod(typeof(sselFinOps.PageInitializer), "Initialize")]

namespace sselFinOps
{
    public class PageInitializer
    {
        public static void Initialize()
        {
            PageInitializerModule.Initialize();
        }
    }
}