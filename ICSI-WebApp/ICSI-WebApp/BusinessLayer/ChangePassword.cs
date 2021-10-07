using ICSI_WebApp.Models;
using ICSI_WebApp.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ICSI_WebApp.BusinessLayer
{
    public class ChangePassword
    {
        public ActionClass beforeChangePassword(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            frm["s"] = "edit";
            //frm["ID"] = Convert.ToString(HttpContext.Current.Session["USER_ID"]);
            frm["ui"] = Convert.ToString(HttpContext.Current.Session["USER_ID"]);
            return Util.UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterChangePassword(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass act = null;
            int id = Convert.ToInt32(frm["ID"]);
            Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
            string applicationSchema = Util.UtilService.getApplicationScheme(screen);
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ACTIVE_YN", 1);
            conditions.Add("ID", id);
            JObject jdata = DBTable.GetData("fetch", conditions, screen.Table_Name_Tx, 0, 10, applicationSchema);
            DataTable dt = null;
            foreach (JProperty property in jdata.Properties())
            {
                if (property.Name.Equals(screen.Table_Name_Tx)) dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
            }
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                string pwd = frm["pwd"];
                string newpwd = frm["newpwd"];
                DataRow row = dt.Rows[0];
                if (Convert.ToString(row["LOGIN_PWD_TX"]) == pwd)
                {
                    frm["LOGIN_PWD_TX"] = newpwd;
                    act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
                }
                else
                {
                    act = Util.UtilService.GetMessage(-501);
                }
            }
            else
            {
                act = Util.UtilService.GetMessage(-202);
            }
            return act;
        }
    }
}