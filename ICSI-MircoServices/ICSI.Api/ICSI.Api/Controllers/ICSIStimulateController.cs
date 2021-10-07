using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ICSI.Api.DB;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Collections;
using System.Data;
using System.Text;

namespace ICSI.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ICSIStimulateController : ControllerBase
    {
        public static int PASSWORD_TYPE = 1;
        private Database _database;
        private Dictionary<string, DB.Database> _databases;
        private static string DEFAULT_PASSWORD = "5f4dcc3b5aa765d61d8327deb882cf99";
        public ICSIStimulateController(Dictionary<string, DB.Database> databases)
        {
            this._databases = databases;
            _database = databases["DEFAULT"];
        }

        [HttpPost]
        [Route("Fetch")]
        public ActionResult<string> Fetch(object json)
        {
            return Util.UtilService.doAction(_databases, json);
        }

        [HttpPost]
        [Route("Reconcile")]
        public ActionResult<string> Reconcile(object json)
        {
            return Util.UtilService.doAction(_databases, json);
        }

        [HttpPost]
        [Route("SearchForm")]
        public ActionResult<string> SearchForm(object json)
        {
            return Util.UtilService.doAction(_databases, json);
        }

        [HttpPost]
        [Route("AddUpdate")]
        public ActionResult<string> AddUpdate(object json)
        {
            return Util.UtilService.doAction(_databases, json);
        }

        private bool fetchICSILoginDetails(SqlConnection sqlConnection, string sid, out int studentHdrID)
        {
            studentHdrID = 0;
            try
            {
                Dictionary<string, object> userData = new Dictionary<string, object>();
                userData.Add("LOGIN_ID", sid.Trim());
                List<Dictionary<string, object>> l = (List<Dictionary<string, object>>)Util.UtilService.GetData(sqlConnection, "USER_T", userData, 0, 0);
                Hashtable ht = new Hashtable();
                ht.Add("UserId", sid);
                //string spname = "PRD_USER_GETAUTHENTICATE";
                string spname = "PRD_USER_GETAUTHENTICATE_TRAINING";
                SqlConnection icsiConn = (SqlConnection)_databases["ICSI"].CreateOpenConnection(); // TODO - needs to implement the exception handling
                                                                                                   //DataTable dt = Util.UtilService.validateUser(icsiConn, sid);
                DataTable dt = Util.UtilService.ExecuteSP(icsiConn, spname, ht);
                bool isIcsiDataExists = false;
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    if (dt.Rows[0]["Result"].ToString().Equals("8"))
                    {
                        isIcsiDataExists = true;
                        Dictionary<string, object> dict = null;
                        int userFKID = 0;
                        bool isNew = false;
                        userFKID = Convert.ToInt32(dt.Rows[0]["USER_PKID"]);
                        if (l != null && l.Count > 0)
                        {
                            dict = l[0];
                            dict["USER_NAME_TX"] = dt.Rows[0]["FirstName"];
                            dict["LOGIN_PWD_TX"] = PASSWORD_TYPE == 1 ? dt.Rows[0]["USER_PWD"] : DEFAULT_PASSWORD;
                            dict["USER_PKID"] = dt.Rows[0]["USER_PKID"];
                            dict["ROLE_FKID"] = dt.Rows[0]["ROLE_FKID"];
                            dict["ROLE_NAME"] = dt.Rows[0]["ROLE_NAME"];
                            dict["USER_TYPE_FKID"] = dt.Rows[0]["USER_TYPE_FKID"];
                            dict["USER_TYPE_CODE"] = dt.Rows[0]["USER_TYPE_CODE"];
                            dict["USER_TYPE"] = dt.Rows[0]["USER_TYPE"];
                            dict["USERID"] = dt.Rows[0]["UserID"];
                            dict["FIRST_NAME_TX"] = dt.Rows[0]["FirstName"];
                            dict["MIDDLE_NAME_TX"] = dt.Rows[0]["MiddleName"];
                            dict["LAST_NAME_TX"] = dt.Rows[0]["LastName"];
                            dict["ACTIVE_YN"] = 1;
                        }
                        else
                        {
                            isNew = true;
                            dict = new Dictionary<string, object>();
                            dict["USER_ID"] = dt.Rows[0]["LOGIN_ID"];
                            dict["LOGIN_ID"] = dt.Rows[0]["LOGIN_ID"];
                            dict["USER_TYPE_ID"] = 3;
                            dict["USER_NAME_TX"] = dt.Rows[0]["FirstName"];
                            dict["LOGIN_PWD_TX"] = PASSWORD_TYPE == 1 ? dt.Rows[0]["USER_PWD"] : DEFAULT_PASSWORD;
                            dict["USER_PKID"] = dt.Rows[0]["USER_PKID"];
                            dict["ROLE_FKID"] = dt.Rows[0]["ROLE_FKID"];
                            dict["ROLE_NAME"] = dt.Rows[0]["ROLE_NAME"];
                            dict["USER_TYPE_FKID"] = dt.Rows[0]["USER_TYPE_FKID"];
                            dict["USER_TYPE_CODE"] = dt.Rows[0]["USER_TYPE_CODE"];
                            dict["USER_TYPE"] = dt.Rows[0]["USER_TYPE"];
                            dict["USERID"] = dt.Rows[0]["UserID"];
                            dict["FIRST_NAME_TX"] = dt.Rows[0]["FirstName"];
                            dict["MIDDLE_NAME_TX"] = dt.Rows[0]["MiddleName"];
                            dict["LAST_NAME_TX"] = dt.Rows[0]["LastName"];
                            dict["ACTIVE_YN"] = 1;
                        }
                        List<Dictionary<string, object>> userdata = new List<Dictionary<string, object>>();
                        userdata.Add(dict);
                        int id = Util.UtilService.InsertData(sqlConnection, "", "", "USER_T", userdata, 1);
                        if (id > 0)
                        {
                            if (isNew)
                            {
                                Dictionary<string, object> roled = new Dictionary<string, object>();
                                roled["USER_ID"] = id;
                                roled["ROLE_ID"] = 3;
                                userdata = null;
                                userdata = new List<Dictionary<string, object>>();
                                userdata.Add(roled);
                                Util.UtilService.InsertData(sqlConnection, "", "", "USER_ROLE_T", userdata, 1);
                            }
                            userData = null;
                            l = null;
                            StringBuilder sql = new StringBuilder();
                            sql.Append("select  Distinct Region.OfficeName as Region,Chapter.OfficeName, hdr.StudentHdrID,hdr.UserFKID,hdr.OldRegistrationNumber,HDR.RegistrationNumber AS RegistrationNumber,hdr.coursefkid,Sy.SyllabusName, (SELECT TOP 1 RegistrationDate FROM STU_STUDENT_COURSE_DTL WHERE STUDENTHDRFKID=HDR.StudentHdrID AND COURSEFKID=2 ORDER BY RegistrationDate ASC) AS RegistrationDate,HDR.FirstName + ' ' + HDR.MiddleName + ' ' + HDR.LastName  AS [StudentName],ADR.AdrLine1,ADR.AdrLine2,ADR.AdrLine3, CT.CityName, DT.DistrictName,ST.StateName,ADR.PinCode,     hdr.Mobile,hdr.EmailID, hdr.DOB");
                            sql.Append(" FROM  STU_STUDENT_HDR HDR WITH(nolock) INNER JOIN STU_STUDENT_COURSE_DTL CD   WITH(nolock)  ON Studenthdrid = CD.studenthdrfkid  and CD.IsCurrent=1 inner join STU_ADDRESS_DTL  ADR on  HDR.StudentHdrID =ADR.StudentHdrFKID  AND ADR.AdrType = 'CORS' and ADR.isactive=1  inner join PRD_MST_DISTRICT DT on ADR.DistrictFKID = DT.DistrictID inner join PRD_MST_CITY CT on  CT.CityID = ADR.CityFKID");
                            //sql.Append(" INNER JOIN PRD_MST_STATE AS ST (NOLOCK)  ON ST.StateID = ADR.StateFKID INNER JOIN PRD_MST_COURSE C WITH(nolock)   ON C.Courseid = cd.Coursefkid  LEFT JOIN PRD_MST_SYLLABUS Sy WITH(nolock)  ON Sy.Syllabusid = CD.SyllabusFKID INNER JOIN Prd_mst_office Chapter (NOLOCK)  on  Chapter.officeID = hdr.ChapterFkid   INNER JOIN  Prd_mst_office Region (NOLOCK)  on Region.OfficeID = Chapter.ParentOfficeFKID and  Chapter.OfficeTypeFKID =(select OfficeTypeID from prd_mst_officeType where OfficeTypeCode='CPT') WHERE    HDR.ActionFKID IN (1,23)   and HDR.UserFKID=");
                            sql.Append(" INNER JOIN PRD_MST_STATE AS ST (NOLOCK)  ON ST.StateID = ADR.StateFKID INNER JOIN PRD_MST_COURSE C WITH(nolock)   ON C.Courseid = cd.Coursefkid  LEFT JOIN PRD_MST_SYLLABUS Sy WITH(nolock)  ON Sy.Syllabusid = CD.SyllabusFKID INNER JOIN Prd_mst_office Chapter (NOLOCK)  on  Chapter.officeID = hdr.ChapterFkid   INNER JOIN  Prd_mst_office Region (NOLOCK)  on Region.OfficeID = Chapter.ParentOfficeFKID and  Chapter.OfficeTypeFKID =(select OfficeTypeID from prd_mst_officeType where OfficeTypeCode='CPT') WHERE 1=1 AND (((CD.StuCourseStatusFKID=2 and CD.Coursefkid>1) and HDR.ActionFKID =18 and CD.IsCurrent = 1) or HDR.ActionFKID in(1,23)) and HDR.UserFKID=");
                            sql.Append(userFKID);
                            /*sql.Append("select  Distinct Region.OfficeName as Region,Chapter.OfficeName, hdr.StudentHdrID,hdr.UserFKID,hdr.OldRegistrationNumber,HDR.RegistrationNumber AS RegistrationNumber,hdr.coursefkid,Sy.SyllabusName, CD.RegistrationDate,HDR.FirstName + ' ' + HDR.MiddleName + ' ' + HDR.LastName  AS [StudentName],ADR.AdrLine1,ADR.AdrLine2,ADR.AdrLine3, CT.CityName, DT.DistrictName,ST.StateName,ADR.PinCode,     hdr.Mobile,hdr.EmailID");
                            sql.Append(" FROM  [192.168.2.62].smash.dbo.STU_STUDENT_HDR HDR WITH(nolock) INNER JOIN [192.168.2.62].smash.dbo.STU_STUDENT_COURSE_DTL CD   WITH(nolock)  ON Studenthdrid = CD.studenthdrfkid  and CD.IsCurrent=1 inner join [192.168.2.62].smash.dbo.STU_ADDRESS_DTL  ADR on  HDR.StudentHdrID =ADR.StudentHdrFKID  AND ADR.AdrType = 'CORS' and ADR.isactive=1  ");
                            sql.Append("inner join [192.168.2.62].smash.dbo.PRD_MST_DISTRICT DT on ADR.DistrictFKID = DT.DistrictID inner join [192.168.2.62].smash.dbo.PRD_MST_CITY CT on  CT.CityID = ADR.CityFKID");
                            sql.Append(" INNER JOIN [192.168.2.62].smash.dbo.PRD_MST_STATE AS ST (NOLOCK)  ON ST.StateID = ADR.StateFKID INNER JOIN [192.168.2.62].smash.dbo.PRD_MST_COURSE C WITH(nolock)   ON C.Courseid = cd.Coursefkid  ");
                            sql.Append("LEFT JOIN [192.168.2.62].smash.dbo.PRD_MST_SYLLABUS Sy WITH(nolock)  ON Sy.Syllabusid = CD.SyllabusFKID INNER JOIN [192.168.2.62].smash.dbo.Prd_mst_office Chapter (NOLOCK)  on  Chapter.officeID = hdr.ChapterFkid   INNER JOIN  [192.168.2.62].smash.dbo.Prd_mst_office Region (NOLOCK)  on Region.OfficeID = Chapter.ParentOfficeFKID and  Chapter.OfficeTypeFKID =(select OfficeTypeID from [192.168.2.62].smash.dbo.prd_mst_officeType where OfficeTypeCode='CPT') WHERE    HDR.ActionFKID IN (1,23)   and HDR.UserFKID=");
                            sql.Append(userFKID);*/

                            List<Dictionary<string, object>> l1 = (List<Dictionary<string, object>>)Util.UtilService.GetSQLData(icsiConn, sql.ToString());
                            if (l1 != null && l1.Count > 0)
                            {
                                Dictionary<string, object> dict1 = l1[0];
                                userData = new Dictionary<string, object>();
                                userData.Add("USER_ID", id);
                                SqlConnection trainConn = (SqlConnection)_databases["Training"].CreateOpenConnection();
                                l = (List<Dictionary<string, object>>)Util.UtilService.GetData(trainConn, "STUDENT_T", userData, 0, 0);
                                int stid = 0;
                                dict = null;
                                if (l != null && l.Count > 0)
                                {
                                    dict = l[0];
                                    //dict1.Remove("RegistrationDate");
                                    stid = Convert.ToInt32(dict["ID"]);
                                    studentHdrID = Convert.ToInt32(dict["STUDENT_HDR_ID"]);
                                    
                                    dict = new Dictionary<string, object>();
                                    dict["ID"] = stid;
                                    dict["STUDENT_NAME_TX"] = dict1["StudentName"];                                    
                                    dict["ADDR_LINE_1"] = dict1["AdrLine1"];
                                    dict["ADDR_LINE_2"] = dict1["AdrLine2"];
                                    dict["ADDR_LINE_3"] = dict1["AdrLine3"];
                                    dict["CITY_NAME_TX"] = dict1["CityName"];
                                    dict["DISTRICT_NAME_TX"] = dict1["DistrictName"];
                                    dict["STATE_NAME_TX"] = dict1["StateName"];
                                    dict["PIN_CODE_TX"] = dict1["PinCode"];
                                    dict["MOBILE_TX"] = Convert.ToString(dict1["Mobile"]);
                                    dict["EMAIL_ID"] = dict1["EmailID"];
                                    dict["DOB_DT"]= dict1["DOB"];
                                    dict["REG_NUMBER_TX"] = dict1["RegistrationNumber"];

                                    userdata = new List<Dictionary<string, object>>();
                                    userdata.Add(dict);
                                    Util.UtilService.InsertData(trainConn, "", "", "STUDENT_T", userdata, 1);
                                }
                                else
                                {
                                    studentHdrID = Convert.ToInt32(dict1["StudentHdrID"]);
                                    dict = new Dictionary<string, object>();
                                    dict["REGION_TX"] = dict1["Region"];
                                    dict["OFFICE_NAME_TX"] = dict1["OfficeName"];
                                    dict["STUDENT_HDR_ID"] = dict1["StudentHdrID"];
                                    dict["USER_FKID"] = dict1["UserFKID"];
                                    dict["OLD_REG_NUMBER_TX"] = Convert.ToString(dict1["OldRegistrationNumber"]);
                                    dict["REG_NUMBER_TX"] = Convert.ToString(dict1["RegistrationNumber"]);
                                    dict["COURSE_FKID"] = dict1["coursefkid"];
                                    dict["SYLLABUS_NAME_TX"] = dict1["SyllabusName"];
                                    dict["REG_DT"] = dict1["RegistrationDate"];
                                    dict["STUDENT_NAME_TX"] = dict1["StudentName"];
                                    dict["ADDR_LINE_1"] = dict1["AdrLine1"];
                                    dict["ADDR_LINE_2"] = dict1["AdrLine2"];
                                    dict["ADDR_LINE_3"] = dict1["AdrLine3"];
                                    dict["CITY_NAME_TX"] = dict1["CityName"];
                                    dict["DISTRICT_NAME_TX"] = dict1["DistrictName"];
                                    dict["STATE_NAME_TX"] = dict1["StateName"];
                                    dict["PIN_CODE_TX"] = dict1["PinCode"];
                                    dict["MOBILE_TX"] = Convert.ToString(dict1["Mobile"]);
                                    dict["EMAIL_ID"] = dict1["EmailID"];
                                    dict["DOB_DT"] = dict1["DOB"];
                                    dict["USER_ID"] = id;
                                    //if (dict.ContainsKey("STUDENT_TRAINING_ID")) dict.Remove("STUDENT_TRAINING_ID");
                                    userdata = new List<Dictionary<string, object>>();
                                    userdata.Add(dict);
                                    //l = (List<Dictionary<string, object>>)Util.UtilService.GetData(trainConn, "STUDENT_T", userData, 0, 0);
                                    //if (l != null && l.Count > 0) dict["ID"]=l[0]["ID"];
                                    id = Util.UtilService.InsertData(trainConn, "", "", "STUDENT_T", userdata, 1);
                                }
                                if (trainConn.State == ConnectionState.Open) trainConn.Close();
                                //if (stid > 0) dict["ID"] = stid;
                                //dict["STUDENT_TRAINING_ID"] = id;
                                //id = Util.UtilService.InsertData(sqlConnection, "", "", "STUDENT_T", userdata, 1);
                            }
                        }

                    }
                    else if (dt.Rows[0]["Result"].ToString().Equals("3"))
                    {
                        Dictionary<string, object> dict = l[0];
                        dict["ACTIVE_YN"] = 0;
                        List<Dictionary<string, object>> userdata = new List<Dictionary<string, object>>();
                        userdata.Add(dict);
                        int id = Util.UtilService.InsertData(sqlConnection, "", "", "USER_T", userdata, 1);
                    }
                }
                if (icsiConn.State == ConnectionState.Open) icsiConn.Close();
                //string strSaltKy = SaltKeyHandler.SaltKeyGet(ref ObjSaltKey);
                //string lStrDBPwd = getMD5Hash(core_hmac_md5(Strpwdhash, strSaltKy));
                return isIcsiDataExists;
            }
            catch (Exception exxxx)
            {
                return false;
            }
        }

        //As per new New CS regulation 2020 
        public int checkValidStudent(int StudentHdrId)
        {
            int result = 0;
            string PROF_PASSED_QRY = "SELECT ActionFKID,STU_STUDENT_HDR.RegistrationNumber,StudentHdrFKID FROM STU_STUDENT_COURSE_DTL JOIN STU_STUDENT_HDR ON STU_STUDENT_HDR.StudentHdrId = STU_STUDENT_COURSE_DTL.StudentHdrFkId WHERE StuCourseStatusFKID = 2 AND IsCurrent = 1 AND STU_STUDENT_COURSE_DTL.CourseFKID = 3 AND CAST(STU_STUDENT_HDR.ExpiredOn AS DATE) < CAST(GETDATE() AS DATE) AND ((STU_STUDENT_COURSE_DTL.[Year] = 2019 and STU_STUDENT_COURSE_DTL.[Session] = 12) OR stu_student_course_dtl.year > 2019) AND STU_STUDENT_HDR.StudentHdrId=" + StudentHdrId + "";
            string PROF_EXEC_PROF_ONGOING_QRY = "SELECT ActionFKID,STU_STUDENT_HDR.RegistrationNumber,StudentHdrFKID FROM STU_STUDENT_COURSE_DTL JOIN STU_STUDENT_HDR ON STU_STUDENT_HDR.StudentHdrId = STU_STUDENT_COURSE_DTL.StudentHdrFkId WHERE StuCourseStatusFKID<> 2 AND IsCurrent = 1 AND STU_STUDENT_COURSE_DTL.CourseFKID in (2, 3) AND CAST(STU_STUDENT_HDR.ExpiredOn AS DATE) < CAST(GETDATE() AS DATE) AND STU_STUDENT_HDR.StudentHdrId=" + StudentHdrId + "";
            SqlConnection icsiConn = (SqlConnection)_databases["ICSI"].CreateOpenConnection();

            try
            {
                List<Dictionary<string, object>> l1 = (List<Dictionary<string, object>>)Util.UtilService.GetSQLData(icsiConn, PROF_PASSED_QRY);
                if (l1 != null && l1.Count > 0)
                    result = 1;
                else
                {
                    l1 = (List<Dictionary<string, object>>)Util.UtilService.GetSQLData(icsiConn, PROF_EXEC_PROF_ONGOING_QRY);
                    if (l1 != null && l1.Count > 0)
                        result = 2;
                }
            }
            catch (Exception ex) { }
            finally
            {
                if (icsiConn.State == ConnectionState.Open) icsiConn.Close();
            }
            return result;
        }

        [HttpPost]
        [Route("login")]
        public ActionResult<string> reqlogin(object json)
        {
            string Message = string.Empty;
            string Session_key = string.Empty;
            int statCode = 0;
            string pwd = null;
            List<Dictionary<string, object>> jldata = new List<Dictionary<string, object>>();
            if (json != null)
            {
                JObject req = null;
                SqlConnection sqlConnection = null;
                try
                {
                    req = JObject.Parse(json.ToString());
                    if (req != null && req.HasValues && req.ContainsKey("sid") && req.ContainsKey("data"))
                    {
                        Dictionary<string, object> userData = new Dictionary<string, object>();
                        string sid = req.GetValue("sid").ToString();
                        string edata = req.GetValue("data").ToString();
                        int StudentHdrId = 0;

                        userData.Add("LOGIN_ID", sid);
                        userData.Add("ACTIVE_YN", 1);
                        sqlConnection = (SqlConnection)_database.CreateOpenConnection();
                        bool isDataExists = fetchICSILoginDetails(sqlConnection, sid, out StudentHdrId);

                        int result = 0;
                        if (StudentHdrId != 0)
                        {
                            result = checkValidStudent(StudentHdrId);
                            if (result == 1)
                            {
                                statCode = (int)Util.UtilService.ICSICodes.PROFESSIONAL_INVALID;
                                Message = Util.UtilService.GetDisplayName(Util.UtilService.ICSICodes.PROFESSIONAL_INVALID);
                            }
                            else if (result == 2)
                            {
                                statCode = (int)Util.UtilService.ICSICodes.PROF_EXEC_ONGOING;
                                Message = Util.UtilService.GetDisplayName(Util.UtilService.ICSICodes.PROF_EXEC_ONGOING);
                            }
                        }

                        if (result == 0)
                        {
                            List<Dictionary<string, object>> l = (List<Dictionary<string, object>>)Util.UtilService.GetData(sqlConnection, "USER_T", userData, 0, 0);
                            if (l != null && l.Count >= 1)
                            {
                                Dictionary<string, object> user = l[0];
                                try
                                {
                                    pwd = PASSWORD_TYPE == 0 ? DEFAULT_PASSWORD : (string)user["LOGIN_PWD_TX"];
                                    string data = Util.CryptographyUtil.DecryptDataPKCS7(edata, pwd);
                                    if (data != null)
                                    {
                                        JObject jdata = JObject.Parse(data);
                                        if (jdata.ContainsKey("type") && jdata.GetValue("type").ToString().Equals("login"))
                                        {
                                            string type = jdata.GetValue("type").ToString();
                                            if (Convert.ToInt32(user["USER_TYPE_ID"]) == 4)
                                            {
                                                Session_key = pwd;
                                            }
                                            else
                                                Session_key = Util.UtilService.NewSessionKey(sqlConnection);
                                            string qry = "UPDATE USER_T SET SESSION_KEY='" + Session_key + "' WHERE ID=" + user["ID"];
                                            SqlCommand cmd = Util.UtilService.CreateCommand(qry, sqlConnection);
                                            cmd.ExecuteNonQuery();
                                            user["SESSION_KEY"] = Session_key;
                                            Message = Convert.ToString(Util.UtilService.ICSICodes.Success);
                                            statCode = (int)Util.UtilService.ICSICodes.Success;
                                            jldata.Add(user);
                                        }
                                        else
                                        {
                                            statCode = (int)Util.UtilService.ICSICodes.Wrong_password;
                                            Message = Util.UtilService.GetDisplayName(Util.UtilService.ICSICodes.Wrong_password);
                                        }
                                    }
                                    else
                                    {
                                        // encrypted data is null
                                        statCode = (int)Util.UtilService.ICSICodes.Wrong_password;
                                        Message = Util.UtilService.GetDisplayName(Util.UtilService.ICSICodes.Wrong_password);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    statCode = (int)Util.UtilService.ICSICodes.Wrong_password;
                                    Message = Util.UtilService.GetDisplayName(Util.UtilService.ICSICodes.Wrong_password);
                                }
                            }
                            else
                            {
                                statCode = (int)Util.UtilService.ICSICodes.INVALID_USER;
                                Message = Util.UtilService.GetDisplayName(Util.UtilService.ICSICodes.INVALID_USER);
                            }
                        }
                    }
                    else
                    {
                        // the sid and data are not present in the json
                        statCode = (int)Util.UtilService.ICSICodes.Wrong_JSON_Format;
                        Message = Util.UtilService.GetDisplayName(Util.UtilService.ICSICodes.Wrong_JSON_Format);
                    }
                }
                catch (Exception ex)
                {
                    statCode = (int)Util.UtilService.ICSICodes.Invalid_JSON_Request;
                    Message = Util.UtilService.GetDisplayName(Util.UtilService.ICSICodes.Invalid_JSON_Request);
                }
                if (sqlConnection != null) sqlConnection.Close();
                sqlConnection = null;
            }
            else
            {
                // there is no json or data
                statCode = (int)Util.UtilService.ICSICodes.Invalid_Request_Arguments;
                Message = Util.UtilService.GetDisplayName(Util.UtilService.ICSICodes.Invalid_Request_Arguments);
            }
            return Util.UtilService.BuildResponse(pwd, statCode.ToString(), Message, jldata);
        }
    }
}