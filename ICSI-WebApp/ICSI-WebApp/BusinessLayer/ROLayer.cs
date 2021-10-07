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

namespace ICSI_WebApp.BusinessLayer
{
    public class ROLayer
    {
        public ActionClass searchTrainingSchedule(int WEB_APP_ID, FormCollection frm, string scrtype, string scmptid, string screenId = "",Screen_T sreen=null)
        {
            //frm["SCRH_TRAINING_CALENDER_T.REGION_ID"] = Convert.ToString(HttpContext.Current.Session["REGION_ID"]);
            return Util.UtilService.searchLoad(WEB_APP_ID, frm, scrtype, scmptid, screenId);
        }

        public ActionClass beforeTrainingSchedule(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            return Util.UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterTrainingSchedule(int WEB_APP_ID, FormCollection frm)
        {
            return Util.UtilService.afterSubmit(WEB_APP_ID, frm);
        }

        public ActionClass afterTrainingCalendar(int WEB_APP_ID, FormCollection frm)
        {
            int regionId = Convert.ToInt32(HttpContext.Current.Session["REGION_ID"]);
            if (regionId > 0)
                frm["REGION_ID"] = Convert.ToString(HttpContext.Current.Session["REGION_ID"]);

            return Util.UtilService.afterSubmit(WEB_APP_ID, frm);
        }

        public ActionClass beforeTrainingCalendar(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            int officeId = Convert.ToInt32(HttpContext.Current.Session["OFFICE_ID"]);
            if (officeId > 0)
            {
                frm["CHAPTER_ID"] = Convert.ToString(HttpContext.Current.Session["OFFICE_ID"]);
                screen.Screen_Content_Tx = screen.Screen_Content_Tx.Replace("@CHAPTER_ID", Convert.ToString(HttpContext.Current.Session["OFFICE_ID"]));
            }

            return Util.UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass beforeCompletionCertificate(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            Screen_Comp_T s = screen.ScreenComponents.Where(x => x.Comp_Type_Nm == Convert.ToInt32(UtilService.HTMLTag.LOAD)).FirstOrDefault();
            return Util.UtilService.searchLoad(WEB_APP_ID, frm, screen.Action_Tx, Convert.ToString(s.Id), Convert.ToString(screen.ID));
        }

        public ActionClass afterCompletionCertificate(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass act = new ActionClass();
            Screen_T screen = Util.UtilService.screenObject(WEB_APP_ID, frm);
            string UserName = HttpContext.Current.Session["LOGIN_ID"].ToString();
            string Session_Key = HttpContext.Current.Session["SESSION_KEY"].ToString();
            string AppUrl = Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]);
           
            Dictionary<string, object> conditions = new Dictionary<string, object>();
            List<Dictionary<string, object>> lstData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> lstData1 = new List<Dictionary<string, object>>();

            #region First Table updation
            Dictionary<string, object> tableData = new Dictionary<string, object>();
            tableData.Add("ID", Convert.ToInt32(frm["ID"]));
            tableData.Add("APPROVE_NM", Convert.ToInt32(frm["APPROVE_NM"]));
            tableData.Add("APPROVE_REMARKS_TX", Convert.ToString(frm["APPROVE_REMARKS_TX"]));
            lstData1.Add(tableData);

            AppUrl = AppUrl + "/AddUpdate";
            lstData.Add(Util.UtilService.addSubParameter(UtilService.getApplicationScheme(screen), "STUDENT_REGISTER_TRAINING_T", 0, 0, lstData1, conditions));
            act = UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "update", lstData));
            //act.columnMetadata = jdata;
            #endregion

            #region 2ND TABLE INSERTION            
            //lstData1 = new List<Dictionary<string, object>>();
            //lstData = new List<Dictionary<string, object>>();
            //Dictionary<string, object> Mul_tblData;

            //if (frm["Ops_Multiple"] == "yes")
            //{
            //    int recCount = 0;
            //    foreach (var key in frm.AllKeys)
            //    {
            //        if (key.StartsWith("MUL-"))
            //        {
            //            string[] splitArr = frm[key].Split(',');
            //            recCount = splitArr.Length;
            //            break;
            //        }
            //    }
            //    for (int i = 0; i < recCount; i++)
            //    {
            //        Mul_tblData = new Dictionary<string, object>();
            //        foreach (var key in frm.AllKeys)
            //        {
            //            if (key.StartsWith("MUL-"))
            //            {
            //                string[] splitArr = frm[key].Split(',');
            //                if (key.Substring(4) != "ID")
            //                    Mul_tblData.Add(key.Substring(4), splitArr[i]);
            //                else
            //                    Mul_tblData.Add(key.Substring(4), Convert.ToInt64(splitArr[i]));
            //            }
            //        }
            //        lstData1.Add(Mul_tblData);
            //    }

            //    //AppUrl = AppUrl + "/AddUpdate";
            //    lstData.Add(Util.UtilService.addSubParameter(UtilService.getApplicationScheme(screen), "COMPLETION_CERTIFICATE_APPROVAL_T", 0, 0, lstData1, conditions));
            //    ActionClass obj=  UtilService.createRequestObject(AppUrl, UserName, Session_Key, UtilService.createParameters("", "", "", "", "", "insert", lstData));
            //}
            #endregion

            //send Email and SMS

            #region Old code
            //string StudentID = string.Empty;
            //if (!string.IsNullOrEmpty(Convert.ToString(HttpContext.Current.Session["STUDENT_ID"])))
            //{
            //    StudentID = HttpContext.Current.Session["STUDENT_ID"].ToString();
            //}

            //DataTable dt = new DataTable();
            //Dictionary<string, object> condition = new Dictionary<string, object>();
            //condition.Add("ACTIVE_YN", 1);
            //condition.Add("REG_NUMBER_TX", frm["REG_NUMBER_TX"]);
            //dt = UtilService.getData("Training", "STUDENT_T", condition, null, 0, 1);
            //if (dt.Rows.Count > 0)
            //{
            //    StudentID = dt.Rows[0]["ID"].ToString();
            //}
            //if (Convert.ToInt32(act.StatCode) >= 0 && StudentID != string.Empty)
            //{
            //    Util.UtilService.SendEmailAndSMS(StudentID, Convert.ToInt32(frm["si"]));
            //}
            #endregion

            if (!string.IsNullOrEmpty(act.DecryptData) && screen.is_Email_yn)
            {
                int UniqueId = Convert.ToInt32(frm["ID"]);
                UtilService.storeEmailData(act, screen, "update", AppUrl, Session_Key, UserName, UniqueId);
            }
            return act;
        }

        public ActionClass InsertQuestions(int WEB_APP_ID, FormCollection frm)
        {
            ActionClass actionClass = null;
            string ro = frm["u"];
            int IDexists = 0;
            string COURSE_ID = frm["COURSE_ID"];
            string SYLLABUS_ID = frm["SYLLABUS_ID"];
            string MODULE_ID = frm["MODULE_ID"];
            string SUBJECT_ID = frm["SUBJECT_ID"];
            string PAPER_ID = frm["PAPER_ID"];
            string TOPIC_ID = frm["TOPIC_ID"];
            string QUESTION_TYPE_ID = frm["QUESTION_TYPE_ID"];
            string QUESTION_TX = frm["QUESTION_TX"];
            string OPTION_1_TX = frm["OPTION_1_TX"];
            string OPTION_2_TX = frm["OPTION_2_TX"];
            string OPTION_3_TX = frm["OPTION_3_TX"];
            string OPTION_4_TX = frm["OPTION_4_TX"];
            string OPTION_5_TX = frm["OPTION_5_TX"];
            string NO_OF_OPTION_NM = frm["NO_OF_OPTION_NM"];
            string ANSWER_NM = frm["ANSWER_NM"];
            if (string.IsNullOrEmpty(MODULE_ID) || string.IsNullOrEmpty(QUESTION_TYPE_ID) || string.IsNullOrEmpty(QUESTION_TX) || string.IsNullOrEmpty(NO_OF_OPTION_NM) || string.IsNullOrEmpty(ANSWER_NM))
            {
                actionClass = new ActionClass()
                {
                    StatMessage = "Invalid data"
                };
                return actionClass;
            }
            else
            {
                DataTable dtRDetails2 = new DataTable();
                Dictionary<string, object> conditions2 = new Dictionary<string, object>();                
                conditions2.Add("QUESTION_TYPE_ID", QUESTION_TYPE_ID);
                conditions2.Add("QUESTION_TX", QUESTION_TX);
                conditions2.Add("NO_OF_OPTION_NM", NO_OF_OPTION_NM);
                conditions2.Add("ANSWER_NM", ANSWER_NM);
     
                dtRDetails2 = UtilService.getData("Training", "OBJECTIVE_QUESTIONS_T", conditions2, null, 0, 1);
                if (dtRDetails2.Rows.Count > 0)
                {
                    IDexists = Convert.ToInt32(dtRDetails2.Rows[0]["ID"]);

                }
                List<exam> mylist = new List<exam>();



                List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();


                Dictionary<string, object> d = new Dictionary<string, object>
                {
                    //["COURSE_ID"] = COURSE_ID,
                    //["SYLLABUS_ID"] = SYLLABUS_ID,
                    ["MODULE_ID"] = MODULE_ID,
                    ["SUBJECT_ID"] = SUBJECT_ID,
                    ["PAPER_ID"] = PAPER_ID,
                    ["TOPIC_ID"] = TOPIC_ID,
                    ["QUESTION_TYPE_ID"] = QUESTION_TYPE_ID,
                    ["QUESTION_TX"] = QUESTION_TX,
                    ["OPTION_1_TX"] = OPTION_1_TX,
                    ["OPTION_2_TX"] = OPTION_2_TX,
                    ["OPTION_3_TX"] = OPTION_3_TX,
                    ["OPTION_4_TX"] = OPTION_4_TX,
                    ["OPTION_5_TX"] = OPTION_5_TX,
                    ["NO_OF_OPTION_NM"] = NO_OF_OPTION_NM,
                    ["ANSWER_NM"] = ANSWER_NM,
                    ["ID"] = IDexists
                };
                int Id = 0;
                list.Add(d);
                actionClass = UtilService.insertOrUpdate("Training", "OBJECTIVE_QUESTIONS_T", list);

                return actionClass;
            }
        }

        public ActionClass beforeROBrucher(int WEB_APP_ID, FormCollection frm, Screen_T screen)
        {
            string compid = "";
            compid = Convert.ToString((screen.ScreenComponents.Where(x => x.Comp_Type_Nm == 27)).ToList()[0].Id);
            return UtilService.searchLoad(WEB_APP_ID, frm, screen.Action_Tx, compid, Convert.ToString(screen.ID));
            //return Util.UtilService.beforeLoad(WEB_APP_ID, frm);
        }

        public ActionClass afterROBrucher(int WEB_APP_ID, FormCollection frm)
        {
            string File_name_tx = string.Empty;
            string file_path_tx = string.Empty;
            string FolderName = string.Empty;
            FolderName = "ROEVENT\\BRUCHER\\UPLOADS\\EVENT_ID_" + Convert.ToString(frm["ID"]);
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
                    if (_FileName != "")
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