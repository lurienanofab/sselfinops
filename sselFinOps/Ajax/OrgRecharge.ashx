<%@ WebHandler Language="C#" Class="sselFinOps.Ajax.OrgRecharge" %>

using System;
using System.Collections;
using System.Web;
using System.Web.SessionState;
using LNF.Repository;
using Newtonsoft.Json;

namespace sselFinOps.Ajax
{
    public class OrgRecharge : IHttpHandler, IReadOnlySessionState
    {

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            IEnumerable data = null;
            try
            {
                string command = context.Request["command"];
                switch (command)
                {
                    case "udpate-org-recharge":
                        int orgRechargeId = int.Parse(context.Request["OrgRechargeID"]);
                        DateTime enableDate = DateTime.Parse(context.Request["EnableDate"]);
                        var item = DA.Current.Single<LNF.Repository.Billing.OrgRecharge>(orgRechargeId);
                        if (item == null) throw new Exception(string.Format("Cannot not find an OrgRecharge record with OrgRechargeID = {0}", orgRechargeId));
                        item.EnableDate = enableDate;
                        context.Response.Write(GetJson(new { Success = true, Message = string.Empty, Data = enableDate.ToString() }));
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            catch (Exception ex)
            {
                context.Response.Write(GetJson(new { Success = false, Message = ex.Message, Data = data }));
            }
        }

        private string GetJson(object value)
        {
            return JsonConvert.SerializeObject(value);
        }
    }
}
