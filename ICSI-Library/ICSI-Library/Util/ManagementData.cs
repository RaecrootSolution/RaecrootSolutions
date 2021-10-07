using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using ICSI_Library.Membership;

namespace ICSI_WebApp.Util
{
    public static class DBTable
    {
        public static DataTable WEB_APPLICATION_T { get; set; }
        public static DataTable APPLICATION_T { get; set; }
        public static DataTable APP_MODULE_T { get; set; }
        public static DataTable SCREEN_T { get; set; }
        public static DataTable ROLE_T { get; set; }
        public static DataTable RESPONSIBILITY_T { get; set; }
        public static DataTable MENU_T { get; set; }
        public static DataTable REPORT_T { get; set; }
        //public static DataTable RESP_SCREEN_T { get; set; }
        public static DataTable ROLE_RESP_T { get; set; }
        public static DataTable SCREEN_COMP_T { get; set; }
        public static DataTable USER_TYPE_T { get; set; }
        //public static DataTable PAYMENT_GW_MASTER_T { get; set; }
        public static DataTable SCREEN_MESSAGE_T { get; set; }
        public static DataTable USER_APPROVAL_CONFIG_T { get; set; }

        public static void SetManagementData()
        {
            if (HttpContext.Current.Application["ManagementData"] != null)
            {
                JObject jdata = JObject.Parse(HttpContext.Current.Application["ManagementData"].ToString());
                foreach (JProperty property in jdata.Properties())
                {
                    if (property.Name == ICSIManagement.WEB_APPLICATION_T.ToString())
                        DBTable.WEB_APPLICATION_T = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    else if (property.Name == ICSIManagement.APPLICATION_T.ToString())
                        DBTable.APPLICATION_T = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    else if (property.Name == ICSIManagement.APP_MODULE_T.ToString())
                        DBTable.APP_MODULE_T = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    else if (property.Name == ICSIManagement.SCREEN_T.ToString())
                        DBTable.SCREEN_T = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    else if (property.Name == ICSIManagement.ROLE_T.ToString())
                        DBTable.ROLE_T = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    else if (property.Name == ICSIManagement.RESPONSIBILITY_T.ToString())
                        DBTable.RESPONSIBILITY_T = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    else if (property.Name == ICSIManagement.MENU_T.ToString())
                        DBTable.MENU_T = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    else if (property.Name == ICSIManagement.REPORT_T.ToString())
                        DBTable.REPORT_T = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    //else if (property.Name == ICSIManagement.RESP_SCREEN_T.ToString())
                        //DBTable.RESP_SCREEN_T = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    else if (property.Name == ICSIManagement.ROLE_RESP_T.ToString())
                        DBTable.ROLE_RESP_T = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    else if (property.Name == ICSIManagement.SCREEN_COMP_T.ToString())
                        DBTable.SCREEN_COMP_T = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    else if (property.Name == ICSIManagement.USER_TYPE_T.ToString())
                        DBTable.USER_TYPE_T = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    //else if (property.Name == ICSIManagement.PAYMENT_GW_MASTER_T.ToString())
                    //    DBTable.PAYMENT_GW_MASTER_T = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    else if (property.Name == ICSIManagement.SCREEN_MESSAGE_T.ToString())
                        DBTable.SCREEN_MESSAGE_T = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    else if (property.Name == ICSIManagement.USER_APPROVAL_CONFIG_T.ToString())
                        DBTable.USER_APPROVAL_CONFIG_T = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                }
            }
        }

        public static int GetUserData(int userId, UserData userData)
        {
            int status = 0;
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
            try
            {
                AppUrl = AppUrl + "/fetch";
                List<Dictionary<string, object>> data_data = new List<Dictionary<string, object>>();
                List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();

                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("ACTIVE_YN", 1);
                conditions.Add("USER_ID", userId);

                var tupleList = new List<Tuple<string, int, int>>
                {
                    new Tuple<string,int, int>("USER_RESP_T",0,100),
                    new Tuple<string,int, int>("USER_ROLE_T",0,100),
                    //new Tuple<string,int, int>("STUDENT_T",0,100),
                    new Tuple<string,int, int>("USER_SCREEN_COMP_T",0,100)
                };

                foreach (var item in tupleList)
                {
                    if (item.Item1 == "USER_TYPE_T")
                        conditions.Remove("USER_ID");
                    data_data.Add(Util.UtilService.addSubParameter("", item.Item1, item.Item2, item.Item3, data, conditions));
                }
                string Message = string.Empty;
                string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
                string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
                string sdata = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "fetch", data_data), out Message);
                JObject jdata = JObject.Parse(sdata);
                if (jdata.HasValues)
                    status = 1;
                foreach (JProperty property in jdata.Properties())
                {
                    if (property.Name == ICSIManagement.USER_RESP_T.ToString())
                        userData.USER_RESP_T = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    else if (property.Name == ICSIManagement.USER_ROLE_T.ToString())
                        userData.USER_ROLE_T = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    else if (property.Name == ICSIManagement.STUDENT_T.ToString())
                        userData.STUDENT_T = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    else if (property.Name == ICSIManagement.USER_SCREEN_COMP_T.ToString())
                        userData.USER_SCREEN_COMP_T = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                }

                conditions.Clear();
                conditions.Add("USER_ID", userId);
                conditions.Add("ACTIVE_YN", 1);
                DataTable dt = UtilService.getData("Training", "STUDENT_T", conditions, null, 0, 1);
                userData.STUDENT_T = dt;
            }
            catch (Exception ex) { status = 0; }
            return status;
        }

        public enum ICSIManagement
        {
            WEB_APPLICATION_T = 1,
            APPLICATION_T = 2,
            APP_MODULE_T = 3,
            SCREEN_T = 4,
            ROLE_T = 5,
            RESPONSIBILITY_T = 6,
            MENU_T = 7,
            REPORT_T = 8,
            //RESP_SCREEN_T = 9,
            ROLE_RESP_T = 9,
            SCREEN_COMP_T = 10,
            USER_RESP_T = 11,
            USER_ROLE_T = 12,
            USER_SCREEN_COMP_T = 13,
            USER_TYPE_T = 14,
            USER_T = 15,
            //PAYMENT_GW_MASTER_T=16,
            STUDENT_T=17,
            SCREEN_MESSAGE_T=18,
            USER_APPROVAL_CONFIG_T=19
        }

        public static JObject GetData(string MethodName, Dictionary<string, object> conditions, string TableName, int start, int end, string SchemaName)
        {
            return GetData(MethodName, conditions, null, TableName, start, end, SchemaName);
        }

        

        public static JObject GetData(string MethodName, Dictionary<string, object> conditions, Dictionary<string, string> conditionops, string TableName, int start, int end, string SchemaName)
        {
            DataTable dt = new DataTable();
            JObject jdata = null;
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
            try
            {
                AppUrl = AppUrl + "/fetch";
                List<Dictionary<string, object>> data_data = new List<Dictionary<string, object>>();
                List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();

                data_data.Add(Util.UtilService.addSubParameter(SchemaName, TableName, start, end, data, conditions, conditionops));

                string Message = string.Empty;
                string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
                string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
                string sdata = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", MethodName, data_data), out Message);
                jdata = JObject.Parse(sdata);
            }
            catch (Exception ex) { }
            return jdata;
        }

        public static Models.ActionClass GetSearchData(Dictionary<string, object> conditions, Dictionary<string, string> conditionops, string TableName, bool pgs, int pgn, int pgr, int start, int end, string SchemaName, string MethodType = "")
        {
            Models.ActionClass actionClass = new Models.ActionClass();
            string MethodName = "search";
            if (!string.IsNullOrEmpty(MethodType))
            {
                MethodName = MethodType;
            }
            //DataTable dt = new DataTable();
            //JObject jdata = null;
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
            try
            {
                AppUrl = AppUrl + "/fetch";
                List<Dictionary<string, object>> data_data = new List<Dictionary<string, object>>();
                List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();

                data_data.Add(Util.UtilService.addSubParameter(SchemaName, TableName, start, end, pgs, pgn, pgr, data, conditions, conditionops));

                string Message = string.Empty;
                string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
                string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
                actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", MethodName, data_data));
                //jdata = JObject.Parse(sdata);
            }
            catch (Exception ex) { }
            return actionClass;
        }
    }

    public class UserData
    {
        public DataTable USER_RESP_T { get; set; }
        public DataTable USER_ROLE_T { get; set; }
        public DataTable USER_SCREEN_COMP_T { get; set; }
        public DataTable STUDENT_T { get; set; }
        //public Dictionary<string, object> menu { get; set; }

    }
    public class ManagementData
    {
        string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
        public ManagementData()
        {
            if (HttpContext.Current.Session["LOGIN_ID"] != null && HttpContext.Current.Application["ManagementData"] == null)
                GetManagementData();
        }

        public void GetManagementData()
        {
            AppUrl = AppUrl + "/fetch";
            List<Dictionary<string, object>> data_data = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();

            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ACTIVE_YN", 1);

            var tupleList = new List<Tuple<string, int, int>>
                {
                    new Tuple<string,int, int>("WEB_APPLICATION_T",0,10),
                    new Tuple<string,int, int>("APPLICATION_T",0,10),
                    new Tuple<string,int, int>("APP_MODULE_T",0,100),
                    new Tuple<string,int, int>("SCREEN_T",0,2000),
                    new Tuple<string,int, int>("ROLE_T",0,100),
                    new Tuple<string,int, int>("RESPONSIBILITY_T",0,2000),
                    new Tuple<string,int, int>("MENU_T",0,600),
                    new Tuple<string,int, int>("REPORT_T",0,200),
                    //new Tuple<string,int, int>("RESP_SCREEN_T",0,2000), // Not requireed
                    new Tuple<string,int, int>("ROLE_RESP_T",0,3000),
                    new Tuple<string,int, int>("SCREEN_COMP_T",0,5000),
                    new Tuple<string,int, int>("USER_TYPE_T",0,100),
                    //new Tuple<string,int, int>("PAYMENT_GW_MASTER_T",0,100),
                    new Tuple<string,int, int>("SCREEN_MESSAGE_T",0,10000),
                    new Tuple<string,int, int>("USER_APPROVAL_CONFIG_T",0,1000),
                };

            foreach (var item in tupleList)
            {
                data_data.Add(Util.UtilService.addSubParameter("", item.Item1, item.Item2, item.Item3, data, conditions));
            }
            string Message = string.Empty;
            string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
            string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
            string sdata = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "fetch", data_data), out Message);
            JObject jdata = JObject.Parse(sdata);
            HttpContext.Current.Application["ManagementData"] = jdata;
            DBTable.SetManagementData();
        }
    }

    public class MembershipDetails
    {
        public ICSIDataMembers GetMembershipData(string membershipNumber)
        {
            Service service = new Service();
            ICSIDataMembers data = service.GetMemberDataDisciplinary(membershipNumber);
            string premem = string.Empty;
            int memno = 0;
            premem = membershipNumber.Substring(0, 1);
            memno = int.Parse(membershipNumber.Substring(1, membershipNumber.Length - 1));
            ICSI copdata = service.GetMemberShipDataForGST(premem, memno);
            return data;
        }
        public ICSI GetMembershipDataGST(string membershipNumber)
        {
            Service service = new Service();
            string premem = string.Empty;
            int memno = 0;
            premem = membershipNumber.Substring(0, 1);
            memno = int.Parse(membershipNumber.Substring(1, membershipNumber.Length - 1));
            ICSI copdata = service.GetMemberShipDataForGST(premem, memno);
            return copdata;
        }

        public CREIDTHOURS GetPCHDetails(string membershipNumber)
        {
            CREIDTHOURS PCHData = new CREIDTHOURS();
            try
            {
                Service service = new Service();
                string premem = string.Empty;
                int memno = 0;
                premem = membershipNumber.Substring(0, 1);
                memno = int.Parse(membershipNumber.Substring(1, membershipNumber.Length - 1));
                PCHData = service.GetBlockyear20202021_PCH(premem, memno.ToString());
                return PCHData;
            }
            catch (Exception ex) { }
            finally { }
            return PCHData;
        }

        public ICSIDataMembersRenewalStimulate GetMemberDetails(string membershipNumber, string DOB)
        {
            ICSIDataMembersRenewalStimulate MemberData = new ICSIDataMembersRenewalStimulate();
            try
            {
                Service service = new Service();
                MemberData = service.ICSIDataMembersRenewalStimulateQuick(membershipNumber, DOB);
                return MemberData;
            }
            catch (Exception ex) { }
            finally { }
            return MemberData;
        }
    }
}