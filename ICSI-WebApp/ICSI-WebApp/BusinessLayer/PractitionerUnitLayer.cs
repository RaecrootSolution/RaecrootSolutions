using ICSI_Library.Membership;
using ICSI_WebApp.Models;
using ICSI_WebApp.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using static ICSI_WebApp.Util.UtilService;
namespace ICSI_WebApp.BusinessLayer
{
    public class PractitionerUnitLayer
    {


        public ActionClass beforeAprroveEducationRequest(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            return UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterAprroveEducationRequest(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass actionClass = new ActionClass();
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
            string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
            string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
            AppUrl = AppUrl + "/AddUpdate";
            Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            Dictionary<string, object> dataNominations = new Dictionary<string, object>();
            List<Dictionary<string, object>> lstNominationsfData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> lstNominationsfData1 = new List<Dictionary<string, object>>();

            string requestId = frm["hidUI"].ToString();
            int currentRoleId = Convert.ToInt32(frm["ROLE_ID"].ToString());
            string membershipNumber = frm["MEMBERSHIP_NUMBER"].ToString();
            string lifeTimeMembershipNumber = frm["LIFE_MEMBERSHIP_NUMBER"].ToString();
            string amount = frm["AMOUNT_OF_REIMBURSEMENT_TX"].ToString();

            string status = frm["radio1"].ToString();
            string forwardToRole = frm["FORWARD_TO"].ToString();
            string internalRemarks = frm["INTERNAL_REMARKS"].ToString();
            string remarksForMember = frm["REMARKS_FOR_MEMBER"].ToString();

            string forwardToText = "";
            if (forwardToRole == "18")
                forwardToText = "DD/AD/JD";
            if (forwardToRole == "17")
                forwardToText = "HOD";

            if (Convert.ToInt32(status) == 2) //Call For
            {
                if (currentRoleId == 18 || currentRoleId == 17)
                    forwardToRole = "16";
                else if (currentRoleId == 16)
                    forwardToRole = "-1";// Return back to member
            }

            string forwardByName = string.Empty;
            if (currentRoleId == 16)
                forwardByName = "DO";
            else if (currentRoleId == 18)
                forwardByName = "JD";
            else if (currentRoleId == 18)
                forwardByName = "HOD";

            conditions.Add("ID", Convert.ToInt32(requestId));
            dataNominations.Add("ID", Convert.ToInt32(requestId));
            dataNominations.Add("STATUS_NM", Convert.ToInt32(status));
            dataNominations.Add("PENDING_WITH_NM", Convert.ToInt32(forwardToRole));
            dataNominations.Add("CHILDREN_EDUCATION_ALLOWANCE_NM", Convert.ToDecimal(frm["AMOUNT_OF_REIMBURSEMENT_TX"]));
            lstNominationsfData1.Add(dataNominations);
            lstNominationsfData.Add(Util.UtilService.addSubParameter("Training", "CSBF_EDU_ALLOWANCE_REQUEST_T", 0, 0, lstNominationsfData1, conditions));
            actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "update", lstNominationsfData));
            dataNominations.Clear();
            lstNominationsfData.Clear();
            lstNominationsfData1.Clear();
            conditions.Clear();

            dataNominations.Add("APPROVE_NM", Convert.ToInt32(status));
            dataNominations.Add("REF_ID", Convert.ToInt32(requestId));
            dataNominations.Add("REQUEST_TYPE_NM", 1);
            dataNominations.Add("FORWARD_BY", forwardByName);
            dataNominations.Add("FORWARDED_BY_ID", Convert.ToInt32(frm["u"].ToString()));
            dataNominations.Add("FORWARD_TO", forwardToText);
            dataNominations.Add("FORWARDED_TO_ID", Convert.ToInt32(forwardToRole));
            dataNominations.Add("FORWARD_DATE", DateTime.Now);
            dataNominations.Add("INTERNAL_REMARKS_TX", internalRemarks);
            dataNominations.Add("REMARKS_FOR_MEMBER_TX", remarksForMember);
            lstNominationsfData1.Add(dataNominations);
            lstNominationsfData.Add(Util.UtilService.addSubParameter("Training", "CSBF_FORWARDING_HISTORY_T", 0, 0, lstNominationsfData1, conditions));
            actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstNominationsfData));
            dataNominations.Clear();
            lstNominationsfData.Clear();
            lstNominationsfData1.Clear();

            frm["nextscreen"] = Convert.ToString(screen.Screen_Next_Id);
            return actionClass;
        }

        public ActionClass beforeAprroveMedicalReimbursement(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            return UtilService.beforeLoad(WEB_APP_ID, frm);
        }
        public ActionClass afterAprroveMedicalReimbursement(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass actionClass = new ActionClass();
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
            string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
            string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
            AppUrl = AppUrl + "/AddUpdate";
            Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            Dictionary<string, object> dataNominations = new Dictionary<string, object>();
            List<Dictionary<string, object>> lstNominationsfData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> lstNominationsfData1 = new List<Dictionary<string, object>>();

            string requestId = frm["hidUI"].ToString();
            int currentRoleId = Convert.ToInt32(frm["ROLE_ID"].ToString()); 
            string membershipNumber = frm["MEMBERSHIP_NUMBER"].ToString();
            string lifeTimeMembershipNumber = frm["LIFE_MEMBERSHIP_NUMBER"].ToString();
            string amount = frm["AMOUNT_OF_REIMBURSEMENT_TX"].ToString();

            string status = frm["radio1"].ToString();
            string forwardToRole = frm["FORWARD_TO"].ToString();
            string internalRemarks = frm["INTERNAL_REMARKS"].ToString();
            string remarksForMember = frm["REMARKS_FOR_MEMBER"].ToString();

            string forwardToText = "";
            if (forwardToRole == "18")
                forwardToText = "DD/AD/JD";
            if (forwardToRole == "17")
                forwardToText = "HOD";

            if (Convert.ToInt32(status) == 2) //Call For
            {
                if(currentRoleId == 18 || currentRoleId == 17)
                    forwardToRole = "16";
                else if (currentRoleId == 16)
                    forwardToRole = "-1";// Return back to member
            }

            string forwardByName = string.Empty;
            if (currentRoleId == 16)
                forwardByName = "DO";
            else if (currentRoleId == 18)
                forwardByName = "JD";
            else if(currentRoleId == 18)
                forwardByName = "HOD";

            conditions.Add("ID", Convert.ToInt32(requestId));            
            dataNominations.Add("ID", Convert.ToInt32(requestId));
            dataNominations.Add("STATUS_NM", Convert.ToInt32(status));
            dataNominations.Add("PENDING_WITH_NM", Convert.ToInt32(forwardToRole));
            dataNominations.Add("AMOUNT_NM", Convert.ToDecimal(frm["AMOUNT_OF_REIMBURSEMENT_TX"]));
            lstNominationsfData1.Add(dataNominations);
            lstNominationsfData.Add(Util.UtilService.addSubParameter("Training", "CSBF_MEDICAL_EXPENSE_REQUEST_T", 0, 0, lstNominationsfData1, conditions));
            actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "update", lstNominationsfData));
            dataNominations.Clear();
            lstNominationsfData.Clear();
            lstNominationsfData1.Clear();
            conditions.Clear();

            dataNominations.Add("APPROVE_NM", Convert.ToInt32(status));
            dataNominations.Add("REF_ID", Convert.ToInt32(requestId));
            dataNominations.Add("REQUEST_TYPE_NM", 2);
            dataNominations.Add("FORWARD_BY", forwardByName);
            dataNominations.Add("FORWARDED_BY_ID", Convert.ToInt32(frm["u"].ToString()));
            dataNominations.Add("FORWARD_TO", forwardToText);
            dataNominations.Add("FORWARDED_TO_ID", Convert.ToInt32(forwardToRole));
            dataNominations.Add("FORWARD_DATE", DateTime.Now);
            dataNominations.Add("INTERNAL_REMARKS_TX", internalRemarks);
            dataNominations.Add("REMARKS_FOR_MEMBER_TX", remarksForMember);
            lstNominationsfData1.Add(dataNominations);
            lstNominationsfData.Add(Util.UtilService.addSubParameter("Training", "CSBF_FORWARDING_HISTORY_T", 0, 0, lstNominationsfData1, conditions));
            actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstNominationsfData));
            dataNominations.Clear();
            lstNominationsfData.Clear();
            lstNominationsfData1.Clear();

            frm["nextscreen"] = Convert.ToString(screen.Screen_Next_Id);
            return actionClass;
        }

        public ActionClass beforeCSBFEducationAllowanceRequest(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            DataTable dt = new DataTable();
            string UserName = string.Empty;
            if (HttpContext.Current.Session["LOGIN_ID"] != null)
            {
                UserName = HttpContext.Current.Session["LOGIN_ID"].ToString();
            }
            return UtilService.beforeLoad(WEB_APP_ID, frm);
        }
        public ActionClass afterCSBFEducationAllowanceRequest(int WEB_APP_ID, FormCollection frm)
        {
            Dictionary<string, object> eduAllowanceEntity = new Dictionary<string, object>();
            Dictionary<string, object> bankEntity = new Dictionary<string, object>();
            Dictionary<string, object> child1Entity = new Dictionary<string, object>();
            Dictionary<string, object> child2Entity = new Dictionary<string, object>();
            List<Dictionary<string, object>> lstNominationsfData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> lstNominationsfData1 = new List<Dictionary<string, object>>();
            int eduAllowanceID = 0;

            Dictionary<string, object> conditions = new Dictionary<string, object>();
            ActionClass actionClass = new ActionClass();
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
            string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
            string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
            AppUrl = AppUrl + "/AddUpdate";
            Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
            int bankRefID = 0;

            bankEntity.Add("BANK_NAME_TX", frm["BANK_NAME_TX"].ToString());
            bankEntity.Add("ACCOUNT_NUMBER_TX ", frm["ACCOUNT_NUMBER_TX"].ToString());
            bankEntity.Add("ACCOUNT_HOLDER_NAME_TX ", frm["ACCOUNT_HOLDER_NAME_TX"].ToString());
            bankEntity.Add("IFSC_CODE_TX ", frm["IFSC_CODE_TX"].ToString());
            bankEntity.Add("APPLICANT_REMARKS_TX", frm["APPLICANT_REMARKS_TX"].ToString());

            if (bankEntity["BANK_NAME_TX"].ToString().Trim().Length > 0)
            {
                lstNominationsfData1.Add(bankEntity);
                lstNominationsfData.Add(Util.UtilService.addSubParameter("Training", "CSBF_BANK_DETAILS_T", 0, 0, lstNominationsfData1, conditions));
                actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstNominationsfData));
                if (Convert.ToInt32(actionClass.StatCode) >= 0)
                {
                    JObject userdata = JObject.Parse(Convert.ToString(actionClass.DecryptData));
                    DataTable dtb = new DataTable();
                    if (userdata.HasValues)
                    {
                        foreach (JProperty val in userdata.Properties())
                        {
                            if (val.Name == "CSBF_BANK_DETAILS_T")
                            {
                                dtb = JsonConvert.DeserializeObject<DataTable>(val.Value.ToString());
                                bankRefID = Convert.ToInt32(dtb.Rows[0]["ID"]);
                            }
                        }
                    }
                }
                lstNominationsfData.Clear();
                lstNominationsfData1.Clear();
            }
            if (frm["REG_ID"].ToString().Trim().Length > 0)
            {
                eduAllowanceEntity.Add("REF_ID", frm["REG_ID"].ToString());
                eduAllowanceEntity.Add("DOD_DT", frm["DATE_OF_ENTRY"].ToString());
                eduAllowanceEntity.Add("REF_NUMBER_TX", "0");
                eduAllowanceEntity.Add("BANK_REF_ID", bankRefID);
                eduAllowanceEntity.Add("PENDING_WITH_NM", 16);
                lstNominationsfData1.Add(eduAllowanceEntity);

                lstNominationsfData.Add(Util.UtilService.addSubParameter("Training", "CSBF_EDU_ALLOWANCE_REQUEST_T", 0, 0, lstNominationsfData1, conditions));
                actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstNominationsfData));
                if (Convert.ToInt32(actionClass.StatCode) >= 0)
                {
                    JObject userdata = JObject.Parse(Convert.ToString(actionClass.DecryptData));
                    DataTable dtb = new DataTable();
                    if (userdata.HasValues)
                    {
                        foreach (JProperty val in userdata.Properties())
                        {
                            if (val.Name == "CSBF_EDU_ALLOWANCE_REQUEST_T")
                            {
                                dtb = JsonConvert.DeserializeObject<DataTable>(val.Value.ToString());
                                eduAllowanceID = Convert.ToInt32(dtb.Rows[0]["ID"]);
                            }
                        }
                    }
                    //lstNominationsfData1[0]["REF_NUMBER_TX"] = "EA" + eduAllowanceID.ToString("00000###");
                    //lstNominationsfData.Clear();
                    //conditions.Add("ID", eduAllowanceID);
                    //lstNominationsfData.Add(Util.UtilService.addSubParameter("Training", "CSBF_EDU_ALLOWANCE_REQUEST_T", 0, 0, lstNominationsfData1, conditions));
                    //actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "update", lstNominationsfData));
                    ////actionClass = UtilService.insertOrUpdate("Training", "CSBF_EDU_ALLOWANCE_REQUEST_T", lstNominationsfData1);
                }


                lstNominationsfData.Clear();
                lstNominationsfData1.Clear();
            }
            child1Entity.Add("NAME_TX", frm["CHILD1_NAME_TX"].ToString());
            child1Entity.Add("REF_ID", eduAllowanceID);
            child1Entity.Add("AGE_TX", frm["CHILD1_AGE_TX"].ToString());
            child1Entity.Add("RELATION_TO_SUBSCRIBER_TX", frm["CHILD1_RELATION_TO_SUBSCRIBER_TX"].ToString());
            child1Entity.Add("PHONE_TX", frm["CHILD1_PHONE_TX"].ToString());
            child1Entity.Add("EMAIL_TX", frm["CHILD1_EMAIL_TX"].ToString());
            child1Entity.Add("ADDRESS_TX", frm["CHILD1_ADDRESS_TX"].ToString());

            child2Entity.Add("REF_ID", eduAllowanceID);
            child2Entity.Add("NAME_TX", frm["NAME_TX"].ToString());
            child2Entity.Add("AGE_TX", frm["AGE_TX"].ToString());
            child2Entity.Add("RELATION_TO_SUBSCRIBER_TX", frm["RELATION_TO_SUBSCRIBER_TX"].ToString());
            child2Entity.Add("PHONE_TX", frm["PHONE_TX"].ToString());
            child2Entity.Add("EMAIL_TX", frm["EMAIL_TX"].ToString());
            child2Entity.Add("ADDRESS_TX", frm["ADDRESS_TX"].ToString());

            lstNominationsfData1.Add(child1Entity);
            if (child2Entity["NAME_TX"].ToString().Trim().Length > 0)
                lstNominationsfData1.Add(child2Entity);
            lstNominationsfData.Add(Util.UtilService.addSubParameter("Training", "CSBF_CHILD_DETAILS_T", 0, 0, lstNominationsfData1, conditions));
            actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstNominationsfData));

            lstNominationsfData.Clear();
            lstNominationsfData1.Clear();

            //frm["nextscreen"] = Convert.ToString(screen.Screen_Next_Id);

            return actionClass;
        }

        public ActionClass beforeCSBFRegistration(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            DataTable dt = new DataTable();
            string UserName = string.Empty;
            if (HttpContext.Current.Session["LOGIN_ID"] != null)
            {
                UserName = HttpContext.Current.Session["LOGIN_ID"].ToString();
            }
            return UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        /// <summary>
        /// Created  by Soni Saroj
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <returns></returns>
        public ActionClass afterCSBFRegistration(int WEB_APP_ID, FormCollection frm)
        {
            Dictionary<string, object> dataCsbfRegistration = new Dictionary<string, object>();
            List<Dictionary<string, object>> lstCsbfData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> lstCsbfData1 = new List<Dictionary<string, object>>();

            List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> lstData1 = new List<Dictionary<string, object>>();
            Dictionary<string, object> data = new Dictionary<string, object>();
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            ActionClass actionClass = new ActionClass();
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
            string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
            string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
            AppUrl = AppUrl + "/AddUpdate";
            Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
            int ID = 0;

            dataCsbfRegistration.Add("MEMBERSHIP_NUMBER_TX", frm["FCS_MEMBERSHIP_NUMBER_TX"].ToString());
            dataCsbfRegistration.Add("DATE_OF_ENTRY_TX", frm["DATE_OF_ENTRY"].ToString());
            dataCsbfRegistration.Add("GOOD_HEALTH_YN", frm["APPROVE_NM"].ToString() == "1" ? true : false);
            dataCsbfRegistration.Add("REF_MEMBERSHIP_NUMBER_TX", frm["MEMBERSHIP_NUMBER_TX"].ToString());
            dataCsbfRegistration.Add("USER_ID_TX", Convert.ToInt32(frm["u"]));
            dataCsbfRegistration.Add("STATUS_TX", "Pending");
            dataCsbfRegistration.Add("SCHEME_ID_NM", frm["SCHEME_ID_NM"].ToString());
            lstCsbfData1.Add(dataCsbfRegistration);
            lstCsbfData.Add(Util.UtilService.addSubParameter("Training", "CSBF_REGISTARTION_T", 0, 0, lstCsbfData1, conditions));
            actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstCsbfData));
            if (Convert.ToInt32(actionClass.StatCode) >= 0)
            {
                JObject userdata = JObject.Parse(Convert.ToString(actionClass.DecryptData));
                DataTable dtb = new DataTable();
                if (userdata.HasValues)
                {
                    foreach (JProperty val in userdata.Properties())
                    {
                        if (val.Name == "CSBF_REGISTARTION_T")
                        {
                            dtb = JsonConvert.DeserializeObject<DataTable>(val.Value.ToString());
                            ID = Convert.ToInt32(dtb.Rows[0]["ID"]);
                        }
                    }
                }
            }
            dataCsbfRegistration.Clear();
            lstCsbfData1.Clear();
            lstCsbfData.Clear();

            Dictionary<string, object> dataNominations = new Dictionary<string, object>();
            List<Dictionary<string, object>> lstNominationsfData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> lstNominationsfData1 = new List<Dictionary<string, object>>();
            dataNominations.Add("REF_ID", ID);
            dataNominations.Add("NOMINEE_NAME_TX", frm["NOMINEE_NAME_TX"].ToString());
            dataNominations.Add("NOMINEE_AGE_TX", frm["NOMINEE_AGE_TX"].ToString());
            dataNominations.Add("NOMINEE_RELATION_TO_SUBSCRIBER_TX", frm["NOMINEE_RELATION_TO_SUBSCRIBER_TX"].ToString());
            dataNominations.Add("NOMINEE_EMAIL_TX", frm["NOMINEE_EMAIL_TX"].ToString());
            dataNominations.Add("NOMINEE_PHONE_TX", frm["NOMINEE_PHONE_TX"].ToString());
            dataNominations.Add("NOMINEE_ADDRESS_TX", frm["NOMINEE_ADDRESS_TX"].ToString());
            lstNominationsfData1.Add(dataNominations);
            lstNominationsfData.Add(Util.UtilService.addSubParameter("Training", "CSBF_NOMINEE_DETAILS_T", 0, 0, lstNominationsfData1, conditions));
            actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstNominationsfData));
            dataNominations.Clear();
            lstNominationsfData.Clear();
            lstNominationsfData1.Clear();


            Dictionary<string, object> dataRequestHistory = new Dictionary<string, object>();
            List<Dictionary<string, object>> lstRequestHistoryData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> lstRequestHistoryData1 = new List<Dictionary<string, object>>();
            dataRequestHistory.Add("REF_ID", ID);
            dataRequestHistory.Add("MEMBERSHIP_NUMBER_TX", frm["FCS_MEMBERSHIP_NUMBER_TX"].ToString());
            dataRequestHistory.Add("REQUEST_TYPE", "Medical Reimbursement");
            dataRequestHistory.Add("REQUEST_DATE", DateTime.Now);
            dataRequestHistory.Add("APPLICATION_STATUS", "Pending For Approval");
            lstRequestHistoryData1.Add(dataRequestHistory);
            lstRequestHistoryData.Add(Util.UtilService.addSubParameter("Training", "CSBF_REQUEST_HISTORY_T", 0, 0, lstRequestHistoryData1, conditions));
            actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstRequestHistoryData));
            dataRequestHistory.Clear();
            lstRequestHistoryData.Clear();
            lstRequestHistoryData1.Clear();



            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            Dictionary<string, object> d;
            if (!string.IsNullOrEmpty(Convert.ToString(frm["NAME_TX_1"])))
            {
                for (int i = 1; i <= Convert.ToInt32(frm["TOTAL_DEPEDENT"]); i++)
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(frm["NAME_TX_" + i + ""])))
                    {
                        string Name = Convert.ToString(frm["NAME_TX_" + i + ""]);
                        string Age = Convert.ToString(frm["AGE_TX_" + i + ""]);

                        string RELATION_TO_SUB = Convert.ToString(frm["RELATION_TO_SUB_TX_" + i + ""]);
                        string EMAIL_ID = Convert.ToString(frm["EMAIL_TX_" + i + ""]);
                        string PHONE_NUMBER = Convert.ToString(frm["PHONE_TX_" + i + ""]);
                        string ADDRESS = Convert.ToString(frm["ADDRESS_TX_" + i + ""]);

                        data.Add("REF_ID", ID);
                        data.Add("NAME_TX", Name);
                        data.Add("AGE_TX", Age);
                        data.Add("RELATION_TO_SUBSCRIBER_TX", RELATION_TO_SUB);
                        data.Add("EMAIL_TX", EMAIL_ID);
                        data.Add("PHONE_TX", PHONE_NUMBER);
                        data.Add("ADDRESS_TX", ADDRESS);

                        lstData1.Add(data);
                        lstData.Add(Util.UtilService.addSubParameter("Training", "CSBF_DEPENDANTS_RELATION_T ", 0, 0, lstData1, conditions));
                        actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));
                        data.Clear();
                        lstData.Clear();
                        lstData1.Clear();
                    }
                }
            }

            frm["nextscreen"] = Convert.ToString(screen.Screen_Next_Id);
            return actionClass;
        }

        public ActionClass beforePeerRegistration(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            return UtilService.beforeLoad(WEB_APP_ID, frm);
        }
        public ActionClass afterPeerRegistration(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass act = new ActionClass();
            DataTable dtReg = new DataTable();
            Screen_T screen = UtilService.GetScreenData(WEB_APP_ID, null, frm["screenid"], frm);
            string applicationSchema = Util.UtilService.getApplicationScheme(screen);
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
            string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
            string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
            AppUrl = AppUrl + "/AddUpdate";
            act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);

            Dictionary<string, object> conditions = new Dictionary<string, object>();
            int ID = 0;
            if (Convert.ToInt32(act.StatCode) >= 0)
            {
                JObject userdata = JObject.Parse(Convert.ToString(act.DecryptData));
                DataTable dtb = new DataTable();
                if (userdata.HasValues)
                {
                    foreach (JProperty val in userdata.Properties())
                    {
                        if (val.Name == screen.Table_Name_Tx)
                        {
                            dtb = JsonConvert.DeserializeObject<DataTable>(val.Value.ToString());
                            ID = Convert.ToInt32(dtb.Rows[0]["ID"]);
                        }

                    }
                }
            }

            return act;
        }

        private static string GetUqNumber(Screen_T screen, FormCollection frm)
        {
            string uqNumber = string.Empty;
            bool isPassed = false;
            if (frm["UNIQUE_REG_ID"] != null && !Convert.ToString(frm["UNIQUE_REG_ID"]).Trim().Equals(""))
            {
                isPassed = true;
                uqNumber = frm["UNIQUE_REG_ID"];
            }
            while (!isPassed)
            {
                string uq = UtilService.RandomNumber();
                if (isUqNumberExists(uqNumber, screen))
                {
                    isPassed = true;
                    uqNumber = uq;
                }
            }
            return uqNumber;
        }

        private static bool isUqNumberExists(string uqNumber, Screen_T screen)
        {
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("UNIQUE_REG_ID", uqNumber);
            DataTable dt = UtilService.getData(UtilService.getApplicationScheme(screen), screen.Table_Name_Tx, conditions, null, 0, 1);
            if (dt == null || dt.Rows == null || dt.Rows.Count == 0)
            {
                return true;
            }
            else return false;
        }




        public ActionClass beforePUQuestionnaire(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            DataTable dt = new DataTable();
            string UserName = string.Empty;
            if (HttpContext.Current.Session["LOGIN_ID"] != null)
            {
                UserName = HttpContext.Current.Session["LOGIN_ID"].ToString();
            }

            Dictionary<string, object> conds = new Dictionary<string, object>();
            conds.Add("ACTIVE_YN", 1);

            DataTable dtServices = Util.UtilService.getData("Membership", "PU_SERVICES", conds, null, 0, 100);

            if (dtServices != null && dtServices.Rows != null && dtServices.Rows.Count > 0)
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@ServiceCount", Convert.ToString(dtServices.Rows.Count));
            else
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@ServiceCount", string.Empty);
            return UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterPUQuestionnaire(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass actionClass = new ActionClass();
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
            string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
            string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
            AppUrl = AppUrl + "/AddUpdate";
            Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);

            try
            {
                frm["s"] = "update";
                frm["APPLICATION_STATUS_TX"] = "Allocated";


                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("USER_ID", HttpContext.Current.Session["USER_ID"]);
                DataTable dt = UtilService.getData("Membership", "PEER_REGISTRATION_T", conditions, null, 0, 1);
                string strMemNo = string.Empty;
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    if (dt.Rows[0]["MEMBER_DOB"] != DBNull.Value) frm["MEMBER_DOB_TX"] = Convert.ToString(dt.Rows[0]["MEMBER_DOB"]);
                    else frm["MEMBER_DOB_TX"] = string.Empty;

                    if (dt.Rows[0]["MEMBER_NO_TX"] != DBNull.Value)
                    {
                        strMemNo = Convert.ToString(dt.Rows[0]["MEMBER_NO_TX"]);
                        if (strMemNo.Substring(0, 1) == "F") frm["MEMBER_TYPE_TX"] = "FCS";
                        else frm["MEMBER_TYPE_TX"] = "ACS";

                        frm["MEMBER_NUM_TX"] = strMemNo.Substring(1, strMemNo.Length - 1);
                    }
                    else frm["MEMBER_NO_TX"] = string.Empty;
                }

                MembershipDetails ObjMemberShip = new MembershipDetails();
                if (!string.IsNullOrEmpty(strMemNo))
                {
                    ICSIDataMembers dataDtls = ObjMemberShip.GetMembershipData(strMemNo);
                    if (dataDtls != null)
                    {
                        frm["COP_NM"] = dataDtls.CP_NO;
                        if (!string.IsNullOrEmpty(dataDtls.CP_NO) && dataDtls.CP_NO != "0")
                            frm["COP_STATUS_TX"] = "Active";
                    }
                    else
                    {
                        frm["COP_NM"] = string.Empty;
                        frm["COP_STATUS_TX"] = string.Empty;
                    }
                }
                else
                {
                    frm["COP_NM"] = string.Empty;
                    frm["COP_STATUS_TX"] = string.Empty;
                }
                frm["BLOCK_YR_TX"] = string.Empty;
                actionClass = afterSubmit(WEB_APP_ID, frm);
                int ID = 0;
                if (Convert.ToInt32(actionClass.StatCode) >= 0)
                {
                    JObject userdata = JObject.Parse(Convert.ToString(actionClass.DecryptData));
                    DataTable dtb = new DataTable();
                    if (userdata.HasValues)
                    {
                        foreach (JProperty val in userdata.Properties())
                        {
                            if (val.Name == screen.Table_Name_Tx)
                            {
                                dtb = JsonConvert.DeserializeObject<DataTable>(val.Value.ToString());
                                ID = Convert.ToInt32(dtb.Rows[0]["ID"]);
                            }

                        }
                    }
                }
                if (ID > 0)
                {
                    HttpContext.Current.Session["PuId"] = ID;
                    List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
                    List<Dictionary<string, object>> lstData1 = new List<Dictionary<string, object>>();
                    conditions = new Dictionary<string, object>();
                    Dictionary<string, object> data = new Dictionary<string, object>();

                    string quest = string.Empty;
                    //Insert records in PU_Ques_Ans table
                    data.Clear();
                    data.Add("PU_ID", ID);
                    int serviceQuesCount = Convert.ToInt32(frm["hdnServiceQuestCount"]);
                    for (int j = 1; j <= 2; j++)
                    {
                        for (int i = 1; i <= serviceQuesCount; i++)
                        {
                            quest = "FY" + j + "_Q" + i + "_TX";
                            data.Add(quest, frm[quest]);
                        }
                    }

                    for (int i = 1; i <= 20; i++)
                    {
                        quest = "MAP_Q" + i + "_YN";
                        data.Add(quest, frm[quest]);
                    }
                    data.Add("MAP_Q21_TXT", frm["MAP_Q21_TXT"]);
                    lstData1.Add(data);
                    lstData.Add(Util.UtilService.addSubParameter("Membership", "PU_QUEST_ANS", 0, 0, lstData1, conditions));
                    actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));
                    data.Clear();
                    lstData.Clear();
                    lstData1.Clear();
                    int iQuesId = 0;
                    if (Convert.ToInt32(actionClass.StatCode) >= 0)
                    {
                        JObject userdata = JObject.Parse(Convert.ToString(actionClass.DecryptData));
                        DataTable dtb = new DataTable();
                        if (userdata.HasValues)
                        {
                            foreach (JProperty val in userdata.Properties())
                            {
                                if (val.Name == "PU_QUEST_ANS")
                                {
                                    dtb = JsonConvert.DeserializeObject<DataTable>(val.Value.ToString());
                                    iQuesId = Convert.ToInt32(dtb.Rows[0]["ID"]);
                                }

                            }
                        }
                    }
                    if (iQuesId > 0)
                    {
                        HttpContext.Current.Session["PU_QUEST_ID"] = iQuesId;
                    }
                    // Pcs firm Address
                    data.Clear();
                    data.Add("PROFESSIONAL_TX", frm["PROFESSIONAL_TX"]);
                    data.Add("PU_ID", ID);
                    data.Add("CITY_TX", frm["CITY_TX"]);
                    data.Add("STATE_TX", frm["STATE_TX"]);
                    data.Add("PIN_NM", Convert.ToInt32(frm["PIN_NM"]));
                    data.Add("IS_RESIDENCE_YN", 0);
                    lstData1.Add(data);
                    lstData.Add(Util.UtilService.addSubParameter("Membership", "PU_FIRM_ADDRESS", 0, 0, lstData1, conditions));
                    actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));

                    data.Clear();
                    lstData.Clear();
                    lstData1.Clear();

                    data.Add("PROFESSIONAL_TX", frm["RES_PROFESSIONAL_TX"]);
                    data.Add("PU_ID", ID);
                    data.Add("CITY_TX", frm["RES_CITY_TX"]);
                    data.Add("STATE_TX", frm["RES_STATE_TX"]);
                    if (!string.IsNullOrEmpty(frm["RES_PIN_NM"])) data.Add("PIN_NM", Convert.ToInt32(frm["RES_PIN_NM"]));
                    else data.Add("PIN_NM", string.Empty);
                    data.Add("IS_RESIDENCE_YN", 1);
                    HttpContext.Current.Session["PU_PROF_CITY"] = frm["CITY_TX"];
                    HttpContext.Current.Session["PU_RES_CITY"] = frm["RES_CITY_TX"];
                    lstData1.Add(data);
                    lstData.Add(Util.UtilService.addSubParameter("Membership", "PU_FIRM_ADDRESS", 0, 0, lstData1, conditions));
                    actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));
                    data.Clear();
                    lstData.Clear();
                    lstData1.Clear();

                    //Practice Units peer Reviewed

                    if (frm["hNAME_ADDRESS_TX"] != null && frm["hNAME_ADDRESS_TX"] != string.Empty)
                    {
                        string SL_NO = frm["hSL_NO"];
                        string NAME_ADDRESS_TX = frm["hNAME_ADDRESS_TX"].ToString();
                        string REVIWED_YEAR = frm["hREVIWED_YEAR"].ToString();
                        string UP_PARTNER_NAME = frm["hPARTNER_NAME_TX"].ToString();
                        var SL = (SL_NO ?? string.Empty).Split(',');
                        var NA = (NAME_ADDRESS_TX ?? string.Empty).Split(',');
                        var RY = (REVIWED_YEAR ?? string.Empty).Split(',');
                        var PN = (UP_PARTNER_NAME ?? string.Empty).Split(',');

                        for (int i = 0; i < SL.Length; i++)
                        {
                            if (SL[i].ToString() != string.Empty)
                            {


                                //data.Add("SL_NO", Convert.ToInt32(SL[i].ToString()));
                                data.Add("PU_ID", ID);
                                data.Add("NAME_ADDRESS_TX", NA[i].ToString());
                                data.Add("YEAR_PEER_REVIEWED", Convert.ToInt32(RY[i].ToString()));
                                data.Add("UP_PARTNER_NAME_TX", PN[i].ToString());
                                lstData1.Add(data);
                                lstData.Add(Util.UtilService.addSubParameter("Membership", "PU_PEER_REVIEWED ", 0, 0, lstData1, conditions));
                                actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));
                                data.Clear();
                                lstData.Clear();
                                lstData1.Clear();
                            }
                        }
                    }

                    //Partners Information TIRUMALA
                    if (frm["hPartnersNAME"] != null && frm["hPartnersNAME"] != string.Empty)
                    {
                        string SL_NO = frm["hSL_NO"];
                        string NAME = frm["hPartnersNAME"].ToString();
                        string MEMBER_NO = frm["hPartnerMem_No"].ToString();
                        string COP_NO = frm["hPartnerCop_No"].ToString();
                        var SL = (SL_NO ?? string.Empty).Split(',');
                        var name = (NAME ?? string.Empty).Split(',');
                        var mem_no = (MEMBER_NO ?? string.Empty).Split(',');
                        var cop_no = (COP_NO ?? string.Empty).Split(',');

                        for (int i = 0; i < SL.Length; i++)
                        {
                            if (SL[i].ToString() != string.Empty)
                            {
                                data.Add("PU_ID", ID);
                                data.Add("NAME", name[i].ToString());
                                data.Add("MEMBERSHIP_NO", mem_no[i].ToString());
                                data.Add("COP_NO", cop_no[i].ToString());

                                lstData1.Add(data);
                                lstData.Add(Util.UtilService.addSubParameter("Membership", "PU_PARTNERS_INFORMATION ", 0, 0, lstData1, conditions));
                                actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));
                                data.Clear();
                                lstData.Clear();
                                lstData1.Clear();
                            }
                        }
                    }

                    // Particulars of the constitution 
                    if (frm["PANAME_TX"] != null && frm["PANAME_TX"] != string.Empty)
                    {

                        string PANAME_TX = frm["PANAME_TX"];
                        string PMEMBERSHIP_NM = frm["PMEMBERSHIP_NM"].ToString();
                        string PRACTICE_YEAR_NM = frm["PRACTICE_YEAR_NM"].ToString();
                        string R_EXPERIENCE_NM = frm["R_EXPERIENCE_NM"].ToString();
                        var pname = (PANAME_TX ?? string.Empty).Split(',');
                        var membership = (PMEMBERSHIP_NM ?? string.Empty).Split(',');
                        var pyear = (PRACTICE_YEAR_NM ?? string.Empty).Split(',');
                        var rexperience = (R_EXPERIENCE_NM ?? string.Empty).Split(',');



                        for (int i = 0; i < pname.Length; i++)
                        {
                            if (pname[i].ToString() != string.Empty)
                            {
                                data.Add("PU_ID", ID);
                                data.Add("NAME", pname[i].ToString());
                                data.Add("MEMBERSHIP_NUMBER", membership[i].ToString());
                                data.Add("YEARS_OF_PRACTICE", pyear[i].ToString());
                                data.Add("REVIEW_EXPERIENCE", rexperience[i].ToString());
                                lstData1.Add(data);
                                lstData.Add(Util.UtilService.addSubParameter("Membership", "PU_PARTICULARS_CONSTITUTION", 0, 0, lstData1, conditions));
                                actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));

                                data.Clear();
                                lstData.Clear();
                                lstData1.Clear();
                            }
                        }
                    }

                    //  Particulars of Company Secretaries
                    if (frm["CNAME_TX"] != null && frm["CNAME_TX"] != string.Empty)
                    {
                        string CNAME_TX = frm["CNAME_TX"];
                        string MEMBERSHIP_NM = frm["MEMBERSHIP_NM"].ToString();
                        string A_YEAR_NM = frm["A_YEAR_NM"].ToString();
                        string S_EXPERIENCE_NM = frm["S_EXPERIENCE_NM"].ToString();

                        var cname = (CNAME_TX ?? string.Empty).Split(',');
                        var mnum = (MEMBERSHIP_NM ?? string.Empty).Split(',');
                        var Ayear = (A_YEAR_NM ?? string.Empty).Split(',');
                        var sexperience = (S_EXPERIENCE_NM ?? string.Empty).Split(',');



                        for (int i = 0; i < cname.Length; i++)
                        {
                            if (cname[i].ToString() != string.Empty)
                            {
                                data.Add("CNAME", cname[i].ToString());
                                data.Add("PU_ID", ID);
                                data.Add("C_MEMBERSHIP_NUMBER", mnum[i].ToString());
                                data.Add("ASSOCIATION_YEAR_TO_TASK", Ayear[i].ToString());
                                data.Add("SECRETARIE_EXPERIENCE", sexperience[i].ToString());

                                lstData1.Add(data);
                                lstData.Add(Util.UtilService.addSubParameter("Membership", "PU_COMPANY_SECRETARIES", 0, 0, lstData1, conditions));
                                actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));
                                data.Clear();
                                lstData.Clear();
                                lstData1.Clear();
                            }
                        }
                    }

                    //partners / company secretaries employed
                    if (frm["hPR_NAME"] != null && frm["hPR_NAME"] != string.Empty)
                    {

                        string prname = frm["hPR_NAME"];
                        string pmnum = frm["hPRMEMBERSHIP_NM"].ToString();
                        string pdoj = frm["hPDOJ"].ToString();
                        string pdol = frm["hPDOL"].ToString();

                        var pr_name = (prname ?? string.Empty).Split(',');
                        var pm = (pmnum ?? string.Empty).Split(',');
                        var doj = (pdoj ?? string.Empty).Split(',');
                        var dol = (pdol ?? string.Empty).Split(',');



                        for (int i = 0; i < pr_name.Length; i++)
                        {
                            if (pr_name[i].ToString() != string.Empty)
                            {
                                data.Add("PR_NAME", pr_name[i].ToString());
                                data.Add("PU_ID", ID);
                                data.Add("PR_MEMBERSHIP_NUMBER", pm[i].ToString());
                                data.Add("DOJ", doj[i].ToString());
                                data.Add("DOL", dol[i].ToString());

                                lstData1.Add(data);
                                lstData.Add(Util.UtilService.addSubParameter("Membership", "PU_CONSTITUTIONAL_CHANGES", 0, 0, lstData1, conditions));
                                actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));
                                data.Clear();
                                lstData.Clear();
                                lstData1.Clear();
                            }
                        }
                    }

                    //  PU have any branch offices
                    if (frm["hMEMBER_INCHARGE"] != null && frm["hMEMBER_INCHARGE"] != string.Empty)
                    {
                        string SR_NM = frm["hSR_NM"].ToString();
                        string MEMBER_INCHARGE = frm["hMEMBER_INCHARGE"].ToString();
                        string M_NM = frm["hM_NM"].ToString();
                        string LOCATION = frm["hLOCATION"].ToString();
                        string TURNOVER = frm["hTURNOVER"].ToString();
                        string ADDRESS = frm["hADDRESS"].ToString();

                        var sr_nm = (SR_NM ?? string.Empty).Split(',');
                        var mincharge = (MEMBER_INCHARGE ?? string.Empty).Split(',');
                        var location = (LOCATION ?? string.Empty).Split(',');
                        var m_nm = (M_NM ?? string.Empty).Split(',');
                        var turnover = (TURNOVER ?? string.Empty).Split(',');
                        var address = (ADDRESS ?? string.Empty).Split(',');

                        for (int i = 0; i < sr_nm.Length; i++)
                        {
                            if (sr_nm[i].ToString() != string.Empty)
                            {
                                data.Add("PU_ID", ID);
                                data.Add("MEMBER_INCHARGE_TX", mincharge[i].ToString());
                                data.Add("M_NM", Convert.ToString(m_nm[i]));
                                data.Add("LOCATION_TX", location[i].ToString());
                                data.Add("ADDRESS_TX", address[i].ToString());
                                data.Add("TURNOVER_NM", Convert.ToInt32(turnover[i]));
                                //      data.Add("PU_STATE_TX", bstate[i].ToString());

                                lstData1.Add(data);
                                lstData.Add(Util.UtilService.addSubParameter("Membership", "PU_BRANCH_OFFICE", 0, 0, lstData1, conditions));
                                actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));
                                data.Clear();
                                lstData.Clear();
                                lstData1.Clear();
                            }
                        }

                    }

                    //PU have any directors
                    if (frm["hdnDirectorPartnerName"] != null && frm["hdnDirectorCompanyName"] != string.Empty)
                    {
                        string strDirPartnerName = frm["hdnDirectorPartnerName"].ToString();
                        string strDirCompanyName = frm["hdnDirectorCompanyName"].ToString();
                        var PN = (strDirPartnerName ?? string.Empty).Split(',');
                        var CN = (strDirCompanyName ?? string.Empty).Split(',');

                        for (int i = 0; i < PN.Length; i++)
                        {
                            if (PN[i].ToString() != string.Empty)
                            {
                                data.Add("PU_ID", ID);
                                data.Add("DIR_PARTNER_NAME_TX", PN[i].ToString());
                                data.Add("COMPANY_NAME_TX", CN[i].ToString());

                                lstData1.Add(data);
                                lstData.Add(Util.UtilService.addSubParameter("Membership", "PU_DIRECTOR ", 0, 0, lstData1, conditions));
                                actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));
                                data.Clear();
                                lstData.Clear();
                                lstData1.Clear();
                            }
                        }
                    }

                    //PU have any peer review codes
                    if (frm["hdnPRPartnerName"] != null && frm["hdnPRCode"] != string.Empty)
                    {
                        string strPRPartnerName = frm["hdnPRPartnerName"].ToString();
                        string strPRCode = frm["hdnPRCode"].ToString();
                        var NA = (strPRPartnerName ?? string.Empty).Split(',');
                        var prCd = (strPRCode ?? string.Empty).Split(',');

                        for (int i = 0; i < NA.Length; i++)
                        {
                            if (NA[i].ToString() != string.Empty)
                            {
                                data.Add("PU_ID", ID);
                                data.Add("PR_PARTNER_NAME_TX", NA[i].ToString());
                                data.Add("PEER_REVIEWER_CODE", prCd[i].ToString());

                                lstData1.Add(data);
                                lstData.Add(Util.UtilService.addSubParameter("Membership", "PU_PEER_REVIEWED_CODE ", 0, 0, lstData1, conditions));
                                actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));
                                data.Clear();
                                lstData.Clear();
                                lstData1.Clear();
                            }
                        }
                    }
                }

                frm["nextscreen"] = Convert.ToString(screen.Screen_Next_Id);
                frm["DID"] = "1";
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
            return actionClass;
        }


        public ActionClass beforeQuestionnaire1(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {

            return UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterQuestionnaire1(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass actionClass = new ActionClass();
            Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
            if (!string.IsNullOrEmpty(Convert.ToString(HttpContext.Current.Session["PU_QUEST_ID"])) && HttpContext.Current.Session["PU_QUEST_ID"] != null)
            {
                frm["ID"] = Convert.ToString(HttpContext.Current.Session["PU_QUEST_ID"]);
                frm["PU_ID"] = Convert.ToString(HttpContext.Current.Session["PuId"]);
                frm["PU_PROF_CITY"] = Convert.ToString(HttpContext.Current.Session["PU_PROF_CITY"]);
                frm["PU_RES_CITY"] = Convert.ToString(HttpContext.Current.Session["PU_RES_CITY"]);
            }

            actionClass = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
            frm["nextscreen"] = Convert.ToString(screen.Screen_Next_Id);
            return actionClass;
        }




        public ActionClass beforeQuestionnaire2(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            return UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterQuestionnaire2(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass actionClass = new ActionClass();
            Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
            if (HttpContext.Current.Session["PU_QUEST_ID"] != null && HttpContext.Current.Session["PU_QUEST_ID"].ToString() != string.Empty)
            {
                frm["ID"] = HttpContext.Current.Session["PU_QUEST_ID"].ToString();
                frm["PU_ID"] = Convert.ToString(HttpContext.Current.Session["PuId"]);
                frm["PU_PROF_CITY"] = Convert.ToString(HttpContext.Current.Session["PU_PROF_CITY"]);
                frm["PU_RES_CITY"] = Convert.ToString(HttpContext.Current.Session["PU_RES_CITY"]);
            }

            frm["s"] = "update";
            frm["ui"] = frm["ID"].ToString();


            actionClass = Util.UtilService.afterSubmit(WEB_APP_ID, frm);

            frm["nextscreen"] = Convert.ToString(screen.Screen_Next_Id);

            return actionClass;
        }


        public ActionClass beforeQuestionnaire3(int WEB_APP_ID, FormCollection frm)
        {
            return UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterQuestionnaire3(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass actionClass = new ActionClass();
            Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
            if (HttpContext.Current.Session["PuId"] != null && HttpContext.Current.Session["PuId"].ToString() != string.Empty)
            {
                frm["ID"] = HttpContext.Current.Session["PuId"].ToString();
            }

            frm["s"] = "edit";
            frm["ui"] = frm["ID"].ToString();


            actionClass = Util.UtilService.afterSubmit(WEB_APP_ID, frm);

            frm["nextscreen"] = Convert.ToString(screen.Screen_Next_Id);

            return actionClass;
        }



        public ActionClass beforepeer_review_request(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {

            return UtilService.beforeLoad(WEB_APP_ID, frm);
        }



        public ActionClass afterpeer_review_request(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass actionClass = new ActionClass();
            Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
            if (HttpContext.Current.Session["PuId"] != null && HttpContext.Current.Session["PuId"].ToString() != string.Empty)
            {
                frm["ID"] = HttpContext.Current.Session["PuId"].ToString();
                frm["REVIEWER_ID"] = frm["rdoList"];
                //frm["s"] = "update";
                frm["ui"] = frm["ID"].ToString();
            }
            actionClass = Util.UtilService.afterSubmit(WEB_APP_ID, frm);

            frm["nextscreen"] = Convert.ToString(screen.Screen_Next_Id);

            return actionClass;
        }



        public static void SessionHandling(string TableName, string uniqueregId, string ID = null)
        {
            Dictionary<string, object> sessionObjs = new Dictionary<string, object>();
            uniqueregId = uniqueregId.PadLeft(10, '0');
            HttpContext.Current.Session["UNIQUE_REG_ID"] = uniqueregId.ToString();
            object[] sessionvalues = new object[2];
            if (HttpContext.Current.Session["SESSION_OBJECTS"] != null)
            {
                sessionObjs = (Dictionary<string, object>)HttpContext.Current.Session["SESSION_OBJECTS"];
                object sessionval;
                if (HttpContext.Current.Session["SESSION_OBJECTS"] != null && (sessionObjs.TryGetValue(TableName, out sessionval)))
                {
                    if (sessionObjs.TryGetValue(TableName, out sessionval))
                    {
                        sessionvalues = sessionval as object[];
                        sessionvalues[0] = uniqueregId.ToString();
                        sessionvalues[1] = (ID != null) ? ID.ToString() : (TableName == "PRACTICE_UNIT_PROFILE" && !(Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) == 1 || Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) == 2)) ? sessionvalues[1] : null;
                        sessionObjs.Remove(TableName);
                        sessionObjs.Add(TableName, sessionvalues);
                        HttpContext.Current.Session["SESSION_OBJECTS"] = sessionObjs;
                    }
                }
                else
                {
                    //sessionvalues[0] = uniqueregId.ToString();
                    //sessionvalues[1] = (ID != null) ? ID.ToString() : (TableName == "PCS_COMPANY_MASTER_DETAIL_T" && !(Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) == 1 || Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) == 2)) ? sessionvalues[1] : null;
                    //sessionObjs.Add(TableName, sessionvalues);
                    //HttpContext.Current.Session["SESSION_OBJECTS"] = sessionObjs;
                }
            }
            else
            {
                sessionvalues[0] = uniqueregId.ToString();
                sessionvalues[1] = (ID != null) ? ID.ToString() : (TableName == "PCS_COMPANY_MASTER_DETAIL_T" && !(Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) == 1 || Convert.ToInt32(HttpContext.Current.Session["USER_TYPE_ID"]) == 2)) ? sessionvalues[1] : null;
                sessionObjs.Add(TableName, sessionvalues);
                HttpContext.Current.Session["SESSION_OBJECTS"] = sessionObjs;
            }
        }

        public static string GetSessionData(string TableName, string ID = null)
        {
            Dictionary<string, object> sessionObjs = (Dictionary<string, object>)HttpContext.Current.Session["SESSION_OBJECTS"];
            string uniqueRegID = string.Empty;
            object sessionobjvalues;
            if (sessionObjs.TryGetValue(TableName, out sessionobjvalues))
            {
                object[] arr = sessionobjvalues as object[];
                uniqueRegID = (ID != null) ? ((arr[1] != null) ? arr[1].ToString() : null) : arr[0].ToString();
            }
            return uniqueRegID;
        }


        //Peer Reviewer View of Request

        public ActionClass beforeReviewerViewRequest(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {

            ActionClass act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
            screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
            string applicationSchema = Util.UtilService.getApplicationScheme(screen);

            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ACTIVE_YN", 1);
            DataTable dt = Util.UtilService.getData(applicationSchema, "PEER_REVIEWER", conditions, null, 0, 1);
            return UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterReviewerViewRequest(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            ActionClass actionClass = new ActionClass();
            screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
            if (HttpContext.Current.Session["PuId"] != null && HttpContext.Current.Session["PuId"].ToString() != string.Empty)
            {
                frm["ID"] = HttpContext.Current.Session["PuId"].ToString();
            }

            frm["s"] = "update";
            frm["ui"] = frm["ID"].ToString();


            actionClass = Util.UtilService.afterSubmit(WEB_APP_ID, frm);

            frm["nextscreen"] = Convert.ToString(screen.Screen_Next_Id);

            return actionClass;
        }



        public ActionClass beforeApprovalforPeerReview(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            ActionClass actionClass = new ActionClass();
            frm["s"] = "edit";
            actionClass = beforeLoad(WEB_APP_ID, frm);
            return actionClass;
        }


        public ActionClass afterApprovalforPeerReview(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass actionClass = new ActionClass();
            Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
            actionClass = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
            return actionClass;
        }

        public ActionClass beforeReviewApprovalAssignment(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass actionClass = new ActionClass();
            actionClass = beforeLoad(WEB_APP_ID, frm);
            return actionClass;
        }



        public ActionClass afterReviewApprovalAssignment(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass actionClass = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
            Screen_T screen = new Screen_T();
            frm["nextscreen"] = Convert.ToString(screen.Screen_Next_Id);

            return actionClass;
        }


        public ActionClass beforeQuestionnaireView(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            ActionClass actionClass = new ActionClass();
            string PU_ID = string.Empty;
            DataTable dt = new DataTable();
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            //if (HttpContext.Current.Session["PuId"] != null && HttpContext.Current.Session["PuId"].ToString() != string.Empty && frm["ui"]!=string.Empty)
            if (frm["ui"] != string.Empty)
            {
                PU_ID = frm["ui"].ToString();
                conditions.Add("QID", 56);
                conditions.Add("ID", Convert.ToInt32(PU_ID));

                JObject jdata = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, "Membership");

                DataTable dtt = new DataTable();
                foreach (JProperty property in jdata.Properties())
                {
                    if (property.Name == "qfetch")
                        dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                }

                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    foreach (DataColumn col in dt.Columns)
                    {
                        screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@" + col.ColumnName, Convert.ToString(dt.Rows[0][col.ColumnName]));
                    }
                }
            }

            conditions.Clear();
            conditions.Add("PU_ID", Convert.ToInt32(PU_ID));
            DataTable dtResults = Util.UtilService.getData("Membership", "PU_FIRM_ADDRESS", conditions, null, 0, 100);
            DataRow drAddRow = dtResults.Select("IS_RESIDENCE_YN = 0")[0];
            foreach (DataColumn dc in dtResults.Columns)
            {
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@" + dc.ColumnName, Convert.ToString(drAddRow[dc.ColumnName]));
            }

            drAddRow = dtResults.Select("IS_RESIDENCE_YN = 1")[0];
            foreach (DataColumn dc in dtResults.Columns)
            {
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@RES_" + dc.ColumnName, Convert.ToString(drAddRow[dc.ColumnName]));
            }


            actionClass = beforeLoad(WEB_APP_ID, frm);

            return actionClass;
        }



        public ActionClass afterQuestionnaireView(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass actionClass = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
            Screen_T screen = new Screen_T();
            frm["nextscreen"] = Convert.ToString(screen.Screen_Next_Id);

            return actionClass;
        }


        public ActionClass beforePUReport(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {

            ActionClass actionClass = new ActionClass();
            actionClass = beforeLoad(WEB_APP_ID, frm);
            return actionClass;
        }

        public ActionClass beforePUIssueCertificate(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            ActionClass action = new ActionClass();
            action = Util.UtilService.beforeLoad(WEB_APP_ID, frm);
            return action;
        }


        public ActionClass afterPUIssueCertificate(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass action = new ActionClass();
            frm["CERTI_STATUS_TX"] = "Submitted";
            action = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
            Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
            frm["nextscreen"] = Convert.ToString(screen.Screen_Next_Id);
            return action;
        }

        public ActionClass afterPUReport(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass actionClass = new ActionClass();
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
            string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
            string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
            AppUrl = AppUrl + "/AddUpdate";
            Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
            JObject jdata = DBTable.GetData("mfetch", null, "PU_REPORT_T", 0, 100, "Membership");
            frm["STATUS_TX"] = "Submitted";
            frm["CERTI_STATUS_TX"] = "In-Process";
            string strPeriodUnderRV = frm["PERIOD_UNDER_RV_TX"];
            string strTechStdsVal = frm["TECHNICAL_STDS_VAL"];
            frm["PERIOD_UNDER_RV_TX"] = strPeriodUnderRV.Replace(',', ';');
            frm["TECHNICAL_STDS_VAL"] = strTechStdsVal.Replace(',', ';');
            //TIRUMALA
            //          
            //              $("input[name=PERIOD_UNDER_RV_TX]").val($("select[name=PERIOD_UNDER_RV_TX]").val());
            //$("input[name=TECHNICAL_STDS_VAL]").val($("input[name=TECHNICAL_STDS_VAL]:checked").val());
            List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> lstData1 = new List<Dictionary<string, object>>();
            Dictionary<string, object> tableData = new Dictionary<string, object>();
            if (jdata.HasValues)
            {
                foreach (JProperty property in jdata.Properties())
                {
                    if (property.Name == "META_DATA")
                    {
                        DataTable dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            if (Convert.ToString(dt.Rows[i][0]) != "ID" && Convert.ToString(dt.Rows[i][0]) != "ACTIVE_YN"
                                && Convert.ToString(dt.Rows[i][0]) != "CREATED_DT" && Convert.ToString(dt.Rows[i][0]) != "CREATED_BY"
                                && Convert.ToString(dt.Rows[i][0]) != "UPDATED_DT" && Convert.ToString(dt.Rows[i][0]) != "UPDATED_BY"
                                && Convert.ToString(dt.Rows[i][0]) != "STATUS_YN")
                            {
                                tableData.Add(dt.Rows[i][0].ToString(), Util.UtilService.FillFormValue(dt.Rows[i][1].ToString(), frm, dt.Rows[i][0].ToString()));
                            }
                        }
                    }
                    lstData1.Add(tableData);
                    lstData.Add(Util.UtilService.addSubParameter("Membership", "PU_REPORT_T", 0, 0, lstData1, null));
                    actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));
                    actionClass.columnMetadata = jdata;
                    //TIRUMALA
                    if (jdata != null && jdata.HasValues)
                    {
                        string ref_string_id = Convert.ToString(((Newtonsoft.Json.Linq.JProperty)(JObject.Parse(actionClass.DecryptData).First.First.First.First)).Value);
                        int ref_id = 0;
                        int.TryParse(ref_string_id, out ref_id);
                        int UniqueId = 0;
                        UniqueId = Convert.ToInt32(frm["UniqueId"]);
                        if (UniqueId == 0)
                        {
                            UniqueId = ref_id;
                        }
                    }
                }
            }

            //actionClass = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
            return actionClass;
        }

        public ActionClass beforeReportEditSubmit(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            ActionClass actionClass = new ActionClass();
            actionClass = beforeLoad(WEB_APP_ID, frm);
            return actionClass;
        }

        public ActionClass afterReportEditSubmit(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass actionClass = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
            Screen_T screen = new Screen_T();
            frm["nextscreen"] = Convert.ToString(screen.Screen_Next_Id);

            return actionClass;
        }



        public static ActionClass Update(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            ActionClass actionClass = new ActionClass();
            string screenType = frm["s"];
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
            string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
            string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
            // screentype = insert / update / fetch
            try
            {
                if (screenType != null)
                {
                    if (screenType.Equals("insert") || screenType.Equals("update"))
                    {
                        //Screen_T screen = screenObject(WEB_APP_ID, frm);
                        Dictionary<string, object> conditions = new Dictionary<string, object>();
                        JObject jdata = DBTable.GetData("mfetch", null, screen.Table_Name_Tx, 0, 100, "Membership");

                        List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
                        List<Dictionary<string, object>> lstData1 = new List<Dictionary<string, object>>();
                        Dictionary<string, object> tableData = new Dictionary<string, object>();
                        if (jdata.HasValues)
                        {
                            foreach (JProperty property in jdata.Properties())
                            {
                                if (property.Name == "META_DATA")
                                {
                                    DataTable dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());

                                    #region single operation
                                    for (int i = 0; i < dt.Rows.Count; i++)
                                    {
                                        if (screenType.Equals("insert"))
                                        {
                                            if (Convert.ToString(dt.Rows[i][0]) != "ID" && Convert.ToString(dt.Rows[i][0]) != "ACTIVE_YN"
                                                && Convert.ToString(dt.Rows[i][0]) != "CREATED_DT" && Convert.ToString(dt.Rows[i][0]) != "CREATED_BY"
                                                && Convert.ToString(dt.Rows[i][0]) != "UPDATED_DT" && Convert.ToString(dt.Rows[i][0]) != "UPDATED_BY"
                                                && Convert.ToString(dt.Rows[i][0]) != "STATUS_YN")
                                            {
                                                tableData.Add(dt.Rows[i][0].ToString(), Util.UtilService.FillFormValue(dt.Rows[i][1].ToString(), frm, dt.Rows[i][0].ToString()));
                                            }
                                        }
                                        else if (screenType.Equals("update"))
                                        {
                                            if (Convert.ToString(dt.Rows[i][0]) != "CREATED_DT" && Convert.ToString(dt.Rows[i][0]) != "CREATED_BY"
                                               && Convert.ToString(dt.Rows[i][0]) != "UPDATED_DT" && Convert.ToString(dt.Rows[i][0]) != "UPDATED_BY")
                                            {
                                                if (frm[dt.Rows[i][0].ToString()] != null)
                                                {
                                                    tableData.Add(dt.Rows[i][0].ToString(), Util.UtilService.FillFormValue(dt.Rows[i][1].ToString(), frm, dt.Rows[i][0].ToString()));
                                                }
                                            }
                                        }
                                    }

                                    lstData1.Add(tableData);
                                    #endregion

                                    AppUrl = AppUrl + "/AddUpdate";
                                    lstData.Add(Util.UtilService.addSubParameter("Membership", screen.Table_Name_Tx, 0, 0, lstData1, conditions));
                                    actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", screenType.Equals("insert") ? "insert" : "update", lstData));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                actionClass.StatMessage = ex.Message;
                actionClass.StatCode = "-1";
            }
            return actionClass;
        }

        public ActionClass beforeCSBFFinancialAssistanceRequest(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            DataTable dt = new DataTable();
            string UserName = string.Empty;
            if (HttpContext.Current.Session["LOGIN_ID"] != null)
            {
                UserName = HttpContext.Current.Session["LOGIN_ID"].ToString();
            }
            return UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterCSBFFinancialAssistanceRequest(int WEB_APP_ID, FormCollection frm)
        {
            Dictionary<string, object> financialAssistanceEntity = new Dictionary<string, object>();
            Dictionary<string, object> bankEntity = new Dictionary<string, object>();
            List<Dictionary<string, object>> lstNominationsfData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> lstNominationsfData1 = new List<Dictionary<string, object>>();
            int financialAssistanceID = 0;

            Dictionary<string, object> conditions = new Dictionary<string, object>();
            ActionClass actionClass = new ActionClass();
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
            string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
            string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
            AppUrl = AppUrl + "/AddUpdate";
            Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
            int bankRefID = 0;

            bankEntity.Add("BANK_NAME_TX", frm["BANK_NAME_TX"].ToString());
            bankEntity.Add("ACCOUNT_NUMBER_TX ", frm["ACCOUNT_NUMBER_TX"].ToString());
            bankEntity.Add("ACCOUNT_HOLDER_NAME_TX ", frm["ACCOUNT_HOLDER_NAME_TX"].ToString());
            bankEntity.Add("IFSC_CODE_TX ", frm["IFSC_CODE_TX"].ToString());
            bankEntity.Add("APPLICANT_REMARKS_TX", frm["APPLICANT_REMARKS_TX"].ToString());

            if (bankEntity["BANK_NAME_TX"].ToString().Trim().Length > 0)
            {
                lstNominationsfData1.Add(bankEntity);
                lstNominationsfData.Add(Util.UtilService.addSubParameter("Training", "CSBF_BANK_DETAILS_T", 0, 0, lstNominationsfData1, conditions));
                actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstNominationsfData));
                if (Convert.ToInt32(actionClass.StatCode) >= 0)
                {
                    JObject userdata = JObject.Parse(Convert.ToString(actionClass.DecryptData));
                    DataTable dtb = new DataTable();
                    if (userdata.HasValues)
                    {
                        foreach (JProperty val in userdata.Properties())
                        {
                            if (val.Name == "CSBF_BANK_DETAILS_T")
                            {
                                dtb = JsonConvert.DeserializeObject<DataTable>(val.Value.ToString());
                                bankRefID = Convert.ToInt32(dtb.Rows[0]["ID"]);
                            }
                        }
                    }
                }
                lstNominationsfData.Clear();
                lstNominationsfData1.Clear();
            }
            if (frm["REG_ID"].ToString().Trim().Length > 0)
            {
                financialAssistanceEntity.Add("REF_ID", frm["REG_ID"].ToString());
                financialAssistanceEntity.Add("DOD_DT", frm["DATE_OF_ENTRY"].ToString());
                financialAssistanceEntity.Add("REF_NUMBER_TX", "0");
                financialAssistanceEntity.Add("BANK_REF_ID", bankRefID);
                financialAssistanceEntity.Add("PENDING_WITH_NM", 16);
                lstNominationsfData1.Add(financialAssistanceEntity);

                lstNominationsfData.Add(Util.UtilService.addSubParameter("Training", "CSBF_FINANCIAL_ASSISTANCE_REQUEST_T", 0, 0, lstNominationsfData1, conditions));
                actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstNominationsfData));
                if (Convert.ToInt32(actionClass.StatCode) >= 0)
                {
                    JObject userdata = JObject.Parse(Convert.ToString(actionClass.DecryptData));
                    DataTable dtb = new DataTable();
                    if (userdata.HasValues)
                    {
                        foreach (JProperty val in userdata.Properties())
                        {
                            if (val.Name == "CSBF_FINANCIAL_ASSISTANCE_REQUEST_T")
                            {
                                dtb = JsonConvert.DeserializeObject<DataTable>(val.Value.ToString());
                                financialAssistanceID = Convert.ToInt32(dtb.Rows[0]["ID"]);
                            }
                        }
                    }
                }
                lstNominationsfData.Clear();
                lstNominationsfData1.Clear();
            }

            actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstNominationsfData));

            lstNominationsfData.Clear();
            lstNominationsfData1.Clear();

            //frm["nextscreen"] = Convert.ToString(screen.Screen_Next_Id);

            return actionClass;
        }

        public ActionClass beforeCSBFCSBFMedicalExpenseRequest(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            DataTable dt = new DataTable();
            string UserName = string.Empty;
            if (HttpContext.Current.Session["LOGIN_ID"] != null)
            {
                UserName = HttpContext.Current.Session["LOGIN_ID"].ToString();
            }
            return UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterCSBFMedicalExpenseRequest(int WEB_APP_ID, FormCollection frm)
        {
            Dictionary<string, object> medicalExpenseEntity = new Dictionary<string, object>();
            Dictionary<string, object> bankEntity = new Dictionary<string, object>();
            List<Dictionary<string, object>> lstNominationsfData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> lstNominationsfData1 = new List<Dictionary<string, object>>();
            int financialAssistanceID = 0;

            Dictionary<string, object> conditions = new Dictionary<string, object>();
            ActionClass actionClass = new ActionClass();
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
            string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
            string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
            AppUrl = AppUrl + "/AddUpdate";
            Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
            int bankRefID = 0;

            bankEntity.Add("BANK_NAME_TX", frm["BANK_NAME_TX"].ToString());
            bankEntity.Add("ACCOUNT_NUMBER_TX ", frm["ACCOUNT_NUMBER_TX"].ToString());
            bankEntity.Add("ACCOUNT_HOLDER_NAME_TX ", frm["ACCOUNT_HOLDER_NAME_TX"].ToString());
            bankEntity.Add("IFSC_CODE_TX ", frm["IFSC_CODE_TX"].ToString());
            bankEntity.Add("APPLICANT_REMARKS_TX", frm["APPLICANT_REMARKS_TX"].ToString());

            if (bankEntity["BANK_NAME_TX"].ToString().Trim().Length > 0)
            {
                lstNominationsfData1.Add(bankEntity);
                lstNominationsfData.Add(Util.UtilService.addSubParameter("Training", "CSBF_BANK_DETAILS_T", 0, 0, lstNominationsfData1, conditions));
                actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstNominationsfData));
                if (Convert.ToInt32(actionClass.StatCode) >= 0)
                {
                    JObject userdata = JObject.Parse(Convert.ToString(actionClass.DecryptData));
                    DataTable dtb = new DataTable();
                    if (userdata.HasValues)
                    {
                        foreach (JProperty val in userdata.Properties())
                        {
                            if (val.Name == "CSBF_BANK_DETAILS_T")
                            {
                                dtb = JsonConvert.DeserializeObject<DataTable>(val.Value.ToString());
                                bankRefID = Convert.ToInt32(dtb.Rows[0]["ID"]);
                            }
                        }
                    }
                }
                lstNominationsfData.Clear();
                lstNominationsfData1.Clear();
            }
            if (frm["REG_ID"].ToString().Trim().Length > 0)
            {
                medicalExpenseEntity.Add("REF_ID", frm["REG_ID"].ToString());
                medicalExpenseEntity.Add("MEDICAL_REIMBURSEMENT_FOR_TX", frm["MEDICAL_REIMBURSEMENT_FOR_TX"].ToString());
                medicalExpenseEntity.Add("REF_NUMBER_TX", "0");
                medicalExpenseEntity.Add("BANK_REF_ID", bankRefID);
                medicalExpenseEntity.Add("PENDING_WITH_NM", 16);
                lstNominationsfData1.Add(medicalExpenseEntity);

                lstNominationsfData.Add(Util.UtilService.addSubParameter("Training", "CSBF_MEDICAL_EXPENSE_REQUEST_T", 0, 0, lstNominationsfData1, conditions));
                actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstNominationsfData));
                if (Convert.ToInt32(actionClass.StatCode) >= 0)
                {
                    JObject userdata = JObject.Parse(Convert.ToString(actionClass.DecryptData));
                    DataTable dtb = new DataTable();
                    if (userdata.HasValues)
                    {
                        foreach (JProperty val in userdata.Properties())
                        {
                            if (val.Name == "CSBF_MEDICAL_EXPENSE_REQUEST_T")
                            {
                                dtb = JsonConvert.DeserializeObject<DataTable>(val.Value.ToString());
                                financialAssistanceID = Convert.ToInt32(dtb.Rows[0]["ID"]);
                            }
                        }
                    }
                }
                lstNominationsfData.Clear();
                lstNominationsfData1.Clear();
            }

            actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstNominationsfData));

            lstNominationsfData.Clear();
            lstNominationsfData1.Clear();

            //frm["nextscreen"] = Convert.ToString(screen.Screen_Next_Id);

            return actionClass;
        }
    }
}
