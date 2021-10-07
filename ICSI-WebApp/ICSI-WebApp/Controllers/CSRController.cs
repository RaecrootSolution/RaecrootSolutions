using ICSI_WebApp.Models;
using ICSI_WebApp.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ICSI_WebApp.Controllers
{
    public class CSRController : Controller
    {
        // GET: CSRAwards
        private int WEB_APP_ID = Convert.ToInt32(Convert.ToString(ConfigurationManager.AppSettings["WEB_APP_ID"]));
        private string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
        private string PGUrl = Convert.ToString(ConfigurationManager.AppSettings["PGUrl"]);
        private string PGAppUrl = Convert.ToString(ConfigurationManager.AppSettings["PGAppUrl"]);
        private string PGHDFCUrl = Convert.ToString(ConfigurationManager.AppSettings["PGHDFCUrl"]);
        private ManagementData objApplicationObject;
        public ActionResult CSR(FormCollection frm)
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
                else return RedirectToAction("Home", "CSR");
            }
            else
                return RedirectToAction("logout");
        }

        private void login()
        {
            string UserName = "csranonymous";
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

        [HttpGet]
        public ActionResult logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("login", "Home");
        }

        [HttpGet]
        public ActionResult Home()
        {
            if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null && ((List<object>)Session["USER_MENU"]).Count > 0 && Convert.ToString(Session["LOGIN_ID"]) != "csranonymous")
            {
                List<object> menu = (List<object>)Session["USER_MENU"];
                DataRow row = (DataRow)menu[0];
                Screen_T screen = UtilService.GetScreenData(WEB_APP_ID, row, null, null);

                if (screen.Screen_Class_Name_Tx != null && screen.Screen_Method_Name_Tx != null && !screen.Screen_Class_Name_Tx.Trim().Equals("") && !screen.Screen_Method_Name_Tx.Trim().Equals(""))
                {
                    UtilService.executeMethod(screen.Screen_Class_Name_Tx, null, "before" + screen.Screen_Method_Name_Tx.Trim(), new object[] { WEB_APP_ID, null, screen }, (bool)screen.Screen_Class_Static_YN, (bool)screen.Screen_Method_Static_YN);
                }
                screen.resActionClass = (ActionClass)TempData["actionClass"];

                ViewBag.Title = "Home";
                if (screen != null)
                {
                    //if (screen.Screen_File_Name_Tx != null && !screen.Screen_File_Name_Tx.Trim().Equals("")) return RedirectToAction(screen.Screen_File_Name_Tx);
                    //else
                    return View(screen);
                }
                else return View();
            }
            else
                return RedirectToAction("logout");
        }

        [HttpPost]
        public ActionResult Home(FormCollection frm)
        {
            if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null && ((List<object>)Session["USER_MENU"]).Count > 0 || (Convert.ToString(Session["LOGIN_ID"]) == "pcscomp" || Convert.ToInt32(Session["USER_TYPE_ID"]) == 5 || Convert.ToInt32(Session["USER_TYPE_ID"]) == 6))
            {
                string userid = frm["u"];
                string menuid = frm["m"];
                string ScreenType = frm["s"];
                string screenId = frm["si"];
                string pgtype = frm["pt"];
                if (ScreenType.Equals("pg") && !string.IsNullOrEmpty(pgtype))
                {
                    Screen_T PGST = new Screen_T();
                    if (pgtype == "initiate")
                    {
                        string items = frm["items"];
                        Dictionary<string, object> itemObj = null;
                        double amt = 0;
                        if (items != null)
                        {
                            itemObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(items);
                            foreach (var itm in itemObj)
                            {
                                if (itm.Key.Equals("total")) amt = Convert.ToDouble(itm.Value);
                            }
                            itemObj.Remove("total");
                        }
                        string returnURL = "return loadConfirmScreen()";
                        Dictionary<string, object> data = new Dictionary<string, object>();
                        data.Add("ptype", pgtype);
                        data.Add("scid", screenId);
                        data.Add("sid", userid);
                        data.Add("amt", amt);
                        data.Add("officeid", frm["offid"]);
                        data.Add("desc", frm["desc"]);
                        data.Add("items", itemObj);
                        data.Add("custname", frm["custname"]);
                        data.Add("email", frm["email"]);
                        data.Add("mobile", frm["mob"]);
                        data.Add("billing", frm["billing"]);
                        data.Add("shipping", frm["shipping"]);
                        data.Add("taxamt", frm["taxamt"]);
                        data.Add("gstnm", frm["gstnm"]);
                        data.Add("remarks", frm["remarks"]);
                        data.Add("returnurl", returnURL);
                        data.Add("stdregno", frm["stdregno"]);
                        data.Add("session", Convert.ToString(HttpContext.Session["SESSION_KEY"]));
                        data.Add("loginId", Convert.ToString(HttpContext.Session["LOGIN_ID"]));
                        data.Add("refId", frm["hidUI"]);
                        data.Add("addinfo1", frm["addinfo1"]);

                        Dictionary<string, object> strresDict = UtilService.createRequestForPaymentGateway(PGUrl, "Initiate", data);
                        string strres = Convert.ToString(strresDict["strInitiate"]);
                        if (!strres.Equals(""))
                        {
                            strres = strres + "<input type='hidden' name='pt' value='confirm'><input type='hidden' name='si' value='" + screenId + "'>";
                            string script = "<script> function LoadProceedScreen(tpp){$('#s').val('pg'); return true;}</script>";
                            PGST.Screen_Script_Tx = script;// PGST.Screen_Script_Tx == null ? script : (PGST.Screen_Script_Tx + script);
                            Session["PGOBJECT"] = strresDict["pgDetails"];
                        }
                        PGST.Screen_Content_Tx = strres;
                        return View(PGST);
                    }
                    else if (pgtype.Equals("receipt"))
                    {
                        Session["SESSION_KEY"] = frm["sskey"];
                        PGST.Screen_Content_Tx = frm["html"];
                    }
                    else if (pgtype.Equals("confirm"))
                    {
                        Dictionary<string, object> data = new Dictionary<string, object>();
                        string Session_Key = Convert.ToString(Session["SESSION_KEY"]);

                        data.Add("StrBankMerchantId", frm["selMode"]);
                        data.Add("pgDetails", Session["PGOBJECT"]);
                        data.Add("scid", screenId);
                        data.Add("sid", userid);
                        Dictionary<string, object> strresDict = UtilService.createRequestForPaymentGateway(PGUrl, "LoadNextScreen", data);
                        string strres = Convert.ToString(strresDict["strInitiate"]);
                        if (!strres.Equals(""))
                        {
                            string jstr = JsonConvert.SerializeObject(strresDict["pgDetails"]);
                            string eval = CryptographyUtil.EncryptDataPKCS7(jstr, Session_Key);
                            //Dictionary<string, object> dataa = (Dictionary<string, object>)Session["PGOBJECT"];
                            //string json = JsonConvert.SerializeObject(dataa, Formatting.Indented);
                            strres = strres + "<input type='hidden' name='pt' value='confirm'><input type='hidden' name='si' value='" + screenId + "'>";
                            StringBuilder sb = new StringBuilder();
                            sb.Append("<script> function processpayment() { ")
                                .Append("var url=''; ")
                                .Append("var pgMode = $('#lblpgMode').val(); ")
                                .Append("var bool = confirm('Are you sure to continue');if (bool){")
                                .Append("var frm=document.createElement('form');frm.method='post'; ")
                                .Append("frm.action='").Append(PGAppUrl).Append("';")
                                .Append("var pgdtls = '")
                                .Append(eval)
                                .Append("';var sskey='").Append(Session_Key).Append("';frm.appendChild(createInputHidden('sskey',sskey));frm.appendChild(createInputHidden('pgDetails',pgdtls));document.body.appendChild(frm);frm.submit();}}</script>");
                            PGST.Screen_Script_Tx = sb.ToString();// PGST.Screen_Script_Tx == null ? script : (PGST.Screen_Script_Tx + script);
                            Session["PGOBJECT"] = strresDict["pgDetails"];
                        }
                        PGST.Screen_Content_Tx = strres;
                        return View(PGST);
                    }
                    return View(PGST);
                }
                else
                {
                    if (!string.IsNullOrEmpty(screenId))
                    {
                        if (screenId.Contains(","))
                        {
                            screenId = screenId.Split(',')[0];
                            frm["si"] = screenId;
                        }
                    }

                    /*if (ScreenType.ToLower() == "payment")
                    {
                        string ReqID = Convert.ToString(frm["PROCESS_REQUEST_ID"]);
                        int transactionreqid = Convert.ToInt32(Convert.ToString(frm["ONLINE_PAYMENT_DETAIL_T.ID"]));
                        string TotAmount = Convert.ToString(frm["AMOUNT_NM"]);
                        string PayMode = "9";
                        PayMode = "9";
                        string ProcessRoute = Url.RequestContext.HttpContext.Request.RawUrl;
                        PaymentGatewayData objPGData = new PaymentGatewayData();
                        string RequestMsg = string.Empty;
                        TotAmount = "2.00";
                        PaymentTransactionEntity objTxnEntity = objPGData.GetPaymentRequestData(TotAmount, ReqID, PayMode, ProcessRoute, "Dummy", transactionreqid, ref RequestMsg);
                        Session["RequetID"] = objTxnEntity.RequestID;
                        Session["UserID"] = frm["u"];
                        return View("PGMiddleView", new PGMiddleURL { CheckoutUrl = RequestMsg });
                    }
                    else*/
                    {
                        if (frm["scrtype"] == "update")
                        {
                            ScreenType = "update";
                            frm["s"] = ScreenType;
                        }

                        ActionClass act = new ActionClass();
                        if (userid != null && !userid.Trim().Equals("") && ((menuid != null && !menuid.Trim().Equals("")) || (screenId != null && !screenId.Trim().Equals(""))))
                        {
                            if (frm["UNIQUE_REG_ID"] != null && frm["UNIQUE_REG_ID"] != string.Empty && (screenId == "156" || screenId == "157" || screenId == "204" || screenId == "205"))
                            {
                                Session["stdQty_TRN_ID"] = frm["UNIQUE_REG_ID"].ToString();
                            }
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
                        else return RedirectToAction("Home");
                    }
                }
            }
            else
                return RedirectToAction("logout");
        }

        public JsonResult GetDropDownData(string TableName, string condition, string schema)
        {
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ACTIVE_YN", 1);

            if (!string.IsNullOrEmpty(condition))
            {
                string[] splitfirst = condition.Split('/');
                for (int i = 0; i < splitfirst.Length; i++)
                {
                    string[] splitSecond = splitfirst[i].Split('-');
                    if (splitSecond.Length > 0)
                        conditions.Add(splitSecond[0], splitSecond[1]);
                }
            }

            string[] splitTBL = TableName.Split('-');
            JObject jdata = null;
            if (splitTBL.Length > 1)
            {
                TableName = splitTBL[0];
                jdata = DBTable.GetData("fetch", conditions, TableName, 0, 100, splitTBL[1]);
            }
            else
                jdata = DBTable.GetData("fetch", conditions, TableName, 0, 100, schema != null ? schema : Util.UtilService.getSchemaNameById(1));

            DataTable dtData = new DataTable();
            foreach (JProperty property in jdata.Properties())
            {
                if (property.Name == TableName)
                {
                    //dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    dtData = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                }
            }

            return Json(Util.UtilService.DataTableToJSON(dtData));
        }

        public ActionResult DownloadFileByIDFromSpecificTable(string id, string TableName, string ColumnName, string schema)
        {
            if (string.IsNullOrEmpty(schema))
                schema = "Training";

            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ACTIVE_YN", 1);
            conditions.Add("ID", id);
            string FilePath = string.Empty;
            string fileName = string.Empty;
            DataTable dtData = new DataTable();

            dtData = UtilService.getData(schema, TableName, conditions, null, 0, 1);
            string filepath = string.Empty;
            if (dtData.Rows.Count > 0)
            {
                //filepath = Convert.ToString(dtData.Rows[0]["PATH_TX"]);
                filepath = Convert.ToString(dtData.Rows[0][ColumnName]);
            }
            if (System.IO.File.Exists(filepath))
            {
                byte[] filedata = System.IO.File.ReadAllBytes(filepath);
                string contentType = MimeMapping.GetMimeMapping(filepath);

                var cd = new System.Net.Mime.ContentDisposition
                {
                    //FileName = fileName,
                    Inline = false,
                };

                Response.AppendHeader("Content-Disposition", cd.ToString());

                return File(filedata, contentType);
            }
            else
            {
                if (fileName != null && !fileName.Trim().Equals(""))
                {
                    int pos = filepath.LastIndexOf(fileName);
                    if (pos > -1)
                    {
                        string path = filepath.Substring(0, pos);
                        string[] filePaths = Directory.GetFiles(path, fileName + "*");
                        if (filePaths != null && filePaths.Length > 0)
                        {
                            byte[] filedata = System.IO.File.ReadAllBytes(filePaths[0]);
                            string contentType = MimeMapping.GetMimeMapping(filePaths[0]);

                            var cd = new System.Net.Mime.ContentDisposition
                            {
                                //FileName = fileName,
                                Inline = false,
                            };

                            Response.AppendHeader("Content-Disposition", cd.ToString());

                            return File(filedata, contentType);
                        }
                    }
                }
            }
            return RedirectToAction("Home");
        }

        [HttpPost]
        public JsonResult AjaxPagenationSearch(FormCollection frm)
        {
            string scrcontent = string.Empty;
            try
            {
                scrcontent = UtilService.AjaxSearch(WEB_APP_ID, frm);
            }
            catch (Exception ex) { scrcontent = string.Empty; }
            var json = Json(new
            {
                resMessage = scrcontent.Trim().Equals("") ? "Fail" : "Success",
                resCode = scrcontent.Trim().Equals("") ? -1 : 0,
                htmlstring = scrcontent
            });
            return json;
        }

        public ActionResult ExportToExcel(int Id)
        {
            int CurrentYear = 0;
            if (Session["ACTIVE_ARCHIVE_YEAR_ID"] == null)
                BusinessLayer.CSRLayer.GetActiveYear();
            CurrentYear = Convert.ToInt32(Session["ACTIVE_ARCHIVE_YEAR"]);

            string FileName = string.Empty;
            if (Id == 0)
                FileName = "CSR-Nominations-" + CurrentYear + ".xls";
            else if (Id == 1)
                FileName = "Evaluation-of-CSR-Awards-Companies-Large-" + CurrentYear + ".xls";
            else if (Id == 2)
                FileName = "Evaluation-of-CSR-Awards-Companies-Medium-" + CurrentYear + ".xls";
            else if (Id == 3)
                FileName = "Evaluation-of-CSR-Awards-Companies-Emerging-" + CurrentYear + ".xls";
            else if (Id == 4)
                FileName = "CSR-Registered-Companies-" + CurrentYear + ".xls";

            var gv = new GridView();
            if (Id == 0)
                gv.DataSource = BusinessLayer.CSRLayer.NominationsReport();
            else if (Id == 4)
                gv.DataSource = BusinessLayer.CSRLayer.RegisteredCompanies();
            else
                gv.DataSource = BusinessLayer.CSRLayer.EvaluationReport(Id);
            gv.DataBind();
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=" + FileName + "");
            Response.ContentType = "application/ms-excel";
            Response.Charset = "";
            StringWriter objStringWriter = new StringWriter();
            HtmlTextWriter objHtmlTextWriter = new HtmlTextWriter(objStringWriter);
            gv.RenderControl(objHtmlTextWriter);
            Response.Output.Write(objStringWriter.ToString());
            Response.Flush();
            Response.End();
            return View("Home");
        }

        public ActionResult DownloadFile(string id)
        {
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ACTIVE_YN", 1);
            conditions.Add("ID", id.Split('_')[1]);

            string TableName = string.Empty;
            string FilePath = string.Empty;
            if (id.Split('_')[0] == "7")
            {
                TableName = Convert.ToString(Util.UtilService.TableName.CSR_AWARDS_SUPPORTING_DOCS_T);
                FilePath = ConfigurationManager.AppSettings["DOCUMENT_ROOT"].ToString() + Convert.ToString(Util.UtilService.FilePath.CSRAwardsSupportingDocs.GetEnumDisplayName());
            }

            JObject jdata = DBTable.GetData("fetch", conditions, TableName, 0, 100, "CSR");

            DataTable dtData = new DataTable();
            foreach (JProperty property in jdata.Properties())
            {
                if (property.Name == TableName)
                    dtData = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
            }

            string filepath = string.Empty;
            string FileName = string.Empty;
            int attID = 0;
            if (dtData.Rows.Count > 0)
            {
                filepath = Convert.ToString(dtData.Rows[0]["DOC_PATH_TX"]);
                attID = Convert.ToInt32(dtData.Rows[0]["ID"]);
                FileName = filepath.Replace(FilePath, "").Replace("\\", "");
                filepath = FilePath + attID + "_" + FileName;
            }
            //string filepath = UtilService.getDocumentPath(FilePath) + id.Split('_')[1] + "_" + filename;

            //string[] arr = filepath.Split('\\\\');

            if (System.IO.File.Exists(filepath))
            {
                byte[] filedata = System.IO.File.ReadAllBytes(filepath);
                string contentType = MimeMapping.GetMimeMapping(filepath);

                var cd = new System.Net.Mime.ContentDisposition
                {
                    FileName = FileName,
                    Inline = true,
                };

                return new FileStreamResult(new MemoryStream(filedata), contentType) { FileDownloadName = FileName };
            }
            return HttpNotFound();
        }

        [HttpPost]
        public JsonResult AjaxData()
        {
            if (Request.Form["Comp_ID"] != null)
            {
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                int compid = 0;
                int.TryParse(Convert.ToString(Request.Form["Comp_ID"]), out compid);
                conditions.Add("ID", compid);
                for (int i = 0; i < Request.Form.AllKeys.Count(); i++)
                {
                    if (Request.Form.GetKey(i).Trim() != "Comp_ID" && Request.Form.GetKey(i).Trim() != "ICSI_SCHEMA")
                    {
                        conditions.Add(Request.Form.GetKey(i), Request.Form[Request.Form.GetKey(i)]);
                    }
                }
                string schema = string.Empty;
                if (Convert.ToString(Request.Form["ICSI_SCHEMA"]) == "true")
                    schema = "ICSI";
                else if (!string.IsNullOrEmpty(Request.Form["ICSI_SCHEMA"]))
                    schema = Convert.ToString(Request.Form["ICSI_SCHEMA"]);

                JObject jdata = DBTable.GetData("ajax", conditions, "SCREEN_COMP_T", 0, 100, "CSR");
                DataTable dt = new DataTable();
                foreach (JProperty property in jdata.Properties())
                {
                    if (property.Name == "ajax")
                    {
                        dt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    }
                }
                if (dt != null)
                    return Json(Util.UtilService.DataTableToJSON(dt));
                else
                    return Json("");
            }
            else
            {
                return Json("");
            }
        }
    }
}