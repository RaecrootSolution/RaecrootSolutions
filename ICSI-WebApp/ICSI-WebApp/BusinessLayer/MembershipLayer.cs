using ICSI_WebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ICSI_WebApp.BusinessLayer
{
    public class MembershipLayer
    {
        public ActionClass searchCertificateDocApproval(int WEB_APP_ID, FormCollection frm, string ScreenType, string sid, string screenId = "", Screen_T screen = null)
        {
            frm.Remove("SCRH_MEMBER_NO_TX");
            frm.Remove("COND_MEMBER_NO_TX");
            frm.Add("COND_MEMBER_NO_TX", "AND =");
            frm.Add("SCRH_MEMBER_NO_TX", Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]));
            return Util.UtilService.searchLoad(WEB_APP_ID, frm, ScreenType, Convert.ToString(sid));
        }
    }
}