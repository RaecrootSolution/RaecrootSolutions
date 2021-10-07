using ICSI_Library.Membership;
using ICSI_WebApp.BusinessLayer;
using ICSI_WebApp.Models;
using ICSI_WebApp.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ICSI_WebApp.Controllers
{
    public class PCSController : Controller
    {
        private int WEB_APP_ID = Convert.ToInt32(Convert.ToString(ConfigurationManager.AppSettings["WEB_APP_ID"]));
        private string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
        private ManagementData objApplicationObject;

        public ActionResult Pcs(FormCollection frm)
        {
            if (Session["LOGIN_ID"] == null)
            {
                login();
            }
                
            string userid = frm["u"];
            string menuid = frm["m"];
            string ScreenType = frm["s"];
            string screenId = frm["si"];
            if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null)
            {
                if (frm["scrtype"] == "update")
                {
                    ScreenType = "update";
                    frm["s"] = ScreenType;
                }

                ActionClass act = new ActionClass();
                if (userid != null && !userid.Trim().Equals("") && ((menuid != null && !menuid.Trim().Equals("")) || (screenId != null && !screenId.Trim().Equals(""))))
                {
                    Screen_T screen = UtilService.homeAction(WEB_APP_ID, frm, userid, menuid, screenId, ScreenType, act, (List<object>)Session["USER_MENU"]);
                    if (screen != null && screen.Screen_Content_Tx != null) screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@API_REGION_ID", Convert.ToString(Session["REGION_ID"]));
                    if (screen != null)
                    {
                        ViewBag.Title = screen.Screen_Title_Tx;
                        ViewBag.MenuId = menuid;
                        ViewBag.ActionClass = act;
                        return View(screen);
                    }
                    else return View();
                }
                else return RedirectToAction("Pcs");
            }
            else
                return RedirectToAction("logout");
        }

        [HttpGet]
        public ActionResult logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("login");
        }

        private void login()
        {
            string UserName = "pcscomp";
            string Password = "5f4dcc3b5aa765d61d8327deb882cf99";
            int UserId = 0;

            #region Consume Service
            List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
            AppUrl = AppUrl + "/login";
            string Message = string.Empty;
            string sdata = UtilService.createRequestObject(AppUrl, UserName, Password, UtilService.createParameters("", "", "", "", "", "login", data), out Message);

            try
            {
                if (sdata != null)
                {
                    JObject jdata = JObject.Parse(sdata);
                    foreach (var item in jdata["data"].First.Children())
                    {
                        if (((Newtonsoft.Json.Linq.JProperty)item).Name == "LOGIN_ID")
                            Session["LOGIN_ID"] = ((Newtonsoft.Json.Linq.JProperty)item).Value;
                        if (((Newtonsoft.Json.Linq.JProperty)item).Name == "ID")
                        {
                            UserId = Convert.ToInt32(((Newtonsoft.Json.Linq.JProperty)item).Value);
                            Session["USER_ID"] = ((Newtonsoft.Json.Linq.JProperty)item).Value;
                        }
                        else if (((Newtonsoft.Json.Linq.JProperty)item).Name == "USER_TYPE_ID")
                            Session["USER_TYPE_ID"] = ((Newtonsoft.Json.Linq.JProperty)item).Value;
                        else if (((Newtonsoft.Json.Linq.JProperty)item).Name == "USER_NAME_TX")
                            Session["USER_NAME_TX"] = ((Newtonsoft.Json.Linq.JProperty)item).Value;
                        else if (((Newtonsoft.Json.Linq.JProperty)item).Name == "SESSION_KEY")
                            Session["SESSION_KEY"] = ((Newtonsoft.Json.Linq.JProperty)item).Value;
                        else if (((Newtonsoft.Json.Linq.JProperty)item).Name == "USER_ID")
                            Session["USER_ID_TX"] = ((Newtonsoft.Json.Linq.JProperty)item).Value;
                        else if (((Newtonsoft.Json.Linq.JProperty)item).Name == "REGION_ID")
                            Session["REGION_ID"] = ((Newtonsoft.Json.Linq.JProperty)item).Value;
                        else if (((Newtonsoft.Json.Linq.JProperty)item).Name == "OFFICE_ID")
                            Session["OFFICE_ID"] = ((Newtonsoft.Json.Linq.JProperty)item).Value;
                    }
                }
                ViewBag.Message = Message.Replace("_", " ");
                #endregion

                if (Message == "Success")
                {
                    #region Get User Data
                    UserData userData = new UserData();
                    int status = DBTable.GetUserData(UserId, userData);
                    if (status == 1)
                    {
                        if (HttpContext.Application["ManagementData"] == null)
                            objApplicationObject = new ManagementData();
                        DataTable resps = UtilService.fetchResponsibilities(userData);
                        List<object> menu = UtilService.getMenu(WEB_APP_ID, resps);
                        Session["USER_MENU"] = menu;
                        Session["USER_DATA"] = userData;
                    }
                    #endregion
                }
            }
            catch (Exception exx) { }
        }

        public JsonResult UpdateHistoryData(string remarks, string email, string mobileNm = null, string status = null, string registration = null, string userTypeID = null, string isPCSExtension = null, string uniqueRegID = null)
        {
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            DataTable docStatusdt = new DataTable();
            StringBuilder docStatus = new StringBuilder();
            docStatus.Append(remarks);
            string documentsStatus = string.Empty;
            Dictionary<string, object> docConditions = new Dictionary<string, object>();
            Dictionary<string, object> data = new Dictionary<string, object>();
            string UserName = HttpContext.Session["LOGIN_ID"].ToString();
            string Session_Key = HttpContext.Session["SESSION_KEY"].ToString();
            ActionClass actionClass = new ActionClass();
            List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> lstData1 = new List<Dictionary<string, object>>();

            if (uniqueRegID != null)
            {

                object sessionobjvalues;
                Dictionary<string, object> sessionObjs = (Dictionary<string, object>)HttpContext.Session["SESSION_OBJECTS"];
                if (sessionObjs.TryGetValue(uniqueRegID, out sessionobjvalues))
                {
                    string[] DocIDs = sessionobjvalues.ToString().Split(',');


                    foreach (string docid in DocIDs)
                    {
                        documentsStatus = string.Empty;
                        docConditions.Clear();
                        docConditions.Add("ACTIVE_YN", 1);
                        docConditions.Add("UNIQUE_REG_ID", uniqueRegID);
                        docConditions.Add("ID", docid.ToString());
                        docStatusdt = UtilService.getData("Training", "PCS_REGISTRATION_DOCUMENT_T", docConditions, null, 0, 1);
                        if (docStatusdt != null && docStatusdt.Rows.Count > 0)
                        {
                            documentsStatus = " Your document " + docStatusdt.Rows[0]["FILE_NAME_TX"].ToString() + " status changed to " + docStatusdt.Rows[0]["STATUS_TX"].ToString();
                            docStatus.AppendLine(documentsStatus);
                        }
                    }

                }
            }
            if (registration == null)
            {


                data.Add("EMAIL_DESC_TX", docStatus);
                data.Add("EMAIL_TX", email);
                data.Add("EMAIL_SENT_DT", DateTime.Now);
                data.Add("UNIQUE_REG_ID", uniqueRegID);
                if (isPCSExtension != null)
                {
                    data.Add("IS_PCS_EXTENSION_YN", 1);
                }
                //bool value= PCSLayer.sendMail(email, "Test Subject", remarks);
                //if (value)
                //{
                lstData1.Add(data);
                lstData.Add(Util.UtilService.addSubParameter("Training", "PCS_EMAIL_HISTORY_T", 0, 0, lstData1, conditions));
                AppUrl = AppUrl + "/AddUpdate";
                actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));

                //}

                if (status != null && status != "" && status == "forward")
                {
                    //bool value= PCSLayer.sendMail(email, "Test Subject", remarks);
                    data.Clear();
                    lstData.Clear();
                    lstData1.Clear();
                    data.Add("EMAIL_DESC_TX", remarks.ToString());
                    data.Add("EMAIL_TX", email);
                    data.Add("EMAIL_SENT_DT", DateTime.Now);
                    data.Add("UNIQUE_REG_ID", uniqueRegID);
                    lstData.Add(Util.UtilService.addSubParameter("Training", "PCS_FORWARDING_HISTORY_T", 0, 0, lstData1, conditions));
                    actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));
                }

                data.Clear();
                lstData.Clear();
                lstData1.Clear();
                data.Add("SMS_DESC_TX", remarks.ToString());
                data.Add("SMS_SENT_TO_NM", mobileNm);
                data.Add("UNIQUE_REG_ID", uniqueRegID);
                data.Add("SMS_SENT_DT", DateTime.Now);
                if (isPCSExtension != null)
                {
                    data.Add("IS_PCS_EXTENSION_YN", 1);
                }
                //bool value= PCSLayer.sendSMS("", "Test Subject", remarks);
                //if (value)
                //{
                lstData1.Add(data);
                lstData.Add(Util.UtilService.addSubParameter("Training", "PCS_SMS_HISTORY_T", 0, 0, lstData1, conditions));
                actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));

                //}
            }
            if (registration != null && Session["LOGIN_ID"] == null)
            {

                data.Add("USER_TYPE_ID", Int64.Parse(userTypeID));
                data.Add("USER_ID", uniqueRegID);
                data.Add("LOGIN_ID", uniqueRegID);
                data.Add("CREATED_BY", 1);
                data.Add("ACTIVE_YN", 1);
                data.Add("LOGIN_PWD_TX", "5f4dcc3b5aa765d61d8327deb882cf99");
                data.Add("CREATED_DT", DateTime.Now);
                //bool value= PCSLayer.sendMail(email, "Test Subject", remarks);
                //if (value)
                //{
                lstData1.Add(data);
                lstData.Add(Util.UtilService.addSubParameter("Training", "USER_T", 0, 0, lstData1, conditions));
                AppUrl = AppUrl + "/AddUpdate";
                actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));
                int? ID = null;
                if (Convert.ToInt32(actionClass.StatCode) >= 0)
                {
                    JObject userdata = JObject.Parse(Convert.ToString(actionClass.DecryptData));
                    DataTable dtb = new DataTable();
                    if (userdata.HasValues)
                    {
                        foreach (JProperty val in userdata.Properties())
                        {
                            if (val.Name == "USER_T")
                            {
                                dtb = JsonConvert.DeserializeObject<DataTable>(val.Value.ToString());
                                ID = Convert.ToInt32(dtb.Rows[0]["ID"]);
                            }
                        }
                    }
                }
                if (ID != null)
                {
                    lstData.Clear();
                    lstData1.Clear();
                    data.Clear();
                    data.Add("CREATED_BY", 1);
                    data.Add("ACTIVE_YN", 1);
                    data.Add("ROLE_ID", userTypeID);
                    data.Add("CREATED_DT", DateTime.Now);
                    data.Add("USER_ID", ID);
                    lstData1.Add(data);
                    lstData.Add(Util.UtilService.addSubParameter("Training", "USER_ROLE_T", 0, 0, lstData1, conditions));
                    actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));
                }
            }

            return this.Json("Success");
        }
        public JsonResult Validate(string unqid)
        {
            string result = "Failure";
            if (HttpContext.Session["LOGIN_ID"] != null && HttpContext.Session["LOGIN_ID"].ToString() != "pcscomp")
            {
                return Json(result);
            }
            DataTable dt = new DataTable();
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ACTIVE_YN", 1);
            conditions.Add("UNIQUE_REG_ID", unqid);
            dt = UtilService.getData("Training", "PCS_COMPANY_MASTER_DETAIL_T", conditions, null, 0, 1);
            if (dt.Rows.Count > 0)
            {
                if (dt.Rows[0]["STATUS_TX"].ToString() == "pending" || dt.Rows[0]["STATUS_TX"].ToString() == "approved")
                {
                    HttpContext.Session["SESSION_OBJECTS"] = null;
                    return Json(result);
                }
                else
                {
                    result = dt.Rows[0]["ID"].ToString();
                    return Json(result);
                }

            }
            return Json(result);

        }
        public JsonResult GetCurrentID(string uniqueRegID = null)
        {
            string ID = "Failure";
            if (uniqueRegID != null)
            {
                DataTable dt = new DataTable();
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("ACTIVE_YN", 1);
                conditions.Add("UNIQUE_REG_ID", uniqueRegID);
                dt = UtilService.getData("Training", "PCS_COMPANY_MASTER_DETAIL_T", conditions, null, 0, 1);
                if (dt != null && dt.Rows.Count > 0)
                {
                    ID = dt.Rows[0]["ID"].ToString();
                }
            }
            return Json(ID);
        }
        public JsonResult GetPCDetails(string membershipNM = null, string cop = null)
        {
            if (membershipNM == null && HttpContext.Session["SESSION_OBJECTS"] != null)
            {
                Dictionary<string, object> sessionObjs = (Dictionary<string, object>)HttpContext.Session["SESSION_OBJECTS"];
                object sessionobjvalues;
                if (sessionObjs.TryGetValue("MEMBERSHIP_NUMBER", out sessionobjvalues))
                {
                    membershipNM = sessionobjvalues.ToString();
                    HttpContext.Session["SESSION_OBJECTS"] = sessionObjs;
                }
                else if (sessionObjs.TryGetValue("PCS_COMPANY_MASTER_DETAIL_T", out sessionobjvalues))
                {
                    object[] arr = sessionobjvalues as object[];
                    DataTable dt = new DataTable();
                    string uniqueRegID = arr[0].ToString();
                    Dictionary<string, object> conditions = new Dictionary<string, object>();
                    conditions.Add("UNIQUE_REG_ID", uniqueRegID);
                    conditions.Add("ACTIVE_YN", 1);
                    dt = UtilService.getData("Training", "PCS_COMPANY_MASTER_DETAIL_T", conditions, null, 0, 1);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        membershipNM = dt.Rows[0]["MEMBERSHIP_NM"].ToString();
                    }
                }
            }
            var resp = string.Empty;
            MembershipDetails membershipData = new MembershipDetails();
            ICSIDataMembers data = membershipData.GetMembershipData(membershipNM);
            ICSI datacop = membershipData.GetMembershipDataGST(membershipNM);
            if (data.MembershipNo != null)
            {
                if (HttpContext.Session["SESSION_OBJECTS"] != null)
                {
                    Dictionary<string, object> sessionObjs = (Dictionary<string, object>)HttpContext.Session["SESSION_OBJECTS"];
                    sessionObjs.Remove("MEMBERSHIP_NUMBER");
                    sessionObjs.Add("MEMBERSHIP_NUMBER", membershipNM);
                    HttpContext.Session["SESSION_OBJECTS"] = sessionObjs;
                    resp = (cop != null) ? Newtonsoft.Json.JsonConvert.SerializeObject(datacop) : Newtonsoft.Json.JsonConvert.SerializeObject(data);
                }

                else
                {
                    Dictionary<string, object> sessionObjs = new Dictionary<string, object>();
                    sessionObjs.Add("MEMBERSHIP_NUMBER", membershipNM);
                    HttpContext.Session["SESSION_OBJECTS"] = sessionObjs;
                    resp = (cop != null) ? Newtonsoft.Json.JsonConvert.SerializeObject(datacop) : Newtonsoft.Json.JsonConvert.SerializeObject(data);
                }
            }
            else
            {
                resp = "Failure";
            }

            return Json(resp);
        }
        public ActionResult DownloadFilePCS(string id)
        {
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ACTIVE_YN", 1);
            conditions.Add("ID", id.Split('_')[1]);

            string TableName = string.Empty;
            string FilePath = string.Empty;
            string fileName = string.Empty;
            TableName = Convert.ToString(Util.UtilService.TableName.PCS_REGISTRATION_DOCUMENT_T);
            FilePath = Convert.ToString(Util.UtilService.FilePath.PCSCOMPDOCS.GetEnumDisplayName());
            DataTable dtData = new DataTable();

            dtData = UtilService.getData("Training", TableName, conditions, null, 0, 1);


            //foreach (JProperty property in jdata.Properties())
            //{
            //    if (property.Name == TableName)
            //    {
            //        dtData = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
            //    }
            //}

            string filepath = string.Empty;
            if (dtData.Rows.Count > 0)
            {
                filepath = Convert.ToString(dtData.Rows[0]["PATH_TX"]);
                fileName = Convert.ToString(dtData.Rows[0]["FILE_NAME_TX"]);
            }

            // string filepath = AppDomain.CurrentDomain.BaseDirectory + FilePath + id.Split('_')[1] + "_" + filename;
            if (System.IO.File.Exists(filepath))
            {
                byte[] filedata = System.IO.File.ReadAllBytes(filepath);
                string contentType = MimeMapping.GetMimeMapping(filepath);

                var cd = new System.Net.Mime.ContentDisposition
                {
                    FileName = fileName,
                    Inline = false,
                };

                Response.AppendHeader("Content-Disposition", cd.ToString());

                return File(filedata, contentType);
            }
            return RedirectToAction("PCS");
        }
        public ActionResult DownloadApprovalLetterPCS(string id)
        {
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ACTIVE_YN", 1);
            conditions.Add("ID", id);

            string TableName = string.Empty;
            string FilePath = string.Empty;
            string fileName = string.Empty;
            TableName = "PCS_REGISTRATION_APPROVAL_T";
            FilePath = Convert.ToString(Util.UtilService.FilePath.PCSCOMPDOCS.GetEnumDisplayName());
            DataTable dtData = new DataTable();

            dtData = UtilService.getData("Training", TableName, conditions, null, 0, 1);


            //foreach (JProperty property in jdata.Properties())
            //{
            //    if (property.Name == TableName)
            //    {
            //        dtData = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
            //    }
            //}

            string filepath = string.Empty;
            if (dtData.Rows.Count > 0)
            {
                filepath = Convert.ToString(dtData.Rows[0]["PATH_TX"]);
                fileName = Convert.ToString(dtData.Rows[0]["FILE_NAME_TX"]+".pdf");
            }

            // string filepath = AppDomain.CurrentDomain.BaseDirectory + FilePath + id.Split('_')[1] + "_" + filename;
            if (System.IO.File.Exists(filepath))
            {
                byte[] filedata = System.IO.File.ReadAllBytes(filepath);
                string contentType = MimeMapping.GetMimeMapping(filepath);

                var cd = new System.Net.Mime.ContentDisposition
                {
                    FileName = fileName,
                    Inline = false,
                };

                Response.AppendHeader("Content-Disposition", cd.ToString());

                return File(filedata, contentType);
            }
            return RedirectToAction("PCS");
        }
        
        public JsonResult DeleteFile(string id)
        {
            ViewBag.Error = "";
            DataTable dt = new DataTable();
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ACTIVE_YN", 1);
            conditions.Add("ID", id.ToString());
            dt = UtilService.getData("Training", "PCS_REGISTRATION_DOCUMENT_T", conditions, null, 0, 1);
            if (dt.Rows.Count > 0)
            {
                if (dt.Rows[0]["STATUS_TX"].ToString() == "Approve")
                {
                    ViewBag.Error = "Cannot Delete an approved document.";
                    return Json("Failure");
                }
                else
                {
                    conditions.Clear();
                    Dictionary<string, object> data = new Dictionary<string, object>();
                    string UserName = HttpContext.Session["LOGIN_ID"].ToString();
                    string Session_Key = HttpContext.Session["SESSION_KEY"].ToString();
                    ActionClass actionClass = new ActionClass();
                    conditions.Add("ACTIVE_YN", 1);
                    List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
                    List<Dictionary<string, object>> lstData1 = new List<Dictionary<string, object>>();
                    data.Add("ID", id.Split('_')[0].ToString());
                    data.Add("ACTIVE_YN", 0);
                    data.Add("UPDATED_BY", UserName);
                    lstData1.Add(data);
                    lstData.Add(Util.UtilService.addSubParameter("Training", "PCS_REGISTRATION_DOCUMENT_T", 0, 0, lstData1, conditions));
                    AppUrl = AppUrl + "/AddUpdate";
                    actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "update", lstData));
                    return Json("Success");
                }
            }
            return Json("Failure");
        }
        [HttpPost]
        public ActionResult UploadFile(FormCollection frm)
        {
            string result = "Failure";
            // Verify that the user selected a file
            foreach (string file in Request.Files)
            {
                var fileContent = Request.Files[file];

                if (fileContent != null && fileContent.ContentLength > 0)
                {
                    PCSLayer.UploadFile(Request.Files[file], frm);
                    result = "Success";
                }
            }
            // redirect back to the index action to show the form once again
            //return RedirectToAction("PCS");
            return Json(result);
        }

        [Route("GetCompCdDtls")]
        [HttpGet]
        public JsonResult GetCompCdDtls(int comp_Cd)
        {
            string strcin_tx = string.Empty
                , strContactPerson = string.Empty
                , strContactPersonDesg = string.Empty
                , strContactPersonEmail = string.Empty
                , strContactPersonMob = string.Empty
                , strCompPCSNameTx = string.Empty;
            //GET COMP DETAILS
            Dictionary<string, object> conds = new Dictionary<string, object>();
            conds.Add("COMP_CD", comp_Cd);
            conds.Add("COMP_YN", 1);
            DataTable dtRes = Util.UtilService.getData("Training", "COMPANY_PCS_T", conds, null, 0, 10);
            if (dtRes != null && dtRes.Rows != null && dtRes.Rows.Count > 0)
            {
                if (dtRes.Rows[0]["CIN_TX"] != null) strcin_tx = Convert.ToString(dtRes.Rows[0]["CIN_TX"]);
                else strcin_tx = string.Empty;

                if (dtRes.Rows[0]["CONTACT_PERSON_TX"] != null) strContactPerson = Convert.ToString(dtRes.Rows[0]["CONTACT_PERSON_TX"]);
                else strContactPerson = string.Empty;

                if (dtRes.Rows[0]["CONTACT_PER_DESG_TX"] != null) strContactPersonDesg = Convert.ToString(dtRes.Rows[0]["CONTACT_PER_DESG_TX"]);
                else strContactPersonDesg = string.Empty;

                if (dtRes.Rows[0]["CONTACT_PER_EMAIL_TX"] != null) strContactPersonEmail = Convert.ToString(dtRes.Rows[0]["CONTACT_PER_EMAIL_TX"]);
                else strContactPersonEmail = string.Empty;

                if (dtRes.Rows[0]["CONTACT_PER_MOB_TX"] != null) strContactPersonMob = Convert.ToString(dtRes.Rows[0]["CONTACT_PER_MOB_TX"]);
                else strContactPersonMob = string.Empty;
                if (dtRes.Rows[0]["COMP_PCS_NAME_TX"] != null) strCompPCSNameTx = Convert.ToString(dtRes.Rows[0]["COMP_PCS_NAME_TX"]);
                else strContactPersonMob = string.Empty;

            }


            var jsonobj = new
            {
                strcin_tx = strcin_tx
                ,
                strContactPerson = strContactPerson
                ,
                strContactPersonDesg = strContactPersonDesg
                ,
                strContactPersonEmail = strContactPersonEmail
                ,
                strContactPersonMob = strContactPersonMob
                ,
                strCompPCSNameTx = strCompPCSNameTx
            };
            return Json(jsonobj, JsonRequestBehavior.AllowGet);
        }
    }
}