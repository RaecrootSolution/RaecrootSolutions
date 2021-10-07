using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Data;
using ICSI_WebApp.Models;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using ICSI_WebApp.BusinessLayer;

namespace ICSI_WebApp.Controllers
{
    public class COPController : Controller
    {
        COPLayer objCOP = new COPLayer();

        [HttpGet]
        public ActionResult Issue_Payment_Response(bool status = false)
        {
            return View();
        }

        public string Validate_COP_First_Step(int User_Id)
        {
            COP_Details_Model obj = new COP_Details_Model();
            string msg = string.Empty;
            try
            {
                if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null && ((List<object>)Session["USER_MENU"]).Count > 0)
                {
                    string loginId = Session["LOGIN_ID"].ToString().Substring(1, Session["LOGIN_ID"].ToString().Length - 1);
                    obj = objCOP.Validate_COP_First_Step(User_Id, Convert.ToInt32(loginId));

                    return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                }
                else
                {
                    return "SESSION";
                }
            }
            catch (Exception ex)
            {
                return "ERROR";
                //int s = _commonRepository.ErrorLog_Insert(ex.Message, "COP", "Insert_COPIssue_Last_Employment_Details", "1");
            }

            //return msg;
        }

        public string Personal_Details_Load(string User_Id)
        {
            COP_Issue_Personal_Details_Model obj = new COP_Issue_Personal_Details_Model();
            try
            {
                if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null && ((List<object>)Session["USER_MENU"]).Count > 0)
                {
                    string loginId = Session["LOGIN_ID"].ToString().Substring(1, Session["LOGIN_ID"].ToString().Length - 1);
                    obj = objCOP.Personal_Details_Load(User_Id, loginId);

                    return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                }
                else
                {
                    return "SESSION";
                }
            }
            catch (Exception ex)
            {
                return "ERROR";
                //int s = _commonRepository.ErrorLog_Insert(ex.Message, "COP", "Insert_COPIssue_Last_Employment_Details", "1");
            }

        }

        public string Insert_Personal_Details_Of_COP(string User_Id, string Id, string ECSIN_EXEMPTION_CATERGORY_ID,
            string ECSIN_NOT_APPLICABLE_YN, string ECSIN_EXEMPTED)
        {
            string msg = string.Empty;
            try
            {
                if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null && ((List<object>)Session["USER_MENU"]).Count > 0)
                {
                    string loginId = Session["LOGIN_ID"].ToString().Substring(1, Session["LOGIN_ID"].ToString().Length - 1);

                    msg = objCOP.Insert_Personal_Details_Of_COP(User_Id, Convert.ToInt32(Id), ECSIN_EXEMPTION_CATERGORY_ID,
                        ECSIN_NOT_APPLICABLE_YN, ECSIN_EXEMPTED, loginId);
                }
                else
                {
                    msg = "SESSION";
                }
            }
            catch (Exception ex)
            {
                msg = "ERROR";
                //int s = _commonRepository.ErrorLog_Insert(ex.Message, "COP", "Insert_COPIssue_Last_Employment_Details", "1");
            }

            return msg;
        }

        public string Submit_CallFor_Docs(string User_Id, string Id)
        {
            string msg = string.Empty;
            try
            {
                if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null && ((List<object>)Session["USER_MENU"]).Count > 0)
                {
                    msg = objCOP.Submit_CallFor_Docs(User_Id, Convert.ToInt32(Id));
                }
                else
                {
                    msg = "SESSION";
                }
            }
            catch (Exception ex)
            {
                msg = "ERROR";
                //int s = _commonRepository.ErrorLog_Insert(ex.Message, "COP", "Insert_COPIssue_Last_Employment_Details", "1");
            }

            return msg;
        }

        //.................................................................

        public string Insert_COPIssue_Last_Employment_Details(string Company_Name, string Designation, string Joining_Date,
            string Last_End_Date, string Employment_Duration, int User_Id)
        {
            string msg = string.Empty;
            try
            {
                if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null && ((List<object>)Session["USER_MENU"]).Count > 0)
                {
                    string loginId = Session["LOGIN_ID"].ToString().Substring(1, Session["LOGIN_ID"].ToString().Length - 1);

                    msg = objCOP.Insert_COPIssue_Last_Employment_Details(Company_Name, Designation,
                        Joining_Date, Last_End_Date, Employment_Duration, User_Id, loginId);
                }
                else
                {
                    msg = "SESSION";
                }
            }
            catch (Exception ex)
            {
                msg = "ERROR";
                //int s = _commonRepository.ErrorLog_Insert(ex.Message, "COP", "Insert_COPIssue_Last_Employment_Details", "1");
            }

            return msg;
        }

        public string Particulars_Of_COP_Load(string User_Id)
        {
            Particulars_Of_COP_Model obj = new Particulars_Of_COP_Model();
            try
            {
                if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null && ((List<object>)Session["USER_MENU"]).Count > 0)
                {
                    obj = objCOP.Particulars_Of_COP_Load(User_Id);

                    return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                }
                else
                {
                    return "SESSION";
                }
            }
            catch (Exception ex)
            {
                return "ERROR";
                //int s = _commonRepository.ErrorLog_Insert(ex.Message, "COP", "Insert_COPIssue_Last_Employment_Details", "1");
            }

        }

        public string Insert_Particulars_Of_COP(int Id, string Practice_Ids, string Other_Service, string User_Id)
        {
            string msg = string.Empty;
            try
            {
                if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null && ((List<object>)Session["USER_MENU"]).Count > 0)
                {
                    msg = objCOP.Insert_Particulars_Of_COP(Id, Practice_Ids, Other_Service, Session["LOGIN_ID"].ToString(), User_Id);
                }
                else
                {
                    msg = "SESSION";
                }
            }
            catch (Exception ex)
            {
                msg = "ERROR";
                //int s = _commonRepository.ErrorLog_Insert(ex.Message, "COP", "Insert_COPIssue_Last_Employment_Details", "1");
            }

            return msg;
        }

        //.................................................

        public string COP_Issue_Other_Institution_Details_Load(string User_Id)
        {
            Other_Institutions_Model obj = new Other_Institutions_Model();
            try
            {
                if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null && ((List<object>)Session["USER_MENU"]).Count > 0)
                {
                    obj = objCOP.COP_Issue_Other_Institution_Details_Load(User_Id);

                    return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                }
                else
                {
                    return "SESSION";
                }
            }
            catch (Exception ex)
            {
                return "ERROR";
                //int s = _commonRepository.ErrorLog_Insert(ex.Message, "COP", "Insert_COPIssue_Last_Employment_Details", "1");
            }

        }

        public string Insert_COP_Other_Firm_Details(string Firm_Details)
        {
            string msg = string.Empty;
            try
            {
                if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null && ((List<object>)Session["USER_MENU"]).Count > 0)
                {
                    Newtonsoft.Json.Linq.JObject JObject = Newtonsoft.Json.Linq.JObject.Parse(Firm_Details);

                    Firm_Details_Model obj = new Firm_Details_Model();

                    obj.USER_ID = Convert.ToInt64(JObject["USER_ID"]);
                    obj.FIRM_TYPE_ID = Convert.ToInt32(JObject["FIRM_TYPE_ID"]);
                    obj.FIRM_NAME_TX = Convert.ToString(JObject["FIRM_NAME_TX"]);
                    obj.ADDRESS1_TX = Convert.ToString(JObject["ADDRESS1_TX"]);
                    obj.ADDRESS2_TX = Convert.ToString(JObject["ADDRESS2_TX"]);
                    obj.ADDRESS3_TX = Convert.ToString(JObject["ADDRESS3_TX"]);
                    obj.COUNTRY_ID = Convert.ToInt32(JObject["COUNTRY_ID"]);
                    obj.STATE_ID = Convert.ToInt32(JObject["STATE_ID"]);
                    obj.DISTRICT_ID = Convert.ToInt32(JObject["DISTRICT_ID"]);
                    obj.CITY_ID = Convert.ToInt32(JObject["CITY_ID"]);
                    obj.PINCODE_NM = Convert.ToInt32(JObject["PINCODE_NM"]);
                    obj.BUSINESS_NATURE_TX = Convert.ToString(JObject["BUSINESS_NATURE_TX"]);
                    obj.NAME_OF_PARTNERS_TX = Convert.ToString(JObject["NAME_OF_PARTNERS_TX"]);
                    obj.OTHER_DETAILS_TX = Convert.ToString(JObject["OTHER_DETAILS_TX"]);
                    obj.REF_REF_ID = 0;
                    obj.REMOVE_NM = 0;
                    obj.REF_ID = 0;

                    msg = objCOP.Insert_COP_Other_Firm_Details(obj);
                }
                else
                {
                    msg = "SESSION";
                }
            }
            catch (Exception ex)
            {
                msg = "ERROR";
                //int s = _commonRepository.ErrorLog_Insert(ex.Message, "COP", "Insert_COPIssue_Last_Employment_Details", "1");
            }

            return msg;
        }

        public string IOID_Delete_Firm_Details(int ID)
        {
            string msg = string.Empty;
            try
            {
                if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null && ((List<object>)Session["USER_MENU"]).Count > 0)
                {
                    msg = objCOP.IOID_Delete_Firm_Details(ID);

                    return msg;
                }
                else
                {
                    return "SESSION";
                }
            }
            catch (Exception ex)
            {
                return "ERROR";
                //int s = _commonRepository.ErrorLog_Insert(ex.Message, "COP", "Insert_COPIssue_Last_Employment_Details", "1");
            }

        }

        public string Insert_COP_Other_Institutions_Details(string Institutions_Details)
        {
            string msg = string.Empty;
            try
            {
                if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null && ((List<object>)Session["USER_MENU"]).Count > 0)
                {
                    Newtonsoft.Json.Linq.JObject JObject = Newtonsoft.Json.Linq.JObject.Parse(Institutions_Details);

                    Other_Institutions_Model obj = new Other_Institutions_Model();

                    obj.ID = Convert.ToInt32(JObject["ID"]);
                    obj.USER_ID = Convert.ToInt64(JObject["USER_ID"]);
                    obj.IS_ICAI_MEMBER = Convert.ToInt32(JObject["IS_ICAI_MEMBER"]);
                    obj.ICAI_MEMBERSHIP_NO_TX = Convert.ToString(JObject["ICAI_MEMBERSHIP_NO_TX"]);
                    obj.IS_ICAI_MEMBER_HC = Convert.ToInt32(JObject["IS_ICAI_MEMBER_HC"]);

                    obj.IS_COST_MEMBERSHIP = Convert.ToInt32(JObject["IS_COST_MEMBERSHIP"]);
                    obj.COST_MEMBERSHIP_NO_TX = Convert.ToString(JObject["COST_MEMBERSHIP_NO_TX"]);
                    obj.COST_MEMBERSHIP_HC = Convert.ToInt32(JObject["COST_MEMBERSHIP_HC"]);

                    obj.IS_BAR_COUNCIL = Convert.ToInt32(JObject["IS_BAR_COUNCIL"]);
                    obj.BAR_COUNCIL_NO_TX = Convert.ToString(JObject["BAR_COUNCIL_NO_TX"]);
                    obj.IS_BAR_COUNCIL_HC = Convert.ToInt32(JObject["IS_BAR_COUNCIL_HC"]);

                    obj.IS_PROF_INSTITUTE = Convert.ToInt32(JObject["IS_PROF_INSTITUTE"]);
                    obj.PROF_INSTITUTE_NO_TX = Convert.ToString(JObject["PROF_INSTITUTE_NO_TX"]);
                    obj.IS_PROF_INSTITUTE_HC = Convert.ToInt32(JObject["IS_PROF_INSTITUTE_HC"]);

                    msg = objCOP.Insert_COP_Other_Institutions_Details(obj);
                }
                else
                {
                    msg = "SESSION";
                }
            }
            catch (Exception ex)
            {
                msg = "ERROR";
                //int s = _commonRepository.ErrorLog_Insert(ex.Message, "COP", "Insert_COPIssue_Last_Employment_Details", "1");
            }

            return msg;
        }

        //..........................................................................

        public string COP_Issue_Declaration_Details_Load(string User_Id)
        {
            Declaration_Dependants_Model obj = new Declaration_Dependants_Model();
            try
            {
                if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null && ((List<object>)Session["USER_MENU"]).Count > 0)
                {
                    string loginId = Session["LOGIN_ID"].ToString().Substring(1, Session["LOGIN_ID"].ToString().Length - 1);

                    obj = objCOP.COP_Issue_Declaration_Details_Load(User_Id, loginId);

                    return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                }
                else
                {
                    return "SESSION";
                }
            }
            catch (Exception ex)
            {
                return "ERROR";
                //int s = _commonRepository.ErrorLog_Insert(ex.Message, "COP", "Insert_COPIssue_Last_Employment_Details", "1");
            }

        }

        public string Insert_COP_Declaration_Dependent(string Dependants)
        {
            string msg = string.Empty;
            try
            {
                if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null && ((List<object>)Session["USER_MENU"]).Count > 0)
                {
                    DataTable dtDependants = (DataTable)JsonConvert.DeserializeObject(Dependants, (typeof(DataTable)));

                    msg = objCOP.Insert_COP_Declaration_Dependent(dtDependants);
                }
                else
                {
                    msg = "SESSION";
                }
            }
            catch (Exception ex)
            {
                msg = "ERROR";
                //int s = _commonRepository.ErrorLog_Insert(ex.Message, "COP", "Insert_COPIssue_Last_Employment_Details", "1");
            }

            return msg;
        }

        //............................................................................

        public string COP_ISSUE_PREVIEW_LOAD(string User_Id)
        {
            COP_Issue_Personal_Details_Model obj = new COP_Issue_Personal_Details_Model();
            try
            {
                if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null && ((List<object>)Session["USER_MENU"]).Count > 0)
                {
                    string loginId = Session["LOGIN_ID"].ToString().Substring(1, Session["LOGIN_ID"].ToString().Length - 1);

                    obj = objCOP.COP_ISSUE_PREVIEW_LOAD(User_Id, loginId);

                    return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                }
                else
                {
                    return "SESSION";
                }
            }
            catch (Exception ex)
            {
                return "ERROR";
                //int s = _commonRepository.ErrorLog_Insert(ex.Message, "COP", "Insert_COPIssue_Last_Employment_Details", "1");
            }

        }

        public string COP_ISSUE_PAYMENT_LOAD(string User_Id)
        {
            COP_ISSUE_PAYMENT_MODEL obj = new COP_ISSUE_PAYMENT_MODEL();
            try
            {
                if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null && ((List<object>)Session["USER_MENU"]).Count > 0)
                {
                    string loginId = Session["LOGIN_ID"].ToString().Substring(1, Session["LOGIN_ID"].ToString().Length - 1);

                    obj = objCOP.COP_ISSUE_PAYMENT_LOAD(User_Id, loginId);

                    return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                }
                else
                {
                    return "SESSION";
                }
            }
            catch (Exception ex)
            {
                return "ERROR";
                //int s = _commonRepository.ErrorLog_Insert(ex.Message, "COP", "Insert_COPIssue_Last_Employment_Details", "1");
            }

        }

        //.............. Admin ...................................
        public string Admin_COP_Review_Search(string User_Id, string Name, string Request_From, string Request_To, string Email_Id, string Admission_From,
            string Admission_To, string MemberShip_Number, string MobileNo, string COP_Number, int Application_Status_Id, int Request_Type_Id,
            int Internal_Dept_Status_Id)
        {
            string msg = string.Empty;
            List<Admin_COP_Track_Search_Model> obj = new List<Admin_COP_Track_Search_Model>();
            try
            {
                if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null && ((List<object>)Session["USER_MENU"]).Count > 0)
                {

                    obj = objCOP.Admin_COP_Review_Search(User_Id, Name, Request_From, Request_To, Email_Id, Admission_From, Admission_To,
                        MemberShip_Number, MobileNo, COP_Number, Application_Status_Id, Request_Type_Id, Internal_Dept_Status_Id);

                    return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                }
                else
                {
                    msg = "SESSION";
                }
            }
            catch (Exception ex)
            {
                msg = "ERROR";
                //int s = _commonRepository.ErrorLog_Insert(ex.Message, "COP", "Insert_COPIssue_Last_Employment_Details", "1");
            }

            return msg;
        }

        public string Admin_COP_Personal_Details_Issue_View_Load(string User_Id)
        {
            COP_Issue_Personal_Details_Model obj = new COP_Issue_Personal_Details_Model();
            try
            {
                if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null && ((List<object>)Session["USER_MENU"]).Count > 0)
                {
                    string loginId = objCOP.Get_MemberShip_No(User_Id);

                    obj = objCOP.COP_ISSUE_PREVIEW_LOAD(User_Id, loginId);

                    return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                }
                else
                {
                    return "SESSION";
                }
            }
            catch (Exception ex)
            {
                return "ERROR";
                //int s = _commonRepository.ErrorLog_Insert(ex.Message, "COP", "Insert_COPIssue_Last_Employment_Details", "1");
            }

        }

        public string Insert_Admin_COP_Personal_Details_Issue_View(string Document_Details)
        {
            string msg = string.Empty;
            try
            {
                if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null && ((List<object>)Session["USER_MENU"]).Count > 0)
                {
                    DataTable dtExperience = (DataTable)JsonConvert.DeserializeObject(Document_Details, (typeof(DataTable)));
                    msg = objCOP.Insert_Admin_COP_Personal_Details_Issue_View(dtExperience);
                }
                else
                {
                    msg = "SESSION";
                }
            }
            catch (Exception ex)
            {
                msg = "ERROR";
                //int s = _commonRepository.ErrorLog_Insert(ex.Message, "COP", "Insert_COPIssue_Last_Employment_Details", "1");
            }

            return msg;
        }

        public string Admin_COP_Declaration_Issue_View_Load(string User_Id)
        {
            Admin_COP_Approval_Load obj = new Admin_COP_Approval_Load();
            try
            {
                if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null && ((List<object>)Session["USER_MENU"]).Count > 0)
                {
                    //string loginId = Session["LOGIN_ID"].ToString().Substring(1, Session["LOGIN_ID"].ToString().Length - 1);

                    obj = objCOP.Admin_COP_Declaration_Issue_View_Load(User_Id);

                    return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                }
                else
                {
                    return "SESSION";
                }
            }
            catch (Exception ex)
            {
                return "ERROR";
                //int s = _commonRepository.ErrorLog_Insert(ex.Message, "COP", "Insert_COPIssue_Last_Employment_Details", "1");
            }

        }

        public string Insert_Admin_COP_Approval_Details(string Approve_Details)
        {
            string msg = string.Empty;
            try
            {
                if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null && ((List<object>)Session["USER_MENU"]).Count > 0)
                {
                    Newtonsoft.Json.Linq.JObject JObject = Newtonsoft.Json.Linq.JObject.Parse(Approve_Details);

                    COP_Forward_Details_Model obj = new COP_Forward_Details_Model();

                    obj.USER_ID = Convert.ToInt64(JObject["USER_ID"]);
                    obj.APPLICATION_STATUS_ID = Convert.ToInt32(JObject["APPLICATION_STATUS_ID"]);
                    obj.ROLE_ID = Convert.ToInt32(JObject["ROLE_ID"]);
                    obj.ROLE_NAME = Convert.ToString(JObject["ROLE_NAME"]);
                    obj.NEXT_ROLE_ID = Convert.ToInt32(JObject["NEXT_ROLE_ID"]);
                    obj.NEXT_ROLE_NAME = Convert.ToString(JObject["NEXT_ROLE_NAME"]);

                    obj.INTERNAL_REMARKS = Convert.ToString(JObject["INTERNAL_REMARKS"]);
                    obj.REMARKS_FOR_MEMBER = Convert.ToString(JObject["REMARKS_FOR_MEMBER"]);

                    obj.SCREEN_ID = Convert.ToInt32(JObject["SCREEN_ID"]);
                    obj.RES_STATE_ID = Convert.ToString(JObject["RES_STATE_ID"]);
                    obj.MOBILE_NO_TX = Convert.ToString(JObject["MOBILE_NO_TX"]);
                    obj.EMAIL_ID_TX = Convert.ToString(JObject["EMAIL_ID_TX"]);

                    msg = objCOP.Insert_Admin_COP_Approval_Details(obj);
                }
                else
                {
                    msg = "SESSION";
                }
            }
            catch (Exception ex)
            {
                msg = "ERROR";
                //int s = _commonRepository.ErrorLog_Insert(ex.Message, "COP", "Insert_COPIssue_Last_Employment_Details", "1");
            }

            return msg;
        }


        public string COP_ISSUE_PROCESS_PAYMENT(string User_Id)
        {
            string message = string.Empty;
            try
            {
                if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null && ((List<object>)Session["USER_MENU"]).Count > 0)
                {
                    string loginId = Session["LOGIN_ID"].ToString().Substring(1, Session["LOGIN_ID"].ToString().Length - 1);

                    message = objCOP.COP_ISSUE_PROCESS_PAYMENT(User_Id, loginId);

                    return message;
                }
                else
                {
                    return "SESSION";
                }
            }
            catch (Exception ex)
            {
                return "ERROR";
                //int s = _commonRepository.ErrorLog_Insert(ex.Message, "COP", "Insert_COPIssue_Last_Employment_Details", "1");
            }

        }

        public string Get_COP_Issue_Payment_Response(string User_Id, string Status)
        {
            string message = string.Empty;
            int s = objCOP.ErrorLog_Insert("Payment Status : " + Status, "COP", "Get_COP_Issue_Payment_Response", "1");
            try
            {
                if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null && ((List<object>)Session["USER_MENU"]).Count > 0)
                {
                    string loginId = Session["LOGIN_ID"].ToString().Substring(1, Session["LOGIN_ID"].ToString().Length - 1);

                    message = objCOP.COP_ISSUE_PROCESS_PAYMENT(User_Id, loginId, Status);

                    return message;
                }
                else
                {
                    return "SESSION";
                }
            }
            catch (Exception ex)
            {
                return "ERROR";
                //int s = _commonRepository.ErrorLog_Insert(ex.Message, "COP", "Insert_COPIssue_Last_Employment_Details", "1");
            }

        }

        //................................................................................

        public void Admin_Search_ExportToExcel(string User_Id, string Name, string Request_From, string Request_To, string Email_Id, string Admission_From,
            string Admission_To, string MemberShip_Number, string MobileNo, string COP_Number, int Application_Status_Id, int Request_Type_Id,
            int Internal_Dept_Status_Id)
        {
            string msg = string.Empty;
            List<Admin_COP_Track_Search_Model> obj = new List<Admin_COP_Track_Search_Model>();
            try
            {
                if (Session["LOGIN_ID"] != null && Session["USER_MENU"] != null && ((List<object>)Session["USER_MENU"]).Count > 0)
                {

                    obj = objCOP.Admin_COP_Review_Search(User_Id, Name, Request_From, Request_To, Email_Id, Admission_From, Admission_To,
                        MemberShip_Number, MobileNo, COP_Number, Application_Status_Id, Request_Type_Id, Internal_Dept_Status_Id);

                    if (obj.Count > 0)
                    {
                        DataTable vDataTable = new DataTable();
                        vDataTable.Columns.Add("Request_Type");
                        vDataTable.Columns.Add("Name");
                        vDataTable.Columns.Add("Email_ID");
                        vDataTable.Columns.Add("Mobile_No");
                        vDataTable.Columns.Add("Request_Date");
                        vDataTable.Columns.Add("Admission_Date");
                        vDataTable.Columns.Add("Approval_Date");
                        vDataTable.Columns.Add("Fee_Paid");
                        vDataTable.Columns.Add("Application_Status");
                        vDataTable.Columns.Add("COP_Number");
                        vDataTable.Columns.Add("COP_Issue_Date");
                        vDataTable.Columns.Add("Internal_Deptt_Status");
                        vDataTable.Columns.Add("Membership_Number");
                        vDataTable.Columns.Add("Transcation_ID");
                        vDataTable.Columns.Add("Transcation_Date");
                        vDataTable.Columns.Add("Mode_Of_Payment");
                        vDataTable.Columns.Add("Payment_Status");

                        for (int i = 0; i < obj.Count; i++)
                        {
                            DataRow dr = vDataTable.NewRow();

                            dr["Request_Type"] = obj[i].Request_Type;
                            dr["Name"] = obj[i].Name;
                            dr["Email_ID"] = obj[i].Email_Id;
                            dr["Mobile_No"] = obj[i].MobileNo;
                            dr["Request_Date"] = obj[i].Request_Date;
                            dr["Admission_Date"] = obj[i].Admission_Date;
                            dr["Approval_Date"] = obj[i].Approval_Date;
                            dr["Fee_Paid"] = obj[i].Fee_Paid;
                            dr["Application_Status"] = obj[i].Application_Status;
                            dr["COP_Number"] = obj[i].COP_Number;
                            dr["COP_Issue_Date"] = obj[i].COP_Issue_Date;
                            dr["Internal_Deptt_Status"] = obj[i].Internal_Dept_Status;
                            dr["Membership_Number"] = obj[i].MemberShip_Number;
                            dr["Transcation_ID"] = obj[i].Transcation_Id;
                            dr["Transcation_Date"] = obj[i].Transcation_Date;
                            dr["Mode_Of_Payment"] = obj[i].Mode_Of_Payment;
                            dr["Payment_Status"] = obj[i].Payment_Status;

                            vDataTable.Rows.Add(dr);
                        }

                        Common_ExporttoExcel(vDataTable, "Search_Details.xls");
                    }

                }
                else
                {
                    msg = "SESSION";
                }
            }
            catch (Exception ex)
            {
                msg = "ERROR";
            }

        }

        /// <summary>
        /// Common for export to excel.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="FileName"></param>
        public void Common_ExporttoExcel(DataTable table, string FileName)
        {
            string msg = string.Empty;
            try
            {
                HttpContext.Response.Clear();
                HttpContext.Response.ClearContent();
                HttpContext.Response.ClearHeaders();
                HttpContext.Response.Buffer = true;
                HttpContext.Response.ContentType = "application/ms-excel";
                HttpContext.Response.Write(@"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN"">");
                HttpContext.Response.AddHeader("Content-Disposition", "attachment;filename=" + FileName + "");

                HttpContext.Response.Charset = "utf-8";
                HttpContext.Response.ContentEncoding = System.Text.Encoding.GetEncoding("windows-1250");
                //sets font
                HttpContext.Response.Write("<font style='font-size:10.0pt; font-family:Calibri;'>");
                HttpContext.Response.Write("<BR><BR><BR>");
                //sets the table border, cell spacing, border color, font of the text, background, foreground, font height
                HttpContext.Response.Write("<Table border='1' bgColor='#ffffff' " +
                  "borderColor='#000000' cellSpacing='0' cellPadding='0' " +
                  "style='font-size:10.0pt; font-family:Calibri; background:white;'> <TR>");
                //am getting my grid's column headers
                int columnscount = table.Columns.Count;

                for (int j = 0; j < columnscount; j++)
                {      //write in new column
                    HttpContext.Response.Write("<Td>");
                    //Get column headers  and make it as bold in excel columns
                    HttpContext.Response.Write("<B>");
                    HttpContext.Response.Write(table.Columns[j].ColumnName.ToString());
                    HttpContext.Response.Write("</B>");
                    HttpContext.Response.Write("</Td>");
                }
                HttpContext.Response.Write("</TR>");
                foreach (DataRow row in table.Rows)
                {//write in new row
                    HttpContext.Response.Write("<TR>");
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        HttpContext.Response.Write("<Td>");
                        HttpContext.Response.Write(row[i].ToString());
                        HttpContext.Response.Write("</Td>");
                    }

                    HttpContext.Response.Write("</TR>");
                }
                HttpContext.Response.Write("</Table>");
                HttpContext.Response.Write("</font>");
                HttpContext.Response.Flush();
                HttpContext.Response.End();
            }
            catch (Exception ex)
            {
                msg = "ERROR : " + ex.Message;
                ViewBag.Message = msg;
                Response.End();
            }
        }

    }
}