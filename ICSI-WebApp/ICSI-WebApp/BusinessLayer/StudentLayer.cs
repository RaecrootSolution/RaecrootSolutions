using ICSI_WebApp.Models;
using ICSI_WebApp.Util;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Globalization;

namespace ICSI_WebApp.BusinessLayer
{
    public class StudentLayer
    {
        public ActionClass beforeSponsorshipLetter(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            frm["uniqueId"] = Convert.ToString(HttpContext.Current.Session["STUDENT_ID"]);
            Screen_T screen1 = Util.UtilService.screenObject(WEB_APP_ID, frm);
            screen.Screen_Content_Tx = screen1.Screen_Content_Tx;
            screen1 = null;
            return Util.UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterSponsorshipLetter(int WEB_APP_ID, FormCollection frm)
        {
            return Util.UtilService.afterSubmit(WEB_APP_ID, frm);
        }


        public ActionClass beforeLicentiateEducation(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            string compid = "";
            compid = Convert.ToString((screen.ScreenComponents.Where(x => x.Comp_Type_Nm == 27)).ToList()[0].Id);
            return UtilService.searchLoad(WEB_APP_ID, frm, screen.Action_Tx, compid, Convert.ToString(screen.ID));
        }

        public ActionClass afterLicentiateEducation(int WEB_APP_ID, FormCollection frm)
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
                FolderName = "MEMBERSHIP\\LICENTIATE\\UPLOADS\\STUDENT_ID_" + Convert.ToString(frm["STUDENT_ID"]) + "\\" + Convert.ToString(frm["LICENTIATE_REG_ID"]) + "\\" + Convert.ToString(frm["QUALIFICATION_ID"]);
                if (ConfigurationManager.AppSettings.AllKeys.Contains("GLOBAL_DOCUMENT_ROOT"))
                {
                    FolderName = Convert.ToString(ConfigurationManager.AppSettings["GLOBAL_DOCUMENT_ROOT"]) + FolderName;
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

        public ActionClass beforeFCSEducation(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            string compid = "";
            compid = Convert.ToString((screen.ScreenComponents.Where(x => x.Comp_Type_Nm == 27)).ToList()[0].Id);
            return UtilService.searchLoad(WEB_APP_ID, frm, screen.Action_Tx, compid, Convert.ToString(screen.ID));
        }

        public ActionClass afterFCSEducation(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass act = null;
            if (frm["REMOVE_NM"] == "1")
            {
                frm["s"] = "update";
                frm.Add("ID", frm["REMOVE_ID"]);
                frm.Add("ACTIVE_YN", "0");
                string filePath = frm["PATH_TX"];
                System.IO.File.Delete(filePath);

                List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
                Dictionary<string, object> d = new Dictionary<string, object>();
                d["ID"] = frm["REMOVE_ID"];
                d["ACTIVE_YN"] = "0";
                d["REMOVE_NM"] = frm["REMOVE_NM"];

                list.Add(d);
                act = UtilService.insertOrUpdate("Training", "MEM_FCS_REG_EDUCATION_DTL_T", list);
            }
            else
            {
                string File_name_tx = string.Empty;
                string file_path_tx = string.Empty;
                string FolderName = string.Empty;
                FolderName = "MEMBERSHIP\\FCS\\UPLOADS\\STUDENT_ID_" + Convert.ToString(frm["MEMNO_NM"]) + "\\" + Convert.ToString(frm["FCS_REG_ID"]) + "\\" + Convert.ToString(frm["QUALIFICATION_ID"]);
                if (ConfigurationManager.AppSettings.AllKeys.Contains("GLOBAL_DOCUMENT_ROOT"))
                    FolderName = Convert.ToString(ConfigurationManager.AppSettings["GLOBAL_DOCUMENT_ROOT"]) + FolderName;
                else
                    FolderName = AppDomain.CurrentDomain.BaseDirectory + FolderName;

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
                act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
            }
            return act;
        }

        public ActionClass beforeDOBUpload(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            return Util.UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterDOBUpload(int WEB_APP_ID, FormCollection frm)
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
                FolderName = "STUDENT\\DOB\\UPLOADS\\STUDENT_ID_" + Convert.ToString(frm["STUDENT_ID"]);
                if (ConfigurationManager.AppSettings.AllKeys.Contains("GLOBAL_DOCUMENT_ROOT"))
                {
                    FolderName = Convert.ToString(ConfigurationManager.AppSettings["GLOBAL_DOCUMENT_ROOT"]) + FolderName;
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

        public ActionClass beforeSwitchOver(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            int STUDENT_HDR_ID = Convert.ToInt32(HttpContext.Current.Session["STUDENT_HDR_ID"]);
            DataTable dtexmStatus = new DataTable();
            dtexmStatus = UtilService.GetExaminationDetails(STUDENT_HDR_ID);
            int executive_passed = 0;
            string execuitve_month = "";
            string execuitve_year = "";

            for (int i = 0; i < dtexmStatus.Rows.Count; i++)
            {
                if (Convert.ToString(dtexmStatus.Rows[i]["COURSEID"]) == "2" && Convert.ToString(dtexmStatus.Rows[i]["EXM_STATUS"]) == "Passed")
                {
                    executive_passed = 1;
                    execuitve_month = Convert.ToString(dtexmStatus.Rows[i]["PASSEDMONTH"]);
                    execuitve_year = Convert.ToString(dtexmStatus.Rows[i]["PASSEDYEAR"]);
                }
            }
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@EXE_PASSED", executive_passed.ToString());
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@EXE_MONTH", execuitve_month.ToString());
            screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@EXE_YEAR", execuitve_year.ToString());

            return Util.UtilService.beforeLoad(WEB_APP_ID, frm);

            //if (executive_passed == 0)
            //{
            //    ActionClass act = Util.UtilService.GetMessage(-301);
            //    screen.Screen_Content_Tx = "Switch over is not applicable as you have not passed executive passed yet.";
            //    return act;
            //}
            //Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
            //string applicationSchema = Util.UtilService.getApplicationScheme(screen);
            //Dictionary<string, object> conditions = new Dictionary<string, object>();
            //conditions.Add("ACTIVE_YN", 1);
            //conditions.Add("STUDENT_ID", Convert.ToInt32(HttpContext.Current.Session["STUDENT_ID"]));
            //DataTable dt = Util.UtilService.getData(applicationSchema, "SWITCH_OVER_T", conditions, null, 0, 1);
            //if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            //{
            //    ActionClass act = Util.UtilService.GetMessage(-301);
            //    screen.Screen_Content_Tx = "Already Switched over to Modified training";
            //    return act;
            //}
            //else
            //{
            //    UserData userdata = (UserData)HttpContext.Current.Session["USER_DATA"];
            //    if (userdata != null && userdata.STUDENT_T != null && userdata.STUDENT_T.Rows != null && userdata.STUDENT_T.Rows.Count > 0)
            //    {
            //        DateTime dateTime = userdata.STUDENT_T.Rows[0].Field<DateTime>("REG_DT");
            //        if (dateTime != null && dateTime < DateTime.Parse("2014/04/01"))
            //        {
            //            return Util.UtilService.beforeLoad(WEB_APP_ID, frm);
            //        }
            //        else
            //        {
            //            screen.Screen_Content_Tx = "You are not eligible for Switch Over!";
            //            return Util.UtilService.GetMessage(-302);
            //        }
            //    }
            //    else
            //        return Util.UtilService.beforeLoad(WEB_APP_ID, frm);
            //}
        }

        public ActionClass afterSwitchOver(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);

            Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
            string applicationSchema = Util.UtilService.getApplicationScheme(screen);
            // TODO - needs to update all the earlier training records to ACTIVE_YN=0

            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ACTIVE_YN", 1);
            conditions.Add("STUDENT_ID", Convert.ToInt32(HttpContext.Current.Session["STUDENT_ID"]));
            //
            string str_id = "0";
            if (frm["NEW_STR_ID"] != null)
            {
                str_id = Convert.ToString(frm["NEW_STR_ID"]);
            }
            if (str_id == "2")
            {
                DataTable dt = Util.UtilService.getData(applicationSchema, "STUDENT_REGISTER_TRAINING_T", conditions, null, 0, 1);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    Dictionary<string, object> d1 = dt.AsEnumerable().Select(row => dt.Columns.Cast<DataColumn>().ToDictionary(column => column.ColumnName, column => row[column])).FirstOrDefault();
                    d1["ACTIVE_YN"] = 0;
                    conditions.Clear();
                    string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
                    string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
                    string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
                    AppUrl = AppUrl + "/AddUpdate";
                    List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
                    List<Dictionary<string, object>> lstData1 = new List<Dictionary<string, object>>();
                    lstData1.Add(d1);
                    lstData.Add(Util.UtilService.addSubParameter(applicationSchema, screen.Table_Name_Tx, 0, 0, lstData1, conditions));
                    ActionClass actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "update", lstData));
                    //actionClass.columnMetadata = jdata;
                }
                dt = Util.UtilService.getData(applicationSchema, "STUDENT_REGISTER_TRAINING_LONGTERM_T", conditions, null, 0, 1);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    Dictionary<string, object> d1 = dt.AsEnumerable().Select(row => dt.Columns.Cast<DataColumn>().ToDictionary(column => column.ColumnName, column => row[column])).FirstOrDefault();
                    d1["ACTIVE_YN"] = 0;
                    conditions.Clear();
                    string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
                    string UserName = Convert.ToString(HttpContext.Current.Session["LOGIN_ID"]);
                    string Session_Key = Convert.ToString(HttpContext.Current.Session["SESSION_KEY"]);
                    AppUrl = AppUrl + "/AddUpdate";
                    List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
                    List<Dictionary<string, object>> lstData1 = new List<Dictionary<string, object>>();
                    lstData1.Add(d1);
                    lstData.Add(Util.UtilService.addSubParameter(applicationSchema, screen.Table_Name_Tx, 0, 0, lstData1, conditions));
                    ActionClass actionClass = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "update", lstData));
                    //actionClass.columnMetadata = jdata;

                    string StudentID = string.Empty;
                    if (HttpContext.Current.Session["STUDENT_ID"] != null)
                    {
                        StudentID = HttpContext.Current.Session["STUDENT_ID"].ToString();
                    }
                    if (Convert.ToInt32(actionClass.StatCode) >= 0 && StudentID != string.Empty)
                    {
                        Util.UtilService.SendEmailAndSMS(StudentID, Convert.ToInt32(frm["si"]));
                    }
                }
            }
            return act;
        }

        public ActionClass beforeStudentReportsUpload(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            if (frm["UNIQUE_REG_ID"] != null && frm["UNIQUE_REG_ID"].ToString() != string.Empty)
            {
                int studentid = Convert.ToInt32(HttpContext.Current.Session["STUDENT_ID"]);
                int id = Convert.ToInt32(frm["UNIQUE_REG_ID"]);
                Dictionary<string, object> conditions = new Dictionary<string, object>();
                conditions.Add("ID", studentid);
                conditions.Add("ACTIVE_YN", 1);
                DataTable dttt = UtilService.getData(UtilService.getApplicationScheme(screen), "STUDENT_T", conditions, null, 0, 100);
                string mobileNumber = string.Empty;
                string emailid = string.Empty;
                if (dttt != null && dttt.Rows != null && dttt.Rows.Count > 0)
                {
                    mobileNumber = Convert.ToString(dttt.Rows[0]["MOBILE_TX"]);
                    emailid = Convert.ToString(dttt.Rows[0]["EMAIL_ID"]);
                }
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@MOBILE_TX", mobileNumber).Replace("@EMAIL_ID", emailid);
                conditions.Clear();
                conditions.Add("ID", id);
                conditions.Add("QID", 55); // for fetching the drop down data 
                JObject jdata = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, UtilService.getApplicationScheme(screen));
                DataTable dtt = new DataTable();
                foreach (JProperty property in jdata.Properties())
                {
                    if (property.Name == "qfetch")
                    {
                        dtt = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                    }
                }
                if (dtt != null && dtt.Rows != null && dtt.Rows.Count > 0)
                {
                    screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@REQUEST_TYPE_ID", Convert.ToString(dtt.Rows[0]["REQUEST_TYPE_ID"]))
                        .Replace("@COMPANY_TYPE_ID", Convert.ToString(dtt.Rows[0]["COMPANY_TYPE_ID"]))
                        .Replace("@COMP_PCS_NAME_TX", Convert.ToString(dtt.Rows[0]["COMP_PCS_NAME_TX"]))
                        .Replace("@CITY_NAME_TX", Convert.ToString(dtt.Rows[0]["CITY_NAME_TX"]));

                    //if (dtt.Rows[0]["TRAINING_COMMENCE_DT"] != null && !Convert.ToString(dtt.Rows[0]["TRAINING_COMMENCE_DT"]).Trim().Equals(""))
                    //    screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@TRAINING_COMMENCE_DT", Convert.ToDateTime(dtt.Rows[0]["TRAINING_COMMENCE_DT"]).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
                    if (dtt.Rows[0]["TRAINING_COMMENCE_DT"] != null && !Convert.ToString(dtt.Rows[0]["TRAINING_COMMENCE_DT"]).Trim().Equals(""))
                        screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@TRAINING_COMMENCE_DT", Convert.ToString(dtt.Rows[0]["TRAINING_COMMENCE_DT"]));

                    //if (dtt.Rows[0]["TRAINING_COMPLETION_DT"] != null && !Convert.ToString(dtt.Rows[0]["TRAINING_COMPLETION_DT"]).Trim().Equals(""))
                    //    screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@TRAINING_END_DT", Convert.ToDateTime(dtt.Rows[0]["TRAINING_COMPLETION_DT"]).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));                    
                    if (dtt.Rows[0]["TRAINING_COMPLETION_DT"] != null && !Convert.ToString(dtt.Rows[0]["TRAINING_COMPLETION_DT"]).Trim().Equals(""))
                        screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@TRAINING_END_DT", Convert.ToString(dtt.Rows[0]["TRAINING_COMPLETION_DT"]));
                    else
                        screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@TRAINING_END_DT", "");
                }
                else
                    screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@TRAINING_END_DT", "");

                frm["DID"] = "7";
                frm["ui"] = frm["UNIQUE_REG_ID"].ToString();
                frm["ID"] = frm["UNIQUE_REG_ID"].ToString();
                HttpContext.Current.Session["stdQty_TRN_ID"] = frm["ui"].ToString();
                frm["s"] = "edit";
            }

            return Util.UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterStudentReportsUpload(int WEB_APP_ID, FormCollection frm)
        {
            frm["s"] = "update";
            frm["DateFormat"] = "dd/MM/yyyy";
            ActionClass act = null;
            if (HttpContext.Current.Session["stdQty_TRN_ID"] != null && HttpContext.Current.Session["stdQty_TRN_ID"].ToString() != string.Empty)
            {
                frm["ui"] = HttpContext.Current.Session["stdQty_TRN_ID"].ToString();
                frm["ID"] = HttpContext.Current.Session["stdQty_TRN_ID"].ToString();
                if (frm["TRAINING_END_DT"] != null && !frm["TRAINING_END_DT"].Trim().Equals(""))
                {
                    /*Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
                    Dictionary<string, object> conditionss = new Dictionary<string, object>();
                    conditionss.Add("ACTIVE_YN", 1);
                    conditionss.Add("ID", Convert.ToInt32(HttpContext.Current.Session["stdQty_TRN_ID"]));
                    DataTable dtt = UtilService.getData(UtilService.getApplicationScheme(screen), "STUDENT_REGISTER_TRAINING_LONGTERM_T", conditionss, null, 0, 1);
                    if (dtt != null && dtt.Rows != null && dtt.Rows.Count > 0 && (dtt.Rows[0]["TRAINING_COMPLETION_DT"] == null || Convert.ToString(dtt.Rows[0]["TRAINING_COMPLETION_DT"]).Equals("")))
                    {
                        dtt.Rows[0]["TRAINING_COMPLETION_DT"] = Convert.ToDateTime(frm["TRAINING_END_DT"]);
                        List<Dictionary<string, object>> ldict =
                                    dtt.AsEnumerable().Select(
                                    row => dtt.Columns.Cast<DataColumn>().ToDictionary(
                                        column => column.ColumnName as string,    // Key
                                        column => row[column] // Value
                                    )
                                ).ToList();
                        UtilService.insertOrUpdate(UtilService.getApplicationScheme(screen), "STUDENT_REGISTER_TRAINING_LONGTERM_T", ldict);
                    }*/
                    frm["TRAINING_COMPLETION_DT"] = frm["TRAINING_END_DT"];// Convert.ToDateTime(frm["TRAINING_END_DT"]);
                }
                if (frm["hLEAVES_TAKEN_NM"] != null && !frm["hLEAVES_TAKEN_NM"].Trim().Equals(""))
                {
                    frm["LEAVES_TAKEN_NM"] = frm["hLEAVES_TAKEN_NM"];
                }
            }
            frm["SUBMIT_DT"] = DateTime.Now.ToString("dd/MM/yyyy");
            act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);

            frm["nextscreen"] = "64";
            return act;
        }

        public ActionClass beforeApplyClearance(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            string compid = "";
            compid = Convert.ToString((screen.ScreenComponents.Where(x => x.Comp_Type_Nm == 27)).ToList()[0].Id);
            return UtilService.searchLoad(WEB_APP_ID, frm, screen.Action_Tx, compid, Convert.ToString(screen.ID));
        }

        public ActionClass afterApplyClearance(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass act = new ActionClass();
            if (frm["REMOVE_NM"] == "1")
            {
                frm["s"] = "update";
                frm.Remove("ID");
                frm.Remove("ACTIVE_YN");
                frm.Add("ID", frm["REMOVE_ID"]);
                frm.Add("ACTIVE_YN", "0");
                string filePath = frm["FILE_PATH_TX"];
                System.IO.File.Delete(filePath);
                frm["nextscreen"] = "228";
                act = HQLayer.afterSubmitforBL(WEB_APP_ID, frm, "STUDENT_CLEARANCE_CERTIFICATE_DOCUMENT_T");
            }
            else
            {
                if (frm["SUBMIT_TYPE"] == "Add")
                {
                    string File_name_tx = string.Empty;
                    string file_path_tx = string.Empty;
                    string FolderName = string.Empty;
                    FolderName = "CLEARANCE\\UPLOADS\\STUDENT_ID_" + Convert.ToString(frm["STUDENT_ID"]) + "\\" + Convert.ToString(frm["REF_ID"]) + "\\" + Convert.ToString(frm["DOCUMENT_TYPE"]);
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
                            frm.Remove("FILE_NAME_TX");
                            frm.Remove("FILE_PATH_TX");
                            frm.Add("FILE_NAME_TX", File_name_tx);
                            frm.Add("FILE_PATH_TX", file_path_tx);
                            HttpContext.Current.Request.Files[0].SaveAs(_FullPath);
                        }
                    }
                    frm["s"] = "insert";
                    frm["nextscreen"] = "228";
                    act = HQLayer.afterSubmitforBL(WEB_APP_ID, frm, "STUDENT_CLEARANCE_CERTIFICATE_DOCUMENT_T");
                }
                else
                {
                    frm["nextscreen"] = "56";
                    act = HQLayer.afterSubmitforBL(WEB_APP_ID, frm, "STUDENT_CLEARANCE_CERTIFICATE");
                }
            }
            return act;
        }

        public ActionClass beforeStudentDashboard(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            int STUDENT_HDR_ID = Convert.ToInt32(HttpContext.Current.Session["STUDENT_HDR_ID"]);
            DataTable dtexmStatus = new DataTable();
            dtexmStatus = UtilService.GetExaminationDetails(STUDENT_HDR_ID);
            HttpContext.Current.Session["Passed_Course"] = 0;

            #region Get Examination Status
            if (dtexmStatus.Rows.Count > 0)
            {
                int passed_course_id = 0;
                StringBuilder sbexm_status = new StringBuilder();
                sbexm_status.Append("<table class='table'>"
                          + "<thead>"
                              + "<tr>"
                                  + "<th>Course Name</th>"
                                     + "<th>Syllabus Name</th>"
                                        + "<th>Module Name</th>"
                                        + "<th>Passed Year</th>"
                                        + "<th>Roll No</th>"
                                           + "<th>Examination Status</th>"
                                       + "</tr>"
                                   + "</thead>"
                                   + "<tbody>");
                for (int i = 0; i < dtexmStatus.Rows.Count; i++)
                {
                    sbexm_status.Append("<tr>"
                        + "<td>" + dtexmStatus.Rows[i]["COURSENAME"] + "</td>"
                        + "<td>" + dtexmStatus.Rows[i]["SYLLABUSNAME"] + "</td>"
                            + "<td>" + dtexmStatus.Rows[i]["MODULENAME"] + "</td>"
                            + "<td>" + dtexmStatus.Rows[i]["PASSEDYEAR"] + "</td>"
                            + "<td>" + dtexmStatus.Rows[i]["ROLLNUMBER"] + "</td>"
                            + "<td>" + dtexmStatus.Rows[i]["EXM_STATUS"] + "</td>"
                        + "</tr>");
                    int.TryParse(Convert.ToString(dtexmStatus.Rows[i]["PASSEDCOURSE"]), out passed_course_id);
                }
                sbexm_status.Append("</tbody></table>");
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("#tblExaminationStatus", sbexm_status.ToString());
                HttpContext.Current.Session["Passed_Course"] = passed_course_id;
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@course_status_hidden_value", Convert.ToString(HttpContext.Current.Session["Passed_Course"]));
            }
            else
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("#tblExaminationStatus", "");
            #endregion

            DataTable dtemsop_eligible = new DataTable();
            dtemsop_eligible = GetEMSOPEligiblity(STUDENT_HDR_ID);
            HttpContext.Current.Session["EMSOP_ELIGIBLE"] = 0;
            int emsop_eligible = 0;
            int cnt = 0;
            if (dtemsop_eligible.Rows.Count > 0)
            {
                for (int i = 0; i < dtemsop_eligible.Rows.Count; i++)
                {
                    int.TryParse(Convert.ToString(dtemsop_eligible.Rows[i]["cnt"]), out cnt);
                }
            }
            if(cnt>0)
            {
                emsop_eligible = 1;
            }
            HttpContext.Current.Session["EMSOP_ELIGIBLE"] = emsop_eligible;

            #region Training Applicable
            int STUDENT_ID = Convert.ToInt32(HttpContext.Current.Session["STUDENT_ID"]);
            Dictionary<string, object> EmailData = new Dictionary<string, object>();
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("QID", 33);
            conditions.Add("ID", STUDENT_ID);

            JObject jdata = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, Util.UtilService.getApplicationScheme(screen));
            DataTable dt_TraApp = new DataTable();
            StringBuilder sb_TraApp = new StringBuilder();
            foreach (JProperty property in jdata.Properties())
            {
                if (property.Name == "qfetch")
                    dt_TraApp = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
            }
            if (dt_TraApp != null && dt_TraApp.Rows != null && dt_TraApp.Rows.Count > 0)
            {
                dt_TraApp.DefaultView.Sort = "ORDER_NM asc";
                dt_TraApp = dt_TraApp.DefaultView.ToTable();

                sb_TraApp.Append("<ul class='training-list'>");
                sb_TraApp.Append("<li class='text-primary'><strong>Training Details</strong></li>");

                int CompletedCount = dt_TraApp.AsEnumerable().Where(myRow => myRow.Field<Int64>("CERTIFICATE_STATUS") == 1).Count();
                int ApplyButton = 0;
                if (CompletedCount > 0)
                {
                    //var ORDER_NM = dt_TraApp.AsEnumerable().Where(myRow => myRow.Field<Int64>("CERTIFICATE_STATUS") == 1 ||
                    //                      myRow.Field<Int64>("CERTIFICATE_STATUS") == 2 || myRow.Field<Int64>("CERTIFICATE_STATUS") == 3)
                    //                     .OrderByDescending(r => r.Field<Int64>("ORDER_NM")).Take(1).Select(x => x.Field<Int64>("ORDER_NM")).SingleOrDefault();

                    var ORDER_NM = dt_TraApp.AsEnumerable().Where(myRow => myRow.Field<Int64>("CERTIFICATE_STATUS") == 0)
                                         .OrderBy(r => r.Field<Int64>("ORDER_NM")).Take(1).Select(x => x.Field<Int64>("ORDER_NM")).SingleOrDefault();

                    //ApplyButton = Convert.ToInt32(ORDER_NM) + 1;
                    ApplyButton = Convert.ToInt32(ORDER_NM);
                }
                else
                {
                    var ORDER_NM = dt_TraApp.AsEnumerable().Where(myRow => myRow.Field<string>("STATUS") == "Apply")
                                        .OrderBy(r => r.Field<Int64>("ORDER_NM")).Take(1).Select(x => x.Field<Int64>("ORDER_NM")).SingleOrDefault();

                    ApplyButton = Convert.ToInt32(ORDER_NM);
                }

                for (int i = 0; i < dt_TraApp.Rows.Count; i++)
                {
                    bool chkflag = true;
                    if(Convert.ToString(dt_TraApp.Rows[i]["TRAINING_NAME_TX"]).Contains("e-MSOP"))
                    {
                        if(emsop_eligible==0)
                        {
                            chkflag = false;
                        }
                    }
                    if (chkflag)
                    {
                        sb_TraApp.Append("<li class='clearfix'><strong>" + dt_TraApp.Rows[i]["TRAINING_NAME_TX"] + "</strong> <span>(" + dt_TraApp.Rows[i]["DURATION"] + ")</span>");

                        if (ApplyButton == Convert.ToInt32(dt_TraApp.Rows[i]["ORDER_NM"]) && Convert.ToInt32(dt_TraApp.Rows[i]["CERTIFICATE_STATUS"]) == 0)//Apply Button
                        {
                            int MenuApply = 0;
                            if (dt_TraApp.Rows[i]["ID"].ToString() == "2" || dt_TraApp.Rows[i]["ID"].ToString() == "4"
                                || dt_TraApp.Rows[i]["ID"].ToString() == "7")
                                MenuApply = 48;
                            else
                                MenuApply = 30;

                            sb_TraApp.Append("<span class='training-apply'><button type='button' class='btn btn-primary btn-xs' onclick='menuAction(" + MenuApply + ")'>Apply Training</button></span>"
                                + "<div class='clearfix'></div>");
                        }
                        else if (CompletedCount > 0 && Convert.ToInt32(dt_TraApp.Rows[i]["CERTIFICATE_STATUS"]) == 1)//Completed Button
                            sb_TraApp.Append("<span class='training-apply green'>Completed</span>");
                        else if (Convert.ToInt32(dt_TraApp.Rows[i]["CERTIFICATE_STATUS"]) == 3)
                            sb_TraApp.Append("<span class='training-apply green'>" + dt_TraApp.Rows[i]["STATUS"] + "</span>");
                        else if (Convert.ToInt32(dt_TraApp.Rows[i]["CERTIFICATE_STATUS"]) == 2)
                            sb_TraApp.Append("<span class='training-apply green'>" + dt_TraApp.Rows[i]["STATUS"] + "</span>");
                        else
                            sb_TraApp.Append("<span class='training-apply green'></span>");
                        sb_TraApp.Append("</li>");
                    }
                }
                sb_TraApp.Append("</ul>");
                long menu_id = DBTable.MENU_T.AsEnumerable().Where(x => x.Field<long>("SCREEN_ID") == 228).Select(x => x.Field<long>("ID")).FirstOrDefault();
                DataTable dt = new DataTable();
                Dictionary<string, object> conditionss = new Dictionary<string, object>();
                conditionss.Add("ACTIVE_YN", 1);
                conditionss.Add("STUDENT_ID", STUDENT_ID);
                conditionss.Add("APPROVE_NM", 1);
                dt = UtilService.getData("Training", "STUDENT_CLEARANCE_CERTIFICATE", conditionss, null, 0, 1);
                if (dt != null && dt.Rows.Count > 0)
                {
                    sb_TraApp.Append("<ul class='training-list'><li class='clearfix' style='border-top:#ccc 1px dotted;'><strong>Apply For Training Clearance Certificate (TCC)</strong><span class='training-apply green'>Approved</span><br><span style='float:right;'><button type='button' class='btn btn-primary btn-xs' onclick='menuAction(" + menu_id + ")'>Download</button></span></li></ul>");
                }
                else
                {
                    dt = null;
                    conditionss = new Dictionary<string, object>();
                    conditionss.Add("ACTIVE_YN", 1);
                    conditionss.Add("STUDENT_ID", STUDENT_ID);
                    dt = UtilService.getData("Training", "STUDENT_CLEARANCE_CERTIFICATE", conditionss, null, 0, 1);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        sb_TraApp.Append("<ul class='training-list'><li class='clearfix' style='border-top:#ccc 1px dotted;'><strong>Apply For Training Clearance Certificate (TCC)</strong><span class='training-apply green'>Under Process</span><br><span style='float:right;'><button type='button' class='btn btn-primary btn-xs' onclick='menuAction(" + menu_id + ")'>Check Application</button></span></li></ul>");
                    }
                    else
                    {
                        dt = null;
                        conditionss = new Dictionary<string, object>();
                        conditionss.Add("ACTIVE_YN", 1);
                        conditionss.Add("STUDENT_ID", STUDENT_ID);
                        dt = UtilService.getData("Training", "STUDENT_CLEARANCE_CERTIFICATE_T", conditionss, null, 0, 1);
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            sb_TraApp.Append("<ul class='training-list'><li class='clearfix' style='border-top:#ccc 1px dotted;'><strong>Apply For Training Clearance Certificate (TCC)</strong><span class='training-apply'><button type='button' class='btn btn-primary btn-xs' onclick='menuAction(" + menu_id + ")'>Apply TCC</button></span></li></ul>");
                        }
                    }
                }
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("#tblTrainingApp", sb_TraApp.ToString());
            }
            else
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("#tblTrainingApp", "");
            #endregion

            return null;
        }

        #region Old Apply long term code
        //public ActionClass beforeNewTrainingReg(int WEB_APP_ID, FormCollection frm, Screen_T scr)
        //{
        //    return UtilService.beforeLoad(WEB_APP_ID, frm);
        //}

        ////For sending Email and SMS.
        //public ActionClass afterNewTrainingReg(int WEB_APP_ID, FormCollection frm)
        //{
        //    ActionClass actionClass = new ActionClass();
        //    actionClass = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
        //    string StudentID = string.Empty;
        //    if (HttpContext.Current.Session["STUDENT_ID"] != null)
        //    {
        //        StudentID = HttpContext.Current.Session["STUDENT_ID"].ToString();
        //    }
        //    if (Convert.ToInt32(actionClass.StatCode) >= 0 && StudentID != string.Empty && frm["REQUEST_TYPE_ID"] != null && frm["REQUEST_TYPE_ID"].ToString() != "4")
        //    {
        //        Util.UtilService.SendEmailAndSMS(StudentID, Convert.ToInt32(frm["si"]));
        //    }
        //    return actionClass;
        //}
        #endregion

        #region Old Apply exemption code
        //public ActionClass beforeApplyExemption(int WEB_APP_ID, FormCollection frm, Screen_T scr)
        //{
        //    return UtilService.beforeLoad(WEB_APP_ID, frm);
        //}

        //public ActionClass afterApplyExemption(int WEB_APP_ID, FormCollection frm)
        //{
        //    ActionClass actionClass = new ActionClass();
        //    actionClass = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
        //    string StudentID = string.Empty;
        //    if (HttpContext.Current.Session["STUDENT_ID"] != null)
        //    {
        //        StudentID = HttpContext.Current.Session["STUDENT_ID"].ToString();
        //    }
        //    if (Convert.ToInt32(actionClass.StatCode) >= 0 && StudentID != string.Empty && frm["EXEMPTION_FEE"] != null
        //        && (Convert.ToDouble(frm["EXEMPTION_FEE"]) == 0 || frm["EXEMPTION_FEE"].ToString() == string.Empty))
        //    {
        //        Util.UtilService.SendEmailAndSMS(StudentID, Convert.ToInt32(frm["si"]));
        //    }
        //    return actionClass;
        //}
        #endregion

        #region Old Apply Balance Training in Long term
        //public ActionClass beforeBalanceTrainingReg(int WEB_APP_ID, FormCollection frm, Screen_T scr)
        //{
        //    return UtilService.beforeLoad(WEB_APP_ID, frm);
        //}

        //public ActionClass afterBalanceTrainingReg(int WEB_APP_ID, FormCollection frm)
        //{
        //    ActionClass actionClass = new ActionClass();
        //    actionClass = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
        //    string StudentID = string.Empty;
        //    if (HttpContext.Current.Session["STUDENT_ID"] != null)
        //    {
        //        StudentID = HttpContext.Current.Session["STUDENT_ID"].ToString();
        //    }
        //    if (Convert.ToInt32(actionClass.StatCode) >= 0 && StudentID != string.Empty)
        //    {
        //        Util.UtilService.SendEmailAndSMS(StudentID, Convert.ToInt32(frm["si"]));
        //    }
        //    return actionClass;
        //}
        #endregion

        public ActionClass searchStudentTrainingLogic(int WEB_APP_ID, FormCollection frm, string ScreenType, string sid, string screenId = "", Screen_T screen = null)
        {
            int STUDENT_IDD = Convert.ToInt32(HttpContext.Current.Session["STUDENT_ID"]);
            int emsop_elig = Convert.ToInt32(HttpContext.Current.Session["EMSOP_ELIGIBLE"]);

            DataTable dtemsop_eligible = new DataTable();
            dtemsop_eligible = GetStudentStructure(STUDENT_IDD);            
            
            int strid = 0;
            if (dtemsop_eligible.Rows.Count > 0)
            {
                for (int i = 0; i < dtemsop_eligible.Rows.Count; i++)
                {
                    int.TryParse(Convert.ToString(dtemsop_eligible.Rows[i]["STR_ID"]), out strid);
                }
            }
            if(strid==1 || strid==2)
            {
                if(emsop_elig==0)
                {
                    frm.Remove("SCRH_TRAINING_NAME_TX");
                    frm.Remove("COND_TRAINING_NAME_TX");
                    frm.Add("COND_TRAINING_NAME_TX", "AND !=");
                    if (strid == 1)
                    {
                        frm.Add("SCRH_TRAINING_NAME_TX", "e-MSOP (Earlier)");
                    }
                    else
                    {
                        frm.Add("SCRH_TRAINING_NAME_TX", "e-MSOP(Modified)");
                    }
                }
            }
            frm.Remove("SCRH_COURSE_ID");
            frm.Remove("COND_COURSE_ID");
            frm.Add("COND_COURSE_ID", "AND <=");
            string Passed_Course = Convert.ToString(HttpContext.Current.Session["Passed_Course"]);
            if (!string.IsNullOrEmpty(Passed_Course) && Passed_Course == "0")
                Passed_Course = "-1";
            frm.Add("SCRH_COURSE_ID", Passed_Course);
            return Util.UtilService.searchLoad(WEB_APP_ID, frm, ScreenType, Convert.ToString(sid));
        }

        public static DataTable GetEMSOPEligiblity(int STUDENT_HDR_ID)
        {
            string MethodName = "qfetch";
            Dictionary<string, object> EmailData = new Dictionary<string, object>();
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("QID", 245);
            conditions.Add("HDR_ID", STUDENT_HDR_ID);

            JObject jdata = DBTable.GetData(MethodName, conditions, "SCREEN_COMP_T", 0, 100, "ICSI");
            DataTable dtexm = new DataTable();
            foreach (JProperty property in jdata.Properties())
            {
                if (property.Name == "qfetch")
                    dtexm = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
            }

            return dtexm;
        }

        public static DataTable GetStudentStructure(int STUDENT_ID)
        {
            string MethodName = "qfetch";
            Dictionary<string, object> EmailData = new Dictionary<string, object>();
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("QID", 246);
            conditions.Add("STD_ID", STUDENT_ID);

            JObject jdata = DBTable.GetData(MethodName, conditions, "SCREEN_COMP_T", 0, 100, "Training");
            DataTable dtexm = new DataTable();
            foreach (JProperty property in jdata.Properties())
            {
                if (property.Name == "qfetch")
                    dtexm = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
            }

            return dtexm;
        }

        public ActionClass beforeSECEducation(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            string compid = "";
            compid = Convert.ToString((screen.ScreenComponents.Where(x => x.Comp_Type_Nm == 27)).ToList()[0].Id);
            return UtilService.searchLoad(WEB_APP_ID, frm, screen.Action_Tx, compid, Convert.ToString(screen.ID));
        }

        public ActionClass afterSECEducation(int WEB_APP_ID, FormCollection frm)
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
                FolderName = "MEMBERSHIP\\SEC\\UPLOADS\\STUDENT_ID_" + Convert.ToString(frm["STUDENT_ID"]) + "\\" + Convert.ToString(frm["SEC_REG_ID"]) + "\\" + Convert.ToString(frm["QUALIFICATION_ID"]);
                if (ConfigurationManager.AppSettings.AllKeys.Contains("GLOBAL_DOCUMENT_ROOT"))
                {
                    FolderName = Convert.ToString(ConfigurationManager.AppSettings["GLOBAL_DOCUMENT_ROOT"]) + FolderName;
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

        public ActionClass beforeSECDocument(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            string compid = "";
            compid = Convert.ToString((screen.ScreenComponents.Where(x => x.Comp_Type_Nm == 27)).ToList()[0].Id);
            return UtilService.searchLoad(WEB_APP_ID, frm, screen.Action_Tx, compid, Convert.ToString(screen.ID));
        }

        public ActionClass afterSECDocument(int WEB_APP_ID, FormCollection frm)
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
                FolderName = "MEMBERSHIP\\SEC\\UPLOADS\\DOCUMENTS\\STUDENT_ID_" + Convert.ToString(frm["STUDENT_ID"]) + "\\" + Convert.ToString(frm["SEC_REG_ID"]) + "\\" + Convert.ToString(frm["ID"]);
                if (ConfigurationManager.AppSettings.AllKeys.Contains("GLOBAL_DOCUMENT_ROOT"))
                {
                    FolderName = Convert.ToString(ConfigurationManager.AppSettings["GLOBAL_DOCUMENT_ROOT"]) + FolderName;
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

        public ActionClass CreateStudentExam(int WEB_APP_ID, FormCollection frm, ref string MasterExamID)
        {
            ActionClass actionClass = null;
            string StudentId = frm["u"];
            string ExamId = frm["EXAM_ID"];



            //           [EXAM_DATE]  [datetime] NOT NULL,
            //[TOTAL_MARKS_OBTAINED_NM]  DECIMAL(18, 2) NOT NULL,         
            // [PASS_YN] [bit] NULL,
            string date = DateTime.Now.ToString();
            string total = "0";
            string passyn = "0";

            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]) + "/AddUpdate";
            string UserName = HttpContext.Current.Session["LOGIN_ID"].ToString();
            string Session_Key = HttpContext.Current.Session["SESSION_KEY"].ToString();
            //insert master
            List<Dictionary<string, object>> masterlist = new List<Dictionary<string, object>>();
            Dictionary<string, object> tempd = new Dictionary<string, object>
            {
                ["STUDENT_ID"] = StudentId,
                ["EXAM_ID"] = ExamId,
                ["EXAM_DATE"] = date,
                ["TOTAL_MARKS_OBTAINED_NM"] = "0",
                ["PASS_YN"] = "false"
            };
            int Id = 0;
            masterlist.Add(tempd);
            actionClass = UtilService.insertOrUpdate("Training", "STUDENT_EXAM_MASTER_T", masterlist);
            if (actionClass.StatMessage.ToLower() == "success" && actionClass.DecryptData != null && !actionClass.DecryptData.Trim().Equals(""))
            {
                if (!string.IsNullOrEmpty(actionClass.DecryptData))
                {
                    JObject res = JObject.Parse(actionClass.DecryptData);
                    foreach (JProperty jproperty in res.Properties())
                    {
                        if (jproperty.Name != null)
                        {
                            DataTable dtdata = JsonConvert.DeserializeObject<DataTable>(jproperty.Value.ToString());
                            Id = Convert.ToInt32(dtdata.Rows[0]["ID"]);
                        }
                    }
                }
            }
            MasterExamID = Id.ToString();//get the master id
            int UID = 0;




            DataTable dtRDetails = new DataTable();
            //Screen_T s = UtilService.GetScreenData(WEB_APP_ID, null, frm["screenid"], frm);

            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("ID", ExamId); //check records available or not     
                                          //conditions.Add("USER_ID", Convert.ToString(HttpContext.Current.Session["USER_ID"]));

            dtRDetails = UtilService.getData("Training", "OBJECTIVE_EXAM_T", conditions, null, 0, 1);

            string QUESTION_IDS = dtRDetails.Rows[0]["QUESTION_IDS"].ToString();
            var randomOrdering = QUESTION_IDS.Split(',').OrderBy(o => Guid.NewGuid()).ToList();
            for (int i = 0; i < randomOrdering.Count; i++)
            {
                List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();

                Dictionary<string, object> d = new Dictionary<string, object>
                {
                    ["STUDENT_EXAM_MASTER_ID"] = MasterExamID,
                    ["QUESTION_ID"] = randomOrdering[i],
                    ["SELECTED_OPTION_ID"] = "0",
                    ["OPTION_CORRECT_YN"] = "false"
                };

                list.Add(d);

                actionClass = UtilService.insertOrUpdate("Training", "STUDENT_EXAM_DETAILS_T", list);
            }


            return actionClass;
        }


        public ActionClass SubmitExam(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass actionClass = null;
            string StudentId = frm["u"];
            string ExamId = frm["EXAM_ID"];
            //string ExamMasterId = frm["STUDENT_EXAM_MASTER_ID"];
            List<exam> mylist = new List<exam>();
            string json = @"" + frm["MyList"].ToString();
            mylist = JsonConvert.DeserializeObject<List<exam>>(json);

            //            [QUESTION_ID]  [int] NOT NULL,
            //[SELECTED_OPTION_ID]  [int] NOT NULL,
            //[OPTION_CORRECT_YN] [bit] NULL,
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]) + "/AddUpdate";
            string UserName = HttpContext.Current.Session["LOGIN_ID"].ToString();
            string Session_Key = HttpContext.Current.Session["SESSION_KEY"].ToString();


            int UID = 0;


            DataTable dtRDetails = new DataTable();
            //Screen_T s = UtilService.GetScreenData(WEB_APP_ID, null, frm["screenid"], frm);

            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("STUDENT_EXAM_MASTER_ID", ExamId); //check records available or not     
                                                              //conditions.Add("USER_ID", Convert.ToString(HttpContext.Current.Session["USER_ID"]));

            dtRDetails = UtilService.getData("Training", "STUDENT_EXAM_DETAILS_T", conditions, null, 0, 100);



            for (int i = 0; i < dtRDetails.Rows.Count; i++)
            {
                List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
                int answer = 0;
                DataTable dtRDetails2 = new DataTable();
                Dictionary<string, object> conditions2 = new Dictionary<string, object>();
                conditions2.Add("ID", dtRDetails.Rows[i]["QUESTION_ID"]);
                dtRDetails2 = UtilService.getData("Training", "OBJECTIVE_QUESTIONS_T", conditions2, null, 0, 1);
                string correctanswer = dtRDetails2.Rows[0]["ANSWER_NM"].ToString();
                foreach (var l in mylist)
                {
                    if (l.question == Convert.ToInt32(dtRDetails.Rows[i]["QUESTION_ID"]))
                    {
                        answer = l.answer;
                        break; //Stop this loop, we found it!
                    }
                }
                Dictionary<string, object> d = new Dictionary<string, object>
                {
                    ["ID"] = dtRDetails.Rows[i]["ID"].ToString(),
                    ["SELECTED_OPTION_ID"] = answer.ToString(),
                    ["OPTION_CORRECT_YN"] = (correctanswer == answer.ToString()).ToString()
                };

                list.Add(d);

                actionClass = UtilService.insertOrUpdate("Training", "STUDENT_EXAM_DETAILS_T", list);
            }

            //#region For Email
            //if (s.is_Email_yn == true)
            //    Util.UtilService.storeEmailData(actionClass, s, "update", AppUrl, Session_Key, UserName, UID);
            //#endregion

            return actionClass;
        }


        public ActionClass UpdateExamResult(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass actionClass = null;
            string StudentId = frm["u"];
            string ExamId = frm["EXAM_ID"];
            string totalmarks = frm["TOTAL_MARKS_OBTAINED_NM"];
            string isPass = frm["PASS_YN"];
            //string ExamMasterId = frm["STUDENT_EXAM_MASTER_ID"];



            //            [QUESTION_ID]  [int] NOT NULL,
            //[SELECTED_OPTION_ID]  [int] NOT NULL,
            //[OPTION_CORRECT_YN] [bit] NULL,
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]) + "/AddUpdate";
            string UserName = HttpContext.Current.Session["LOGIN_ID"].ToString();
            string Session_Key = HttpContext.Current.Session["SESSION_KEY"].ToString();


            int UID = 0;


            DataTable dtRDetails = new DataTable();
            //Screen_T s = UtilService.GetScreenData(WEB_APP_ID, null, frm["screenid"], frm);

            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("STUDENT_EXAM_MASTER_ID", ExamId); //check records available or not     
                                                              //conditions.Add("USER_ID", Convert.ToString(HttpContext.Current.Session["USER_ID"]));

            dtRDetails = UtilService.getData("Training", "STUDENT_EXAM_MASTER_T", conditions, null, 0, 100);




            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();

            Dictionary<string, object> d = new Dictionary<string, object>
            {
                ["ID"] = ExamId,
                ["TOTAL_MARKS_OBTAINED_NM"] = totalmarks,
                ["PASS_YN"] = isPass
            };

            list.Add(d);

            actionClass = UtilService.insertOrUpdate("Training", "STUDENT_EXAM_MASTER_T", list);




            return actionClass;
        }
        //ApplyforCoaching
        public ActionClass ApplyforCoaching(int WEB_APP_ID, FormCollection frm, ref int AppliedCoachingId, ref int paymentStatusId)
        {
            ActionClass actionClass = null;
            string StudentId = frm["u"];
            string batchid = frm["BATCH_ID"];





            //            [QUESTION_ID]  [int] NOT NULL,
            //[SELECTED_OPTION_ID]  [int] NOT NULL,
            //[OPTION_CORRECT_YN] [bit] NULL,
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]) + "/AddUpdate";
            string UserName = HttpContext.Current.Session["LOGIN_ID"].ToString();
            string Session_Key = HttpContext.Current.Session["SESSION_KEY"].ToString();


            int UID = 0;

            DataTable dtRDetails2 = new DataTable();
            Dictionary<string, object> conditions2 = new Dictionary<string, object>();
            conditions2.Add("ID", batchid);
            dtRDetails2 = UtilService.getData("Training", "COACHING_BATCH_T", conditions2, null, 0, 1);
            //string correctanswer = dtRDetails2.Rows[0]["ANSWER_NM"].ToString();

            DataTable dtRDetails = new DataTable();
            //Screen_T s = UtilService.GetScreenData(WEB_APP_ID, null, frm["screenid"], frm);

            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("STUDENT_ID", StudentId); //check records available or not     
            conditions.Add("COACHING_ID", dtRDetails2.Rows[0]["COACHING_ID"].ToString());                                                 //conditions.Add("USER_ID", Convert.ToString(HttpContext.Current.Session["USER_ID"]));
            conditions.Add("BATCH_ID", batchid);

            dtRDetails = UtilService.getData("Training", "STUDENT_COACHING_APPLICATIONS_T", conditions, null, 0, 100);

            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();

            if (dtRDetails.Rows.Count == 1)
            {
                Dictionary<string, object> d = new Dictionary<string, object>
                {
                    ["ID"] = dtRDetails.Rows[0]["ID"].ToString(),
                    ["STUDENT_ID"] = StudentId,
                    ["COACHING_ID"] = dtRDetails2.Rows[0]["COACHING_ID"].ToString(),
                    ["BATCH_ID"] = batchid,
                    ["SUBJECT_OR_MODULE_YN"] = dtRDetails2.Rows[0]["MODULE_OR_SUBJECT_YN"].ToString(),
                    ["SUBJECT_ID"] = dtRDetails2.Rows[0]["SUBJECT_ID"].ToString(),
                    ["MODULE_ID"] = dtRDetails2.Rows[0]["MODULE_ID"].ToString(),
                    ["FINANCIAL_YEAR_ID"] = 2
                };

                list.Add(d);

                actionClass = UtilService.insertOrUpdate("Training", "STUDENT_COACHING_APPLICATIONS_T", list);

                if (actionClass.StatMessage.ToLower() == "success" && actionClass.DecryptData != null && !actionClass.DecryptData.Trim().Equals(""))
                {
                    if (!string.IsNullOrEmpty(actionClass.DecryptData))
                    {
                        JObject res = JObject.Parse(actionClass.DecryptData);
                        foreach (JProperty jproperty in res.Properties())
                        {
                            if (jproperty.Name != null)
                            {
                                DataTable dtdata = JsonConvert.DeserializeObject<DataTable>(jproperty.Value.ToString());
                                AppliedCoachingId = Convert.ToInt32(dtdata.Rows[0]["ID"]);
                                int statusid;
                                int.TryParse(dtdata.Rows[0]["PAYMENT_STATUS_ID"].ToString(), out statusid);
                                paymentStatusId = Convert.ToInt32(statusid);
                            }
                        }
                    }
                }
            }
            else if (dtRDetails.Rows.Count > 1)
            {

            }
            else
            {



                Dictionary<string, object> d = new Dictionary<string, object>
                {
                    ["STUDENT_ID"] = StudentId,
                    ["COACHING_ID"] = dtRDetails2.Rows[0]["COACHING_ID"].ToString(),
                    ["BATCH_ID"] = batchid,
                    ["SUBJECT_OR_MODULE_YN"] = dtRDetails2.Rows[0]["MODULE_OR_SUBJECT_YN"].ToString(),
                    ["SUBJECT_ID"] = dtRDetails2.Rows[0]["SUBJECT_ID"].ToString(),
                    ["MODULE_ID"] = dtRDetails2.Rows[0]["MODULE_ID"].ToString(),
                    ["FINANCIAL_YEAR_ID"] = 2,
                    ["PAYMENT_STATUS_ID"] = 4
                };

                list.Add(d);

                actionClass = UtilService.insertOrUpdate("Training", "STUDENT_COACHING_APPLICATIONS_T", list);

                if (actionClass.StatMessage.ToLower() == "success" && actionClass.DecryptData != null && !actionClass.DecryptData.Trim().Equals(""))
                {
                    if (!string.IsNullOrEmpty(actionClass.DecryptData))
                    {
                        JObject res = JObject.Parse(actionClass.DecryptData);
                        foreach (JProperty jproperty in res.Properties())
                        {
                            if (jproperty.Name != null)
                            {
                                DataTable dtdata = JsonConvert.DeserializeObject<DataTable>(jproperty.Value.ToString());
                                AppliedCoachingId = Convert.ToInt32(dtdata.Rows[0]["ID"]);
                                int statusid;
                                int.TryParse(dtdata.Rows[0]["PAYMENT_STATUS_ID"].ToString(), out statusid);
                                paymentStatusId = Convert.ToInt32(statusid);
                            }
                        }
                    }
                }

            }


            return actionClass;
        }




        public ActionClass beforeACSRegInstruction(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            ActionClass actionClass = null;
            int StudentID = Convert.ToInt32(HttpContext.Current.Session["STUDENT_ID"]);
            int STUDENT_HDR_ID = Convert.ToInt32(HttpContext.Current.Session["STUDENT_HDR_ID"]);

            Dictionary<string, object> conditions = new Dictionary<string, object>();
            conditions.Add("QID", 166); //check records available or not     
            conditions.Add("STUDENT_ID", StudentID);
            JObject jdata = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, Util.UtilService.getApplicationScheme(screen));
            DataTable dtMembershipReg = new DataTable();

            if (jdata == null || jdata.First.First.First == null)
            {
                conditions = new Dictionary<string, object>();
                conditions.Add("QID", 165);
                conditions.Add("StudentHdrID", STUDENT_HDR_ID);
                jdata = DBTable.GetData("qfetch", conditions, "SCREEN_COMP_T", 0, 100, "ICSI");

                foreach (JProperty property in jdata.Properties())
                {
                    if (property.Name == "qfetch")
                        dtMembershipReg = JsonConvert.DeserializeObject<DataTable>(property.Value.ToString());
                }

                if (dtMembershipReg != null && dtMembershipReg.Rows != null && dtMembershipReg.Rows.Count > 0)
                {
                    List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
                    Dictionary<string, object> d = new Dictionary<string, object>();
                    d["STUDENT_ID"] = StudentID;
                    d["FIRST_NAME_TX"] = dtMembershipReg.Rows[0]["FIRSTNAME"];
                    d["MIDDLE_NAME_TX"] = dtMembershipReg.Rows[0]["MIDDLENAME"];
                    d["LAST_NAME_TX"] = dtMembershipReg.Rows[0]["LASTNAME"];
                    d["GENDER_NM"] = dtMembershipReg.Rows[0]["GENDER"];
                    d["FATHER_HUSB_NAME_TX"] = dtMembershipReg.Rows[0]["FatherSpouseName"];
                    d["MOBILE_NO_TX "] = dtMembershipReg.Rows[0]["MOBILE"];
                    d["EMAIL_ID_TX"] = dtMembershipReg.Rows[0]["EMAILID"];
                    d["HANDICAPPED_YN"] = false;
                    d["NATIONALITY_ID"] = 93;
                    d["CITIZENSHIP_ID"] = 93;
                    d["INDIAN_CITIZEN_NM"] = 2;
                    d["CORRESP_ADDRESS_NM"] = 1;
                    d["SUBSC_JOURNAL_YN"] = false;
                    d["CHAPTER_TX"] = dtMembershipReg.Rows[0]["OfficeName"];

                    for (int i = 0; i < dtMembershipReg.Rows.Count; i++)
                    {
                        if (Convert.ToString(dtMembershipReg.Rows[i]["AdrType"]) == "PERM")
                        {
                            d["RES_ADD_LINE1_TX"] = dtMembershipReg.Rows[i]["AdrLine1"];
                            d["RES_ADD_LINE2_TX"] = dtMembershipReg.Rows[i]["AdrLine2"];
                            d["RES_ADD_LINE3_TX"] = dtMembershipReg.Rows[i]["AdrLine3"];
                            d["RES_CITY_ID"] = dtMembershipReg.Rows[i]["CityFKID"];
                            d["RES_PINCODE_TX"] = dtMembershipReg.Rows[i]["PinCode"];
                        }
                        else if (Convert.ToString(dtMembershipReg.Rows[i]["AdrType"]) == "CORS")
                        {
                            d["PROF_ADD_LINE1_TX"] = dtMembershipReg.Rows[i]["AdrLine1"];
                            d["PROF_ADD_LINE2_TX"] = dtMembershipReg.Rows[i]["AdrLine2"];
                            d["PROF_ADD_LINE3_TX"] = dtMembershipReg.Rows[i]["AdrLine3"];
                            d["PROF_CITY_ID"] = dtMembershipReg.Rows[i]["CityFKID"];
                            d["PROF_PINCODE_TX"] = dtMembershipReg.Rows[i]["PinCode"];
                        }
                    }

                    list.Add(d);
                    actionClass = UtilService.insertOrUpdate(Util.UtilService.getApplicationScheme(screen), "MEM_ACS_MEMBERSHIP_REG_T", list);
                }
            }

            return Util.UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass beforeACSEducation(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            string compid = "";
            compid = Convert.ToString((screen.ScreenComponents.Where(x => x.Comp_Type_Nm == 27)).ToList()[0].Id);
            return UtilService.searchLoad(WEB_APP_ID, frm, screen.Action_Tx, compid, Convert.ToString(screen.ID));
        }

        public ActionClass afterACSEducation(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass act = null;
            if (frm["REMOVE_NM"] == "1")
            {
                frm["s"] = "update";
                frm.Add("ID", frm["REMOVE_ID"]);
                frm.Add("ACTIVE_YN", "0");
                string filePath = frm["PATH_TX"];
                System.IO.File.Delete(filePath);

                List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
                Dictionary<string, object> d = new Dictionary<string, object>();
                d["ID"] = frm["REMOVE_ID"];
                d["ACTIVE_YN"] = "0";
                d["REMOVE_NM"] = frm["REMOVE_NM"];

                list.Add(d);
                act = UtilService.insertOrUpdate("Training", "MEM_ACS_REG_EDUCATION_DTL_T", list);
            }
            else
            {
                string File_name_tx = string.Empty;
                string file_path_tx = string.Empty;
                string FolderName = string.Empty;
                FolderName = "MEMBERSHIP\\ACS\\UPLOADS\\STUDENT_ID_" + Convert.ToString(frm["STUDENT_ID"]) + "\\" + Convert.ToString(frm["ACS_REG_ID"]) + "\\" + Convert.ToString(frm["QUALIFICATION_ID"]);
                if (ConfigurationManager.AppSettings.AllKeys.Contains("GLOBAL_DOCUMENT_ROOT"))
                    FolderName = Convert.ToString(ConfigurationManager.AppSettings["GLOBAL_DOCUMENT_ROOT"]) + FolderName;
                else
                    FolderName = AppDomain.CurrentDomain.BaseDirectory + FolderName;

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
                act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
            }
            return act;
        }

        public ActionClass beforeACSForeignCourse(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            string compid = "";
            compid = Convert.ToString((screen.ScreenComponents.Where(x => x.Comp_Type_Nm == 27)).ToList()[0].Id);
            return UtilService.searchLoad(WEB_APP_ID, frm, screen.Action_Tx, compid, Convert.ToString(screen.ID));
        }

        public ActionClass afterACSForeignCourse(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass act = new ActionClass();
            act.StatCode = "0";
            act.StatMessage = "success";
            string File_name_tx = string.Empty;
            string file_path_tx = string.Empty;
            string FolderName = string.Empty;

            if (!string.IsNullOrEmpty(frm["ID"]))
                frm["s"] = "update";

            if (!string.IsNullOrEmpty(Convert.ToString(frm["FOREIGN_BODY_NAME_TX"])) &&
                !string.IsNullOrEmpty(HttpContext.Current.Request.Files[0].FileName))
            {
                FolderName = "MEMBERSHIP\\ACS\\UPLOADS\\STUDENT_ID_" + Convert.ToString(frm["STUDENT_ID"]) + "\\" + Convert.ToString(frm["ACS_REG_ID"]);
                if (ConfigurationManager.AppSettings.AllKeys.Contains("GLOBAL_DOCUMENT_ROOT"))
                    FolderName = Convert.ToString(ConfigurationManager.AppSettings["GLOBAL_DOCUMENT_ROOT"]) + FolderName;
                else
                    FolderName = AppDomain.CurrentDomain.BaseDirectory + FolderName;

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

            if (!string.IsNullOrEmpty(Convert.ToString(frm["FOREIGN_BODY_NAME_TX"])))
                act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);

            return act;
        }

        public ActionClass beforeACSRegDocuments(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            string compid = "";
            compid = Convert.ToString((screen.ScreenComponents.Where(x => x.Comp_Type_Nm == 27)).ToList()[0].Id);
            return UtilService.searchLoad(WEB_APP_ID, frm, screen.Action_Tx, compid, Convert.ToString(screen.ID));
        }

        public ActionClass afterACSRegDocuments(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass act = null;
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            Dictionary<string, object> d;
            if (!string.IsNullOrEmpty(Convert.ToString(frm["NAME_TX_1"])))
            {
                for (int i = 1; i <= 5; i++)
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(frm["NAME_TX_" + i + ""])))
                    {
                        d = new Dictionary<string, object>();
                        int Id = 0;
                        if (!string.IsNullOrEmpty(Convert.ToString(frm["DEP-BEN_ID_" + i + ""])))
                            Id = Convert.ToInt32(frm["DEP-BEN_ID_" + i + ""]);
                        string Name = Convert.ToString(frm["NAME_TX_" + i + ""]);
                        string Age = Convert.ToString(frm["AGE_TX_" + i + ""]);

                        string RELATION_TO_SUB = Convert.ToString(frm["RELATION_TO_SUB_TX_" + i + ""]);
                        string EMAIL_ID = Convert.ToString(frm["EMAIL_TX_" + i + ""]);
                        string PHONE_NUMBER = Convert.ToString(frm["PHONE_TX_" + i + ""]);
                        string ADDRESS = Convert.ToString(frm["ADDRESS_TX_" + i + ""]);
                        int ACS_REG_ID = Convert.ToInt32(frm["ACS_REG_ID"]);

                        if (Id != 0)
                            d["ID"] = Id;
                        d["ACS_REG_ID"] = ACS_REG_ID;
                        d["NAME_TX"] = Name;
                        d["AGE_TX"] = Age;
                        d["RELATION_TO_SUB_TX"] = RELATION_TO_SUB;
                        d["EMAIL_TX"] = EMAIL_ID;
                        d["PHONE_TX"] = PHONE_NUMBER;
                        d["ADDRESS_TX"] = ADDRESS;
                        d["PURPOSE_TX"] = "ACS Registeration";

                        list.Add(d);
                    }
                }

                if (list.Count > 0)
                    act = UtilService.insertOrUpdate("Training", "MEM_ACS_DEP_BENEIFICIARY_TX", list);
            }
            else
            {
                if (frm["REMOVE_NM"] == "1")
                {
                    d = new Dictionary<string, object>();
                    string filePath = frm["PATH_TX"];
                    System.IO.File.Delete(filePath);

                    d["ID"] = frm["REMOVE_ID"];
                    d["ACTIVE_YN"] = "0";
                    d["REMOVE_NM"] = frm["REMOVE_NM"];

                    list.Add(d);
                    act = UtilService.insertOrUpdate("Training", "MEM_ACS_REG_DOCUMENTS_T", list);
                }
                else if (HttpContext.Current.Request.Files[0].FileName.Length > 0)
                {
                    string File_name_tx = string.Empty;
                    string file_path_tx = string.Empty;
                    string FolderName = string.Empty;
                    FolderName = "MEMBERSHIP\\ACS\\UPLOADS\\STUDENT_ID_" + Convert.ToString(frm["STUDENT_ID"]) + "\\" + Convert.ToString(frm["ACS_REG_ID"]) + "\\" + "RegDocuments";
                    if (ConfigurationManager.AppSettings.AllKeys.Contains("GLOBAL_DOCUMENT_ROOT"))
                        FolderName = Convert.ToString(ConfigurationManager.AppSettings["GLOBAL_DOCUMENT_ROOT"]) + FolderName;
                    else
                        FolderName = AppDomain.CurrentDomain.BaseDirectory + FolderName;

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
                    int count = 1;
                    if (!string.IsNullOrEmpty(frm["FITNESS_CERT_MEM2"]))
                        count = 2;

                    for (int i = 0; i < count; i++)
                    {
                        d = new Dictionary<string, object>();
                        string MemberNo = frm["FITNESS_CERT_MEMBER_TX"];
                        if (count == 2 && i == 1)
                            MemberNo = frm["FITNESS_CERT_MEM2"];

                        d["ACS_REG_ID"] = frm["ACS_REG_ID"];
                        d["DOCUMENT_TYPE_ID"] = frm["DOCUMENT_TYPE_ID"];
                        d["FITNESS_CERT_MEMBER_TX"] = MemberNo;
                        d["ACS_REG_ID"] = frm["ACS_REG_ID"];
                        d["DOCUMENT_TYPE_ID"] = frm["DOCUMENT_TYPE_ID"];
                        d["FILE_NAME_TX"] = frm["FILE_NAME_TX"];
                        d["FILE_PATH_TX"] = frm["FILE_PATH_TX"];
                        d["REMOVE_NM"] = 0;
                        d["APPROVE_NM"] = 0;
                        list.Add(d);
                    }

                    act = UtilService.insertOrUpdate("Training", "MEM_ACS_REG_DOCUMENTS_T", list);
                }
            }
            return act;
        }

        public ActionClass beforeFCSDocument(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            string compid = "";
            compid = Convert.ToString((screen.ScreenComponents.Where(x => x.Comp_Type_Nm == 27)).ToList()[0].Id);
            return UtilService.searchLoad(WEB_APP_ID, frm, screen.Action_Tx, compid, Convert.ToString(screen.ID));
        }

        public ActionClass afterFCSDocument(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass act = null;
            if (Convert.ToString(frm["DOCUMENT_TYPE_ID"]) == "0")
            {
                act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
            }
            else
            {
                if (frm["REMOVE_NM"] == "1")
                {
                    frm["s"] = "update";
                    frm.Add("ID", frm["REMOVE_ID"]);
                    frm.Add("ACTIVE_YN", "0");
                    string filePath = frm["PATH_TX"];
                    System.IO.File.Delete(filePath);

                    List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
                    Dictionary<string, object> d = new Dictionary<string, object>();
                    d["ID"] = frm["REMOVE_ID"];
                    d["ACTIVE_YN"] = "0";
                    d["REMOVE_NM"] = frm["REMOVE_NM"];

                    list.Add(d);
                    act = UtilService.insertOrUpdate("Training", "MEM_FCS_REG_DOCUMENT_DTL_T", list);
                }
                else
                {
                    string File_name_tx = string.Empty;
                    string file_path_tx = string.Empty;
                    string FolderName = string.Empty;
                    FolderName = "MEMBERSHIP\\FCS\\UPLOADS\\STUDENT_ID_" + Convert.ToString(frm["MEMNO_NM"]) + "\\" + Convert.ToString(frm["FCS_REG_ID"]) + "\\DOC" + Convert.ToString(frm["DOCUMENT_TYPE_ID"]);
                    if (ConfigurationManager.AppSettings.AllKeys.Contains("GLOBAL_DOCUMENT_ROOT"))
                        FolderName = Convert.ToString(ConfigurationManager.AppSettings["GLOBAL_DOCUMENT_ROOT"]) + FolderName;
                    else
                        FolderName = AppDomain.CurrentDomain.BaseDirectory + FolderName;

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
                    act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
                }
            }
            return act;
        }

        public ActionClass beforeFIRMDocument(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            string compid = "";
            compid = Convert.ToString((screen.ScreenComponents.Where(x => x.Comp_Type_Nm == 27)).ToList()[0].Id);
            return UtilService.searchLoad(WEB_APP_ID, frm, screen.Action_Tx, compid, Convert.ToString(screen.ID));
        }

        public ActionClass afterFIRMDocument(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass act = null;
            if (Convert.ToString(frm["DOCUMENT_TYPE_ID"]) == "0")
            {
                act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
            }
            else
            {
                if (frm["REMOVE_NM"] == "1")
                {
                    frm["s"] = "update";
                    frm.Add("ID", frm["REMOVE_ID"]);
                    frm.Add("ACTIVE_YN", "0");
                    string filePath = frm["PATH_TX"];
                    System.IO.File.Delete(filePath);

                    List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
                    Dictionary<string, object> d = new Dictionary<string, object>();
                    d["ID"] = frm["REMOVE_ID"];
                    d["ACTIVE_YN"] = "0";
                    d["REMOVE_NM"] = frm["REMOVE_NM"];

                    list.Add(d);
                    act = UtilService.insertOrUpdate("Training", "MEM_FIRM_REG_DOCUMENT_DTL_T", list);
                }
                else
                {
                    string File_name_tx = string.Empty;
                    string file_path_tx = string.Empty;
                    string FolderName = string.Empty;
                    FolderName = "MEMBERSHIP\\FCS\\UPLOADS\\STUDENT_ID_" + Convert.ToString(frm["MEMNO_NM"]) + "\\" + Convert.ToString(frm["FCS_REG_ID"]) + "\\DOC" + Convert.ToString(frm["DOCUMENT_TYPE_ID"]);
                    if (ConfigurationManager.AppSettings.AllKeys.Contains("GLOBAL_DOCUMENT_ROOT"))
                        FolderName = Convert.ToString(ConfigurationManager.AppSettings["GLOBAL_DOCUMENT_ROOT"]) + FolderName;
                    else
                        FolderName = AppDomain.CurrentDomain.BaseDirectory + FolderName;

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
                    act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
                }
            }
            return act;
        }

        public ActionClass beforeACSRegView(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            string compid = "";
            compid = Convert.ToString((screen.ScreenComponents.Where(x => x.Comp_Type_Nm == 27)).ToList()[0].Id);
            return UtilService.searchLoad(WEB_APP_ID, frm, screen.Action_Tx, compid, Convert.ToString(screen.ID));
        }

        public ActionClass beforeACSRegPayment(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            return Util.UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterACSRegPayment(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass act = new ActionClass();
            act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);
            int PaymentID = 0;

            if (act.StatMessage.ToLower() == "success" && act.DecryptData != null && !act.DecryptData.Trim().Equals(""))
            {
                if (!string.IsNullOrEmpty(act.DecryptData))
                {
                    JObject res = JObject.Parse(act.DecryptData);
                    foreach (JProperty jproperty in res.Properties())
                    {
                        if (jproperty.Name != null)
                        {
                            DataTable dtdata = JsonConvert.DeserializeObject<DataTable>(jproperty.Value.ToString());
                            PaymentID = Convert.ToInt32(dtdata.Rows[0]["ID"]);

                            List<Dictionary<string, object>> lstPaymentHead = new List<Dictionary<string, object>>();
                            Dictionary<string, object> dHead;
                            for (int i = 1; i <= 10; i++)
                            {
                                if (!string.IsNullOrEmpty(frm["FEE_HEAD_ID-" + i]))
                                {
                                    dHead = new Dictionary<string, object>();
                                    dHead["PAYMENT_ID"] = PaymentID;
                                    dHead["FEE_HEAD_ID"] = frm["FEE_HEAD_ID-" + i];
                                    dHead["FEE_HEAD_GST_AMOUNT"] = frm["FEE_HEAD_GST_AMOUNT-" + i];
                                    dHead["FEE_HEAD_AMOUNT"] = frm["FEE_HEAD_AMOUNT-" + i];
                                    lstPaymentHead.Add(dHead);
                                }
                            }
                            if (lstPaymentHead.Count > 0)
                                act = UtilService.insertOrUpdate("Training", "MEM_PAYMENT_DETAILS_T", lstPaymentHead);
                        }
                    }
                }
            }

            return act;
        }

        public ActionClass beforeRenewalPaymentQuickLink(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            return Util.UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterRenewalPaymentQuickLink(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass act = new ActionClass();

            if (!string.IsNullOrEmpty(frm["MEMBERSHIP_NO_TX"]))
                HttpContext.Current.Session["MEMBERSHIP_NO"] = frm["MEMBERSHIP_NO_TX"];
            if (!string.IsNullOrEmpty(frm["COP_TX"]) && frm["COP_TX"]!="0")
                HttpContext.Current.Session["COP_NO"] = frm["COP_TX"];

            act = Util.UtilService.afterSubmit(WEB_APP_ID, frm);

            List<Dictionary<string, object>> lstPaymentHead;
            Dictionary<string, object> dHead;
            int PaymentID = 0;
            int REF_ID = 0;

            #region MEM_PAYMENT_T
            if (act.StatMessage.ToLower() == "success" && act.DecryptData != null && !act.DecryptData.Trim().Equals(""))
            {
                if (!string.IsNullOrEmpty(act.DecryptData))
                {
                    JObject res = JObject.Parse(act.DecryptData);
                    foreach (JProperty jproperty in res.Properties())
                    {
                        if (jproperty.Name != null)
                        {
                            DataTable dtdata = JsonConvert.DeserializeObject<DataTable>(jproperty.Value.ToString());
                            REF_ID = Convert.ToInt32(dtdata.Rows[0]["ID"]);

                            lstPaymentHead = new List<Dictionary<string, object>>();
                            dHead = new Dictionary<string, object>();

                            dHead["REF_ID"] = REF_ID;
                            dHead["CONCESSION_TYPE_TX"] = frm["CONCESSION_TYPE_TX"];
                            dHead["CONCESSION_AMOUNT_NM"] = frm["CONCESSION_AMOUNT_NM"];
                            dHead["TOTAL_FEE_WO_GST"] = frm["TOTAL_FEE_WO_GST"];
                            dHead["GST_RATE_NM"] = frm["GST_RATE_NM"];
                            dHead["GST_AMOUNT"] = frm["GST_AMOUNT"];
                            dHead["TOTAL_FEE"] = frm["TOTAL_FEE"];
                            dHead["PURPOSE_TX"] = frm["PURPOSE_TX"];

                            lstPaymentHead.Add(dHead);

                            if (lstPaymentHead.Count > 0)
                                act = UtilService.insertOrUpdate("MembershipTemp", "MEM_PAYMENT_T", lstPaymentHead);
                        }
                    }
                }
            }
            #endregion

            #region MEM_PAYMENT_DETAILS_T
            if (act.StatMessage.ToLower() == "success" && act.DecryptData != null && !act.DecryptData.Trim().Equals(""))
            {
                if (!string.IsNullOrEmpty(act.DecryptData))
                {
                    JObject res = JObject.Parse(act.DecryptData);
                    foreach (JProperty jproperty in res.Properties())
                    {
                        if (jproperty.Name != null)
                        {
                            DataTable dtdata = JsonConvert.DeserializeObject<DataTable>(jproperty.Value.ToString());
                            PaymentID = Convert.ToInt32(dtdata.Rows[0]["ID"]);

                            lstPaymentHead = new List<Dictionary<string, object>>();
                            for (int i = 1; i <= 10; i++)
                            {
                                if (!string.IsNullOrEmpty(frm["FEE_HEAD_ID-" + i]) && frm["FEE_HEAD_AMOUNT-" + i] != "0")
                                {
                                    dHead = new Dictionary<string, object>();
                                    dHead["PAYMENT_ID"] = PaymentID;
                                    dHead["FEE_HEAD_ID"] = frm["FEE_HEAD_ID-" + i];
                                    dHead["FEE_HEAD_GST_AMOUNT"] = frm["FEE_HEAD_GST_AMOUNT-" + i];
                                    dHead["FEE_HEAD_AMOUNT"] = frm["FEE_HEAD_AMOUNT-" + i];
                                    lstPaymentHead.Add(dHead);
                                }
                            }
                            if (lstPaymentHead.Count > 0)
                                act = UtilService.insertOrUpdate("MembershipTemp", "MEM_PAYMENT_DETAILS_T", lstPaymentHead);
                        }
                    }
                }
            }
            #endregion

            return act;
        }
    }


    internal class exam
    {
        public int question { get; set; }
        public int answer { get; set; }
        public int status { get; set; }
    }
}