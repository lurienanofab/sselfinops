using LNF.Cache;
using LNF.Web;
using sselFinOps.AppCode;
using System.IO;
using System.Web;
using System.Web.SessionState;

namespace sselFinOps
{
    /// <summary>
    /// Summary description for Spreadsheets
    /// </summary>
    public class Spreadsheets : IHttpHandler, IReadOnlySessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            int clientId = CacheManager.Current.CurrentUser.ClientID;
            string name = context.Request.QueryString["name"];
            string type = context.Request.QueryString["type"];

            string path;

            if (!string.IsNullOrEmpty(name))
            {
                if (type == "zip")
                {
                    path = ZipUtility.CreateArchive(ExcelUtility.GetWorkPath(clientId), name);

                    if (File.Exists(path))
                    {
                        context.Response.ContentType = "application/zip";
                        context.Response.AddHeader("Content-Disposition", string.Format(@"attachment;filename=""{0}""", Path.GetFileName(path)));
                        context.Response.WriteFile(path);
                        return;
                    }
                }
                else
                {
                    path = Path.Combine(ExcelUtility.GetWorkPath(clientId), name);
                    
                    if (File.Exists(path))
                    {
                        context.Response.ContentType = "application/vnd.ms-excel";
                        context.Response.AddHeader("Content-Disposition", string.Format(@"attachment;filename=""{0}""", Path.GetFileName(path)));
                        context.Response.WriteFile(path);
                        return;
                    }
                }
            }

            throw new HttpException(404, "File not found");
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}