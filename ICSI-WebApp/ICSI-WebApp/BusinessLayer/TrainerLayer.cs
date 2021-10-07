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
using System.Web;
using System.Web.Mvc;
using static ICSI_WebApp.Util.UtilService;

namespace ICSI_WebApp.BusinessLayer
{
    public class TrainerLayer
    {
        public ActionClass beforeTrainerScreen1(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            frm["DID"] = "1";
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            string isQrtrEnds = "0";
            if (frm["UNIQUE_REG_ID"] != null && frm["UNIQUE_REG_ID"].ToString() != string.Empty)
            {
                HttpContext.Current.Session["stdQty_TRN_ID"] = frm["UNIQUE_REG_ID"].ToString();
                conditions.Add("ACTIVE_YN", 1);
                conditions.Add("QRTR_END_YN", 1);
                conditions.Add("TRAINING_ID", frm["UNIQUE_REG_ID"].ToString());
                DataTable dtt = UtilService.getData(UtilService.getApplicationScheme(screen), "student_quaterly_report_t", conditions, null, 0, 1);
                isQrtrEnds = dtt != null && dtt.Rows != null && dtt.Rows.Count > 0?"1":"0";
            }
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@IS_ALL_QRTRS_END", isQrtrEnds);
            conditions.Clear();
            conditions.Add("ACTIVE_YN", 1);
            conditions.Add("UNIQUE_REG_ID", HttpContext.Current.Session["LOGIN_ID"].ToString());
            DataTable dt = UtilService.getData("Training", "COMPANY_PCS_T", conditions, null, 0, 1);
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                string trainerSignature = Convert.ToString(dt.Rows[0]["TRAINER_SIGNATURE_TX"]);
                if (trainerSignature == null && trainerSignature.Trim().Equals(""))
                {
                    trainerSignature = "";
                }
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@TRAINER_SIGNATURE_TX", trainerSignature);
            }
            
            

            return UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterTrainerScreen1(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass actionClass = new ActionClass();
            Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
            if (frm != null)
            {
                frm["DateFormat"] = "dd/MM/yyyy";
                if (frm["h_SIGNATURE_CHANGED"] != null && frm["h_SIGNATURE_CHANGED"] != "" && frm["h_SIGNATURE_CHANGED"] != "0")
                {
                    Dictionary<string, object> conditions = new Dictionary<string, object>();
                    conditions.Add("ACTIVE_YN", 1);
                    conditions.Add("UNIQUE_REG_ID", HttpContext.Current.Session["LOGIN_ID"].ToString());
                    DataTable masterTbl = UtilService.getData(UtilService.getApplicationScheme(screen), "COMPANY_PCS_T", conditions, null, 0, 1);
                    if (masterTbl != null && masterTbl.Rows != null && masterTbl.Rows.Count > 0)
                    {
                        masterTbl.Rows[0]["TRAINER_SIGNATURE_TX"] = Convert.ToString(frm["hTRAINER_SIGNATURE_TX"]);
                        List<Dictionary<string, object>> list =
                                masterTbl.AsEnumerable().Select(
                                row => masterTbl.Columns.Cast<DataColumn>().ToDictionary(
                                    column => column.ColumnName as string,    // Key
                                    column => row[column] // Value
                                )
                            ).ToList();
                        UtilService.insertOrUpdate(UtilService.getApplicationScheme(screen), "COMPANY_PCS_T", list);
                    }
                }
            }
            bool isSpecial = frm["is_special"] != null && !frm["is_special"].Trim().Equals("") && frm["is_special"] == "1";
            if (HttpContext.Current.Session["stdQty_TRN_ID"] != null && HttpContext.Current.Session["stdQty_TRN_ID"].ToString() != string.Empty)
            {
                frm["TRAINING_ID"] = HttpContext.Current.Session["stdQty_TRN_ID"].ToString();
            }
            if (frm["QUARTER_ID"] != null && frm["QUARTER_ID"].ToString() != string.Empty)
            {
                DataTable dt = new DataTable();
                string trnID = HttpContext.Current.Session["stdQty_TRN_ID"].ToString();
                string quarterlyID = frm["QUARTER_ID"].ToString();
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("QUARTER_ID", quarterlyID);
                conditions.Add("TRAINING_ID", trnID);
                conditions.Add("ACTIVE_YN", 1);
                dt = UtilService.getData("Training", "student_quaterly_report_t", conditions, null, 0, 1);

                if (dt != null && dt.Rows.Count > 0)
                {
                    if (isSpecial)
                    {
                        dt.Rows[0]["QRTR_END_YN"] = Convert.ToInt32(frm["QRTR_END_YN"]);
                        List<Dictionary<string, object>> list =
                            dt.AsEnumerable().Select(
                            row => dt.Columns.Cast<DataColumn>().ToDictionary(
                                column => column.ColumnName as string,    // Key
                                column => row[column] // Value
                            )
                        ).ToList();
                        UtilService.insertOrUpdate(UtilService.getApplicationScheme(screen), "student_quaterly_report_t", list);
                    }                  
                    frm["s"] = "update";                    
                    frm["ui"] = dt.Rows[0]["ID"].ToString();
                    frm["ID"] = dt.Rows[0]["ID"].ToString();                    
                }
            }
            isSpecial = isSpecial || (frm["is_special"] != null && !frm["is_special"].Trim().Equals("") && frm["is_special"] == "2");
            if (!isSpecial)
            {
                frm["SUBMIT_DT"] = DateTime.Now.ToString("dd/MM/yyyy");
                actionClass = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
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
                                frm["ID"] = Convert.ToString(dtb.Rows[0]["ID"]);
                                frm["ui"] = Convert.ToString(dtb.Rows[0]["ID"]);
                            }
                        }
                    }
                }
                frm["nextscreen"] = Convert.ToString(screen.Screen_Next_Id);
            }
            else
            {
                
                DataTable dt = new DataTable();
                string trnID = HttpContext.Current.Session["stdQty_TRN_ID"].ToString();
                string quarterlyID = frm["QUARTER_ID"].ToString();
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("QUARTER_ID", quarterlyID);
                conditions.Add("TRAINING_ID", trnID);
                conditions.Add("ACTIVE_YN", 1);
                actionClass = UtilService.GetRawData(conditions, null, "student_quaterly_report_t",  0, 1,UtilService.getApplicationScheme(screen));
                actionClass.StatCode = "0";
                actionClass.StatMessage = "Success";
                string s = "new";
                string si = frm["si"];
                string ui = frm["ui"];
                string uniqueRegId = frm["UNIQUE_REG_ID"];
                string u = frm["u"];
                List<string> coll = new List<string>();
                foreach(string key in frm.Keys)
                {
                    coll.Add(key);
                }
                foreach (string key in coll) frm[key] = null;
                frm["s"] = s;
                frm["si"] = si;
                frm["ui"] = ui;
                //frm["UNIQUE_REG_ID"] = uniqueRegId;
                frm["u"] = u;
                frm["nextscreen"] = "148";
            }
            return actionClass;
        }

        public ActionClass beforeQuarterlyReportAdmin(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            frm["DID"] = "1";
            frm["s"] = "edit";
            return UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass beforeQuarterlyReport2(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ACTIVE_YN", 1);
            conditions.Add("UNIQUE_REG_ID", HttpContext.Current.Session["LOGIN_ID"].ToString());
            DataTable dt = UtilService.getData("Training", "COMPANY_PCS_T", conditions, null, 0, 1);
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                string trainerSignature = Convert.ToString(dt.Rows[0]["TRAINER_SIGNATURE_TX"]);
                if (trainerSignature == null && trainerSignature.Trim().Equals(""))
                {
                    trainerSignature = "";
                }
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@TRAINER_SIGNATURE_TX", trainerSignature);
            }
            return UtilService.beforeLoad(WEB_APP_ID, frm);
        }
        public ActionClass afterQuarterlyReport2(int WEB_APP_ID, FormCollection frm)
        {
            if (frm != null)
            {
                if (frm["h_SIGNATURE_CHANGED"] != null && frm["h_SIGNATURE_CHANGED"] != "" && frm["h_SIGNATURE_CHANGED"] != "0")
                {
                    Dictionary<string, object> conditions = new Dictionary<string, object>();
                    conditions.Add("ACTIVE_YN", 1);
                    conditions.Add("UNIQUE_REG_ID", HttpContext.Current.Session["LOGIN_ID"].ToString());
                    DataTable masterTbl = UtilService.getData("Training", "COMPANY_PCS_T", conditions, null, 0, 1);
                    if (masterTbl != null && masterTbl.Rows != null && masterTbl.Rows.Count > 0)
                    {
                        masterTbl.Rows[0]["TRAINER_SIGNATURE_TX"] = Convert.ToString(frm["hTRAINER_SIGNATURE_TX"]);
                        List<Dictionary<string, object>> list =
                                masterTbl.AsEnumerable().Select(
                                row => masterTbl.Columns.Cast<DataColumn>().ToDictionary(
                                    column => column.ColumnName as string,    // Key
                                    column => row[column] // Value
                                )
                            ).ToList();
                        UtilService.insertOrUpdate("Training", "COMPANY_PCS_T", list);
                    }
                }
            }

            #region For Email
            ActionClass actionClass = new ActionClass();
            Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
            actionClass = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
            string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
            string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
            AppUrl = AppUrl + "/AddUpdate";
            int uID = 0;            
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
                            uID = Convert.ToInt32(dtb.Rows[0]["ID"]);                          
                        }
                    }
                }
            }
            
            if (screen.is_Email_yn)
                storeEmailData(actionClass, screen, "insert", AppUrl, Session_Key, UserName, uID);
            #endregion

            //return UtilService.afterSubmit(WEB_APP_ID, frm);
            return actionClass;
        }


        public ActionClass beforeCompanyNameEditDocument(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            string compid = "";
            compid = Convert.ToString((screen.ScreenComponents.Where(x => x.Comp_Type_Nm == 27)).ToList()[0].Id);
            return UtilService.searchLoad(WEB_APP_ID, frm, screen.Action_Tx, compid, Convert.ToString(screen.ID));
        }

        public ActionClass afterCompanyNameEditDocument(int WEB_APP_ID, FormCollection frm)
        {

            if (frm["REMOVE_NM"] == "1")
            {
                frm["s"] = "update";
                frm.Add("ID", frm["REMOVE_ID"]);
                frm.Add("ACTIVE_YN", "0");
                string filePath = frm["PATH_TX"];
                System.IO.File.Delete(filePath);
            }
            else
            {
                string File_name_tx = string.Empty;
                string file_path_tx = string.Empty;
                string FolderName = string.Empty;
                FolderName = "Uploads\\COMPANY_NAME_DOCUMENT\\REF_ID_" + Convert.ToString(frm["REF_ID"]);
                if (ConfigurationManager.AppSettings.AllKeys.Contains("DOCUMENT_ROOT"))
                {
                    FolderName = Convert.ToString(ConfigurationManager.AppSettings["DOCUMENT_ROOT"]) + FolderName;
                }
                else
                {
                    FolderName = AppDomain.CurrentDomain.BaseDirectory + FolderName;
                }

                if (!string.IsNullOrEmpty(FolderName))
                {
                    string _FileName = System.IO.Path.GetFileName(HttpContext.Current.Request.Files[0].FileName);
                    string _PathExt = System.IO.Path.GetExtension(HttpContext.Current.Request.Files[0].FileName);

                    string _path = FolderName;
                    string _FullPath = System.IO.Path.Combine(_path, _FileName);

                    if (!(System.IO.Directory.Exists(_path)))
                        System.IO.Directory.CreateDirectory(_path);

                    chechagain:

                    if (System.IO.File.Exists(_FullPath))
                    {
                        _FileName = "1_" + _FileName;
                        _FullPath = System.IO.Path.Combine(_path, _FileName);
                        goto chechagain;
                    }
                    else
                    {
                        File_name_tx = _FileName;
                        file_path_tx = _FullPath;
                        frm.Add("FILE_NAME_TX", File_name_tx);
                        frm.Add("FILE_PATH_TX", file_path_tx);
                        HttpContext.Current.Request.Files[0].SaveAs(_FullPath);
                    }
                }
            }
            ActionClass act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
            return act;
        }
    }
}