using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data;
using System.Web.Mvc;
using System.Text;
using System.Configuration;
using System.IO;
using System.Data.SqlClient;
using ICSI_WebApp.Models;

namespace ICSI_WebApp.BusinessLayer
{
    public class COPLayer
    {
        private string Prod_Stimulate = Convert.ToString(ConfigurationManager.AppSettings["PStimulate"]);
        private string Prod_Training_Stimulate = Convert.ToString(ConfigurationManager.AppSettings["PTraining_Stimulate"]);
        SqlConnection sCon;
        SqlCommand sCmd;
        SqlDataAdapter sDa;

        //public string Validate_COP_First_Step(int Student_Id, int MemberNo)
        //{
        //    string status = string.Empty;
        //    try
        //    {
        //        sCon = new SqlConnection(Prod_Training_Stimulate);
        //        sCmd = new SqlCommand("SELECT * FROM MEM_FCS_REG_BASIC_T WHERE ACTIVE_YN=1 AND MEMNO_NM=@MEMNO_NM", sCon);
        //        sCmd.Parameters.Add("@MEMNO_NM", SqlDbType.Int).Value = MemberNo;
        //        DataTable dt = new DataTable();
        //        sDa = new SqlDataAdapter(sCmd);
        //        sDa.Fill(dt);

        //        if (dt != null)
        //        {
        //            if (dt.Rows.Count > 0)
        //            {
        //                if (dt.Rows[0]["FEE_AMOUNT"].ToString() != "")
        //                {
        //                    if (Convert.ToDecimal(dt.Rows[0]["FEE_AMOUNT"].ToString()) > 0)
        //                        status = "SUCCESS";
        //                    else
        //                        status = "FEE";
        //                }
        //                else
        //                    status = "FEE";
        //            }
        //            else
        //                status = "INACTIVE";
        //        }
        //        else
        //            status = "INACTIVE";
        //    }
        //    catch (Exception ex)
        //    {
        //        status = "ERROR";
        //    }
        //    finally
        //    {
        //        if (sCon != null)
        //        {
        //            if (sCon.State == ConnectionState.Open)
        //                sCon.Close();
        //        }
        //    }
        //    return status;
        //}

        public COP_Details_Model Validate_COP_First_Step(int User_Id, int MemberNo)
        {
            COP_Details_Model obj = new COP_Details_Model();
            string status = string.Empty;
            try
            {
                sCon = new SqlConnection(Prod_Training_Stimulate);
                sCmd = new SqlCommand("SELECT FIRST_NAME_TX,MIDDLE_NAME_TX,LAST_NAME_TX,FATHER_HUSB_NAME_TX,MOBILE_NO_TX,EMAIL_ID_TX,"
                    + "STUDENT_ID,WEBSITE_TX,PROF_ADD_LINE1_TX FROM MEM_ACS_MEMBERSHIP_REG_T A JOIN MEMBERS_T M ON A.ID = M.ACS_REG_ID "
                    + "AND A.ACTIVE_YN = 1 AND M.ACTIVE_YN = 1 WHERE M.MEMBNO_TX = @MEMNO_NM", sCon);
                sCmd.Parameters.Add("@MEMNO_NM", SqlDbType.Int).Value = MemberNo;
                DataTable dt = new DataTable();
                sDa = new SqlDataAdapter(sCmd);
                sDa.Fill(dt);

                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        obj.Validation_Status = "SUCCESS";
                        //status = "SUCCESS";
                    }
                    else
                    {
                        obj.Validation_Status = "INACTIVE";
                        //status = "INACTIVE";
                    }
                }
                else
                {
                    obj.Validation_Status = "INACTIVE";
                    //status = "INACTIVE";
                }

                sCon = new SqlConnection(Prod_Training_Stimulate);
                sCmd = new SqlCommand("SELECT * FROM MEM_COP_DETAILS WHERE USER_ID=@USER_ID AND ACTIVE_YN=1 ORDER  BY ID DESC", sCon);
                sCmd.Parameters.Add("@USER_ID", SqlDbType.BigInt).Value = User_Id;
                DataTable dt1 = new DataTable();
                sDa = new SqlDataAdapter(sCmd);
                sDa.Fill(dt1);

                if (dt1 != null)
                {
                    if (dt1.Rows.Count > 0)
                    {
                        obj.Application_Status = dt1.Rows[0]["APPLICATION_STATUS_ID"].ToString();
                    }
                }

            }
            catch (Exception ex)
            {
                status = "ERROR";
            }
            finally
            {
                if (sCon != null)
                {
                    if (sCon.State == ConnectionState.Open)
                        sCon.Close();
                }
            }
            return obj;
        }

        public string Insert_COPIssue_Last_Employment_Details(string Company_Name, string Designation, string Joining_Date,
            string Last_End_Date, string Employment_Duration, int UserId, string MemberShip_No)
        {
            string status = string.Empty;
            try
            {
                sCon = new SqlConnection(Prod_Training_Stimulate);
                sCmd = new SqlCommand("Insert into MEM_COP_LAST_EMPLOYEE_DETAILS_T(USER_ID,MEMBNO_TX,COMPNAY_NAME_TX,DESIGNATION_TX,JOINING_DT,"
                    + "LAST_END_DT,DURATION_OF_EMPLOYEEMENT_TX,ACTIVE_YN,CREATED_DT,CREATED_BY,UPDATED_DT,UPDATED_BY) "
                    + "values(@USER_ID,@MEMBNO_TX,@COMPNAY_NAME_TX,@DESIGNATION_TX,@JOINING_DT,@LAST_END_DT,@DURATION_OF_EMPLOYEEMENT_TX,"
                    + "@ACTIVE_YN,@CREATED_DT,@CREATED_BY,@UPDATED_DT,@UPDATED_BY)", sCon);

                sCmd.Parameters.Add("@USER_ID", SqlDbType.BigInt).Value = UserId;
                sCmd.Parameters.Add("@MEMBNO_TX", SqlDbType.VarChar, 20).Value = MemberShip_No;

                sCmd.Parameters.Add("@COMPNAY_NAME_TX", SqlDbType.VarChar, 200).Value = Company_Name;
                sCmd.Parameters.Add("@DESIGNATION_TX", SqlDbType.VarChar, 100).Value = Designation;
                sCmd.Parameters.Add("@JOINING_DT", SqlDbType.Date).Value = Joining_Date;
                sCmd.Parameters.Add("@LAST_END_DT", SqlDbType.Date).Value = Last_End_Date;
                sCmd.Parameters.Add("@DURATION_OF_EMPLOYEEMENT_TX", SqlDbType.VarChar, 100).Value = Employment_Duration;

                sCmd.Parameters.Add("@ACTIVE_YN", SqlDbType.Bit).Value = true;
                sCmd.Parameters.Add("@CREATED_DT", SqlDbType.DateTime).Value = DateTime.Now;
                sCmd.Parameters.Add("@CREATED_BY", SqlDbType.Int).Value = UserId;
                sCmd.Parameters.Add("@UPDATED_DT", SqlDbType.DateTime).Value = DateTime.Now;
                sCmd.Parameters.Add("@UPDATED_BY", SqlDbType.Int).Value = UserId;

                sCon.Open();
                int i = sCmd.ExecuteNonQuery();
                if (i >= 0)
                    status = "SUCCESS";
                else
                    status = "ERROR";
            }
            catch (Exception ex)
            {
                status = "ERROR";
            }
            finally
            {
                if (sCon != null)
                {
                    if (sCon.State == ConnectionState.Open)
                        sCon.Close();
                }
            }
            return status;
        }

        public COP_Issue_Personal_Details_Model Personal_Details_Load(string User_Id, string Membership_No)
        {
            string status = string.Empty;
            COP_Issue_Personal_Details_Model objModel = new COP_Issue_Personal_Details_Model();
            List<DropDownMappingsModel> lst_ECSIN_Exempted = new List<DropDownMappingsModel>();
            try
            {
                sCon = new SqlConnection(Prod_Training_Stimulate);
                sCmd = new SqlCommand("SELECT ID,CATEGORY_NAME_TX FROM MEM_EXEMPTED_ECSIN_MASTER_T WHERE ACTIVE_YN=1 ORDER BY CATEGORY_NAME_TX ASC", sCon);
                DataTable dt = new DataTable();
                sDa = new SqlDataAdapter(sCmd);
                sDa.Fill(dt);

                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        DropDownMappingsModel obj;
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            obj = new DropDownMappingsModel();
                            obj.Id = Convert.ToInt32(dt.Rows[i]["ID"].ToString());
                            obj.Name = dt.Rows[i]["CATEGORY_NAME_TX"].ToString();
                            lst_ECSIN_Exempted.Add(obj);
                        }
                    }
                }

                objModel.lst_ECSIN_Exempted = lst_ECSIN_Exempted;

                sCmd = new SqlCommand("SELECT * FROM MEM_COP_DETAILS WHERE USER_ID=@USER_ID AND ACTIVE_YN=1", sCon);
                sCmd.Parameters.Add("@USER_ID", SqlDbType.BigInt).Value = Convert.ToInt64(User_Id);
                DataTable dt1 = new DataTable();
                sDa = new SqlDataAdapter(sCmd);
                sDa.Fill(dt1);

                if (dt1 != null)
                {
                    if (dt1.Rows.Count > 0)
                    {
                        objModel.MEM_COP_DETAILS_ID = Convert.ToInt32(dt1.Rows[0]["ID"].ToString());
                        if (dt1.Rows[0]["ECSIN_NOT_APPLICABLE_YN"].ToString().ToUpper() == "TRUE")
                            objModel.ECSIN_NOT_APPLICABLE_YN = 1;
                        if (dt1.Rows[0]["ECSIN_EXEMPTED"].ToString().ToUpper() == "TRUE")
                            objModel.ECSIN_EXEMPTED = 1;
                        objModel.ECSIN_EXEMPTION_CATERGORY_ID = Convert.ToInt32(dt1.Rows[0]["ECSIN_EXEMPTION_CATERGORY_ID"].ToString());

                        objModel.Application_Status_Id = Convert.ToInt32(dt1.Rows[0]["Application_Status_Id"].ToString());
                    }
                }

                sCmd = new SqlCommand("SELECT * FROM MEM_COP_LAST_EMPLOYEE_DETAILS_T WHERE USER_ID=@USER_ID AND ACTIVE_YN=1", sCon);
                sCmd.Parameters.Add("@USER_ID", SqlDbType.BigInt).Value = Convert.ToInt64(User_Id);
                DataTable dtLastEmp = new DataTable();
                sDa = new SqlDataAdapter(sCmd);
                sDa.Fill(dtLastEmp);

                if (dtLastEmp != null)
                {
                    if (dtLastEmp.Rows.Count > 0)
                    {
                        List<COP_Issue_Last_Employee_Details_Model> lst_Last_Employee_Details = new List<COP_Issue_Last_Employee_Details_Model>();
                        COP_Issue_Last_Employee_Details_Model obj;
                        for (int i = 0; i < dtLastEmp.Rows.Count; i++)
                        {
                            obj = new COP_Issue_Last_Employee_Details_Model();
                            obj.COMPNAY_NAME_TX = dtLastEmp.Rows[i]["COMPNAY_NAME_TX"].ToString();
                            obj.DIR_TX = dtLastEmp.Rows[i]["DIR_TX"].ToString();
                            obj.ECSIN_NO_TX = dtLastEmp.Rows[i]["ECSIN_NO_TX"].ToString();
                            obj.DESIGNATION_TX = dtLastEmp.Rows[i]["DESIGNATION_TX"].ToString();
                            obj.JOINING_DT = dtLastEmp.Rows[i]["JOINING_DT"].ToString();
                            obj.LAST_END_DT = dtLastEmp.Rows[i]["LAST_END_DT"].ToString();
                            obj.DURATION_OF_EMPLOYEEMENT_TX = dtLastEmp.Rows[i]["DURATION_OF_EMPLOYEEMENT_TX"].ToString();

                            lst_Last_Employee_Details.Add(obj);
                        }

                        objModel.lst_Last_Employee_Details = lst_Last_Employee_Details;
                    }
                }

                sCmd = new SqlCommand("SELECT * FROM MEM_COP_APPROVE_PROCESS_DETAILS_T where ACTIVE_YN=1 and COP_DETAILS_ID=@COP_DETAILS_ID", sCon);
                sCmd.Parameters.Add("@COP_DETAILS_ID", SqlDbType.BigInt).Value = objModel.MEM_COP_DETAILS_ID;
                DataTable dtProcess = new DataTable();
                sDa = new SqlDataAdapter(sCmd);
                sDa.Fill(dtProcess);

                if (dtProcess != null)
                {
                    if (dtProcess.Rows.Count > 0)
                    {
                        objModel.Application_Process_Status_Id = Convert.ToInt32(dtProcess.Rows[dtProcess.Rows.Count - 1]["Application_Status_Id"].ToString());
                    }
                }

                sCmd = new SqlCommand("Select FIRST_NAME_TX,MIDDLE_NAME_TX,LAST_NAME_TX,FATHER_HUSB_NAME_TX,MOBILE_NO_TX,EMAIL_ID_TX,STUDENT_ID,"
                    + "WEBSITE_TX,PROF_ADD_LINE1_TX from MEM_ACS_MEMBERSHIP_REG_T A join MEMBERS_T M on A.id = M.acs_reg_id and A.active_YN = 1 "
                    + "and M.Active_YN = 1 where M.membno_tx = @MEMBERSHIP_NO", sCon);
                sCmd.Parameters.Add("@MEMBERSHIP_NO", SqlDbType.VarChar, 20).Value = Membership_No;
                DataTable dtMemDetails = new DataTable();
                sDa = new SqlDataAdapter(sCmd);
                sDa.Fill(dtMemDetails);

                if (dtMemDetails != null)
                {
                    if (dtMemDetails.Rows.Count > 0)
                    {
                        objModel.Member_Name = dtMemDetails.Rows[0]["FIRST_NAME_TX"].ToString() + " " + dtMemDetails.Rows[0]["MIDDLE_NAME_TX"].ToString() + " " + dtMemDetails.Rows[0]["LAST_NAME_TX"].ToString();
                        objModel.FATHER_HUSB_NAME_TX = dtMemDetails.Rows[0]["FATHER_HUSB_NAME_TX"].ToString();
                        objModel.MOBILE_NO_TX = dtMemDetails.Rows[0]["MOBILE_NO_TX"].ToString();
                        objModel.EMAIL_ID_TX = dtMemDetails.Rows[0]["EMAIL_ID_TX"].ToString();
                        objModel.WEBSITE_TX = dtMemDetails.Rows[0]["WEBSITE_TX"].ToString();
                        objModel.Membership_No = Membership_No;
                        objModel.Address = dtMemDetails.Rows[0]["PROF_ADD_LINE1_TX"].ToString();
                    }
                }

                //// Document details
                sCmd = new SqlCommand("SELECT MAS.ID AS ID,DOCUMENT_TYPE_TX ,MAS.FILE_NAME_TX,MAS.FILE_PATH,"
                    + "(CONVERT(VARCHAR, MAS.UPDATED_DT , 23)) AS UPLODED,MAS.APPROVE_NM FROM DOCUMENT_TYPE_T INNER join MEM_COP_ISSUE_DOCUMENT_TYPE_MASTER_T MAS "
                    + " ON DOCUMENT_TYPE_T.ID = MAS.DOCUMENT_TYPE_ID WHERE REMOVE_NM = 0 AND MAS.REF_ID = @USER_ID", sCon);
                sCmd.Parameters.Add("@USER_ID", SqlDbType.BigInt).Value = Convert.ToInt64(User_Id);
                DataTable dtdoc = new DataTable();
                sDa = new SqlDataAdapter(sCmd);
                sDa.Fill(dtdoc);

                if (dtdoc != null)
                {
                    if (dtdoc.Rows.Count > 0)
                    {
                        List<COP_Issue_Document_Details_Model> lst_Document_Details = new List<COP_Issue_Document_Details_Model>();
                        COP_Issue_Document_Details_Model obj;
                        for (int i = 0; i < dtdoc.Rows.Count; i++)
                        {
                            obj = new COP_Issue_Document_Details_Model();
                            obj.ID = Convert.ToInt32(dtdoc.Rows[i]["ID"].ToString());
                            obj.DOCUMENT_TYPE_TX = dtdoc.Rows[i]["DOCUMENT_TYPE_TX"].ToString();
                            obj.FILE_NAME_TX = dtdoc.Rows[i]["FILE_NAME_TX"].ToString();
                            obj.FILE_PATH = dtdoc.Rows[i]["FILE_PATH"].ToString();
                            obj.UPLODED = dtdoc.Rows[i]["UPLODED"].ToString();
                            obj.Approved_Id = Convert.ToInt32(dtdoc.Rows[i]["APPROVE_NM"].ToString());

                            lst_Document_Details.Add(obj);
                        }

                        objModel.lst_Document_Details = lst_Document_Details;
                    }
                }

            }
            catch (Exception ex)
            {
                status = "ERROR";
            }
            finally
            {
                if (sCon != null)
                {
                    if (sCon.State == ConnectionState.Open)
                        sCon.Close();
                }
            }
            return objModel;
        }

        public string Insert_Personal_Details_Of_COP(string User_Id, int Id, string ECSIN_EXEMPTION_CATERGORY_ID,
            string ECSIN_NOT_APPLICABLE_YN, string ECSIN_EXEMPTED, string Membership_No)
        {
            string status = string.Empty;
            try
            {
                sCon = new SqlConnection(Prod_Training_Stimulate);
                if (Id == 0)
                {
                    int FinYearId = 0;
                    //string FinYear = (DateTime.Now.Month <= 3 ? DateTime.Now.Year - 1 : DateTime.Now.Year).ToString() + "-" + (DateTime.Now.Month >= 4 ? DateTime.Now.Year + 1 : DateTime.Now.Year).ToString().Substring(2, 2);
                    string FinYear = (DateTime.Now.Month <= 3 ? DateTime.Now.Year - 1 : DateTime.Now.Year).ToString() + "-" + (DateTime.Now.Month >= 4 ? DateTime.Now.Year + 1 : DateTime.Now.Year).ToString();
                    sCmd = new SqlCommand("SELECT ID FROM FINANCIAL_YEAR_T WHERE FINANCIAL_YEAR_TX=@FINANCIAL_YEAR_TX AND ACTIVE_YN=1", sCon);
                    sCmd.Parameters.Add("@FINANCIAL_YEAR_TX", SqlDbType.VarChar, 20).Value = FinYear;
                    sCon.Open();
                    object obj = sCmd.ExecuteScalar();
                    if (obj != null)
                    {
                        FinYearId = Convert.ToInt32(obj.ToString());
                    }

                    sCmd = new SqlCommand("INSERT INTO MEM_COP_DETAILS(USER_ID,MEMBNO_TX,ECSIN_NOT_APPLICABLE_YN,ECSIN_EXEMPTED,ECSIN_EXEMPTION_CATERGORY_ID,"
                        + "REQUEST_TYPE_ID,APPLICATION_STATUS_ID,FIN_YEAR_ID, ACTIVE_YN, CREATED_DT, CREATED_BY, UPDATED_DT, UPDATED_BY) "
                        + "VALUES(@USER_ID,@MEMBNO_TX, @ECSIN_NOT_APPLICABLE_YN, @ECSIN_EXEMPTED, @ECSIN_EXEMPTION_CATERGORY_ID, @REQUEST_TYPE_ID,"
                        + "@APPLICATION_STATUS_ID, @FIN_YEAR_ID, @ACTIVE_YN, @CREATED_DT, @CREATED_BY, @UPDATED_DT, @UPDATED_BY)", sCon);

                    sCmd.Parameters.Add("@USER_ID", SqlDbType.BigInt).Value = Convert.ToInt64(User_Id);
                    sCmd.Parameters.Add("@MEMBNO_TX", SqlDbType.VarChar, 20).Value = Membership_No;
                    sCmd.Parameters.Add("@ECSIN_NOT_APPLICABLE_YN", SqlDbType.Int).Value = ECSIN_NOT_APPLICABLE_YN;
                    sCmd.Parameters.Add("@ECSIN_EXEMPTED", SqlDbType.Int).Value = ECSIN_EXEMPTED;
                    sCmd.Parameters.Add("@ECSIN_EXEMPTION_CATERGORY_ID", SqlDbType.Int).Value = ECSIN_EXEMPTION_CATERGORY_ID;
                    sCmd.Parameters.Add("@REQUEST_TYPE_ID", SqlDbType.VarChar, 20).Value = 1;
                    sCmd.Parameters.Add("@APPLICATION_STATUS_ID", SqlDbType.VarChar, 50).Value = 1;
                    sCmd.Parameters.Add("@FIN_YEAR_ID", SqlDbType.Int).Value = FinYearId;

                    sCmd.Parameters.Add("@ACTIVE_YN", SqlDbType.Bit).Value = true;
                    sCmd.Parameters.Add("@CREATED_DT", SqlDbType.DateTime).Value = DateTime.Now;
                    sCmd.Parameters.Add("@CREATED_BY", SqlDbType.Int).Value = Convert.ToInt32(User_Id);
                    sCmd.Parameters.Add("@UPDATED_DT", SqlDbType.DateTime).Value = DateTime.Now;
                    sCmd.Parameters.Add("@UPDATED_BY", SqlDbType.Int).Value = Convert.ToInt32(User_Id);

                    int i = sCmd.ExecuteNonQuery();
                    if (i >= 0)
                        status = "SUCCESS";
                    else
                        status = "ERROR";
                }
                else
                {
                    sCmd = new SqlCommand("UPDATE MEM_COP_DETAILS SET ECSIN_NOT_APPLICABLE_YN=@ECSIN_NOT_APPLICABLE_YN,"
                        + "ECSIN_EXEMPTED=@ECSIN_EXEMPTED,ECSIN_EXEMPTION_CATERGORY_ID=@ECSIN_EXEMPTION_CATERGORY_ID WHERE ID = @ID", sCon);

                    sCmd.Parameters.Add("@ID", SqlDbType.Int).Value = Id;
                    sCmd.Parameters.Add("@ECSIN_NOT_APPLICABLE_YN", SqlDbType.Int).Value = ECSIN_NOT_APPLICABLE_YN;
                    sCmd.Parameters.Add("@ECSIN_EXEMPTED", SqlDbType.Int).Value = ECSIN_EXEMPTED;
                    sCmd.Parameters.Add("@ECSIN_EXEMPTION_CATERGORY_ID", SqlDbType.Int).Value = ECSIN_EXEMPTION_CATERGORY_ID;

                    sCon.Open();
                    int i = sCmd.ExecuteNonQuery();
                    if (i >= 0)
                        status = "SUCCESS";
                    else
                        status = "ERROR";
                }
            }
            catch (Exception ex)
            {
                status = "ERROR";
            }
            finally
            {
                if (sCon != null)
                {
                    if (sCon.State == ConnectionState.Open)
                        sCon.Close();
                }
            }
            return status;
        }

        public string Submit_CallFor_Docs(string User_Id, int Id)
        {
            string status = string.Empty;
            try
            {
                sCon = new SqlConnection(Prod_Training_Stimulate);
                if (Id != 0)
                {
                    sCmd = new SqlCommand("UPDATE MEM_COP_DETAILS SET APPLICATION_STATUS_ID=2 WHERE ID = @ID", sCon);
                    sCmd.Parameters.Add("@ID", SqlDbType.Int).Value = Id;
                    sCon.Open();
                    sCmd.ExecuteNonQuery();

                    //Process table
                    sCmd = new SqlCommand("INSERT INTO MEM_COP_APPROVE_PROCESS_DETAILS_T(COP_DETAILS_ID,APPLICATION_STATUS_ID,"
                    + "ROLE_ID,ROLE_NAME,NEXT_ROLE_ID,NEXT_ROLE_NAME,INTERNAL_REMARKS,REMARKS_FOR_MEMBER,"
                    + "ACTIVE_YN, CREATED_DT, CREATED_BY, UPDATED_DT, UPDATED_BY) "
                    + "values(@COP_DETAILS_ID,@APPLICATION_STATUS_ID,"
                    + "@ROLE_ID,@ROLE_NAME,@NEXT_ROLE_ID,@NEXT_ROLE_NAME,@INTERNAL_REMARKS,@REMARKS_FOR_MEMBER,"
                    + "@ACTIVE_YN,@CREATED_DT,@CREATED_BY,@UPDATED_DT,@UPDATED_BY)", sCon);

                    sCmd.Parameters.Add("@COP_DETAILS_ID", SqlDbType.Int).Value = Id;

                    ////sCmd.Parameters.Add("@USER_ID", SqlDbType.BigInt).Value = obj.USER_ID;
                    sCmd.Parameters.Add("@APPLICATION_STATUS_ID", SqlDbType.Int).Value = 2;
                    sCmd.Parameters.Add("@ROLE_ID", SqlDbType.Int).Value = 20;
                    sCmd.Parameters.Add("@ROLE_NAME", SqlDbType.VarChar, 200).Value = "Member";
                    sCmd.Parameters.Add("@NEXT_ROLE_ID", SqlDbType.Int).Value = 16;
                    sCmd.Parameters.Add("@NEXT_ROLE_NAME", SqlDbType.VarChar, 200).Value = "Dealing Assistant";
                    sCmd.Parameters.Add("@INTERNAL_REMARKS", SqlDbType.VarChar, 500).Value = "";
                    sCmd.Parameters.Add("@REMARKS_FOR_MEMBER", SqlDbType.VarChar, 500).Value = "";

                    sCmd.Parameters.Add("@ACTIVE_YN", SqlDbType.Bit).Value = true;
                    sCmd.Parameters.Add("@CREATED_DT", SqlDbType.DateTime).Value = DateTime.Now;
                    sCmd.Parameters.Add("@CREATED_BY", SqlDbType.Int).Value = User_Id;
                    sCmd.Parameters.Add("@UPDATED_DT", SqlDbType.DateTime).Value = DateTime.Now;
                    sCmd.Parameters.Add("@UPDATED_BY", SqlDbType.Int).Value = User_Id;

                    int i = sCmd.ExecuteNonQuery();
                    if (i >= 0)
                        status = "SUCCESS";
                    else
                        status = "ERROR";
                }
            }
            catch (Exception ex)
            {
                status = "ERROR";
            }
            finally
            {
                if (sCon != null)
                {
                    if (sCon.State == ConnectionState.Open)
                        sCon.Close();
                }
            }
            return status;
        }


        #region Particualrs Of COP
        public Particulars_Of_COP_Model Particulars_Of_COP_Load(string User_Id)
        {
            string status = string.Empty;
            Particulars_Of_COP_Model objModel = new Particulars_Of_COP_Model();
            List<DropDownMappingsModel> lst_Area_Of_Practice = new List<DropDownMappingsModel>();
            try
            {
                sCon = new SqlConnection(Prod_Training_Stimulate);
                sCmd = new SqlCommand("SELECT * FROM MEM_AREA_OF_PRACTICE_T WHERE ACTIVE_YN=1", sCon);
                DataTable dt = new DataTable();
                sDa = new SqlDataAdapter(sCmd);
                sDa.Fill(dt);

                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        DropDownMappingsModel obj;
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            obj = new DropDownMappingsModel();
                            obj.Id = Convert.ToInt32(dt.Rows[i]["ID"].ToString());
                            obj.Name = dt.Rows[i]["AOP_DESC_T"].ToString();
                            lst_Area_Of_Practice.Add(obj);
                        }
                    }
                }

                objModel.lst_Area_Of_Practice = lst_Area_Of_Practice;

                sCmd = new SqlCommand("SELECT * FROM MEM_COP_PARTICULARS_T WHERE USER_ID=@USER_ID AND ACTIVE_YN=1", sCon);
                sCmd.Parameters.Add("@USER_ID", SqlDbType.BigInt).Value = Convert.ToInt64(User_Id);
                DataTable dt1 = new DataTable();
                sDa = new SqlDataAdapter(sCmd);
                sDa.Fill(dt1);

                if (dt1 != null)
                {
                    if (dt1.Rows.Count > 0)
                    {
                        objModel.COP_PARTICULARS_ID = Convert.ToInt32(dt1.Rows[0]["ID"].ToString());
                        objModel.Area_Of_Practice_Ids = dt1.Rows[0]["AREA_OF_PRACTICE_ID"].ToString();
                        objModel.Other_Service = dt1.Rows[0]["OTHER_SERVICE"].ToString();

                    }
                }

                sCmd = new SqlCommand("SELECT * FROM MEM_COP_DETAILS WHERE USER_ID=@USER_ID AND ACTIVE_YN=1", sCon);
                sCmd.Parameters.Add("@USER_ID", SqlDbType.BigInt).Value = Convert.ToInt64(User_Id);
                DataTable dtCOP = new DataTable();
                sDa = new SqlDataAdapter(sCmd);
                sDa.Fill(dtCOP);

                if (dtCOP != null)
                {
                    if (dtCOP.Rows.Count > 0)
                    {
                        objModel.Application_Status_Id = Convert.ToInt32(dtCOP.Rows[0]["Application_Status_Id"].ToString());
                    }
                }

            }
            catch (Exception ex)
            {
                status = "ERROR";
            }
            finally
            {
                if (sCon != null)
                {
                    if (sCon.State == ConnectionState.Open)
                        sCon.Close();
                }
            }
            return objModel;
        }

        public string Insert_Particulars_Of_COP(int Id, string Practice_Ids, string Other_Service, string MemberNo, string User_Id)
        {
            string status = string.Empty;
            try
            {
                sCon = new SqlConnection(Prod_Training_Stimulate);
                if (Id == 0)
                {
                    sCmd = new SqlCommand("Insert into MEM_COP_PARTICULARS_T(USER_ID,MEM_NO,AREA_OF_PRACTICE_ID,OTHER_SERVICE,"
                        + "ACTIVE_YN,CREATED_DT,CREATED_BY,UPDATED_DT,UPDATED_BY) "
                        + "values(@USER_ID,@MEM_NO,@AREA_OF_PRACTICE_ID,@OTHER_SERVICE,"
                        + "@ACTIVE_YN,@CREATED_DT,@CREATED_BY,@UPDATED_DT,@UPDATED_BY)", sCon);

                    sCmd.Parameters.Add("@USER_ID", SqlDbType.BigInt).Value = Convert.ToInt64(User_Id);
                    sCmd.Parameters.Add("@MEM_NO", SqlDbType.VarChar, 20).Value = MemberNo;
                    sCmd.Parameters.Add("@AREA_OF_PRACTICE_ID", SqlDbType.VarChar, 50).Value = Practice_Ids;
                    sCmd.Parameters.Add("@OTHER_SERVICE", SqlDbType.VarChar, 200).Value = Other_Service;

                    sCmd.Parameters.Add("@ACTIVE_YN", SqlDbType.Bit).Value = true;
                    sCmd.Parameters.Add("@CREATED_DT", SqlDbType.DateTime).Value = DateTime.Now;
                    sCmd.Parameters.Add("@CREATED_BY", SqlDbType.Int).Value = Convert.ToInt32(User_Id);
                    sCmd.Parameters.Add("@UPDATED_DT", SqlDbType.DateTime).Value = DateTime.Now;
                    sCmd.Parameters.Add("@UPDATED_BY", SqlDbType.Int).Value = Convert.ToInt32(User_Id);

                    sCon.Open();
                    int i = sCmd.ExecuteNonQuery();
                    if (i >= 0)
                        status = "SUCCESS";
                    else
                        status = "ERROR";
                }
                else
                {
                    sCmd = new SqlCommand("UPDATE MEM_COP_PARTICULARS_T SET AREA_OF_PRACTICE_ID=@AREA_OF_PRACTICE_ID,"
                        + "OTHER_SERVICE=@OTHER_SERVICE WHERE ID=@ID AND USER_ID=@USER_ID AND ACTIVE_YN=1", sCon);

                    sCmd.Parameters.Add("@AREA_OF_PRACTICE_ID", SqlDbType.VarChar, 50).Value = Practice_Ids;
                    sCmd.Parameters.Add("@OTHER_SERVICE", SqlDbType.VarChar, 200).Value = Other_Service;
                    sCmd.Parameters.Add("@ID", SqlDbType.Int).Value = Id;
                    sCmd.Parameters.Add("@USER_ID", SqlDbType.BigInt).Value = Convert.ToInt64(User_Id);

                    sCon.Open();
                    int i = sCmd.ExecuteNonQuery();
                    if (i >= 0)
                        status = "SUCCESS";
                    else
                        status = "ERROR";
                }
            }
            catch (Exception ex)
            {
                status = "ERROR";
            }
            finally
            {
                if (sCon != null)
                {
                    if (sCon.State == ConnectionState.Open)
                        sCon.Close();
                }
            }
            return status;
        }
        #endregion


        #region Other Institutions
        public Other_Institutions_Model COP_Issue_Other_Institution_Details_Load(string User_Id)
        {
            string status = string.Empty;
            Other_Institutions_Model objModel = new Other_Institutions_Model();
            List<Firm_Details_Model> lst_IOID_Firm_Details = new List<Firm_Details_Model>();
            try
            {
                sCon = new SqlConnection(Prod_Training_Stimulate);
                sCmd = new SqlCommand("SELECT F.FIRM_TYPE_TX,FD.* FROM MEM_COP_REG_FIRM_DTL_T FD "
                    + "JOIN FIRM_T F ON F.ID=FD.FIRM_TYPE_ID WHERE FD.USER_ID=@USER_ID AND FD.ACTIVE_YN=1", sCon);
                sCmd.Parameters.Add("@USER_ID", SqlDbType.BigInt).Value = Convert.ToInt64(User_Id);
                DataTable dt = new DataTable();
                sDa = new SqlDataAdapter(sCmd);
                sDa.Fill(dt);

                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        Firm_Details_Model obj;
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            obj = new Firm_Details_Model();
                            obj.ID = Convert.ToInt32(dt.Rows[i]["ID"].ToString());
                            obj.FIRM_TYPE_ID = Convert.ToInt32(dt.Rows[i]["FIRM_TYPE_ID"].ToString());
                            obj.FIRM_TYPE_TX = dt.Rows[i]["FIRM_TYPE_TX"].ToString();
                            obj.FIRM_NAME_TX = dt.Rows[i]["FIRM_NAME_TX"].ToString();
                            obj.ADDRESS1_TX = dt.Rows[i]["ADDRESS1_TX"].ToString();
                            obj.ADDRESS2_TX = dt.Rows[i]["ADDRESS2_TX"].ToString();
                            obj.ADDRESS3_TX = dt.Rows[i]["ADDRESS3_TX"].ToString();

                            obj.COUNTRY_ID = Convert.ToInt32(dt.Rows[i]["COUNTRY_ID"].ToString());
                            obj.STATE_ID = Convert.ToInt32(dt.Rows[i]["STATE_ID"].ToString());
                            obj.DISTRICT_ID = Convert.ToInt32(dt.Rows[i]["DISTRICT_ID"].ToString());
                            obj.CITY_ID = Convert.ToInt32(dt.Rows[i]["CITY_ID"].ToString());
                            obj.PINCODE_NM = Convert.ToInt32(dt.Rows[i]["PINCODE_NM"].ToString());

                            obj.BUSINESS_NATURE_TX = dt.Rows[i]["BUSINESS_NATURE_TX"].ToString();
                            obj.NAME_OF_PARTNERS_TX = dt.Rows[i]["NAME_OF_PARTNERS_TX"].ToString();
                            obj.OTHER_DETAILS_TX = dt.Rows[i]["OTHER_DETAILS_TX"].ToString();

                            lst_IOID_Firm_Details.Add(obj);
                        }
                    }
                }
                objModel.lst_Firm_Details = lst_IOID_Firm_Details;

                sCmd = new SqlCommand("SELECT * FROM MEM_COP_OTHER_INSTITUTIONS WHERE USER_ID=@USER_ID AND ACTIVE_YN=1", sCon);
                sCmd.Parameters.Add("@USER_ID", SqlDbType.BigInt).Value = Convert.ToInt64(User_Id);
                DataTable dt1 = new DataTable();
                sDa = new SqlDataAdapter(sCmd);
                sDa.Fill(dt1);
                if (dt1 != null)
                {
                    if (dt1.Rows.Count > 0)
                    {
                        objModel.ID = Convert.ToInt32(dt1.Rows[0]["ID"].ToString());

                        objModel.ICAI_MEMBERSHIP_NO_TX = dt1.Rows[0]["ICAI_MEMBERSHIP_NO_TX"].ToString();
                        if (Convert.ToBoolean(dt1.Rows[0]["IS_ICAI_MEMBER"].ToString()) == true)
                            objModel.IS_ICAI_MEMBER = 1;
                        else
                            objModel.IS_ICAI_MEMBER = 0;
                        if (Convert.ToBoolean(dt1.Rows[0]["IS_ICAI_MEMBER_HC"].ToString()) == true)
                            objModel.IS_ICAI_MEMBER_HC = 1;
                        else
                            objModel.IS_ICAI_MEMBER_HC = 0;

                        objModel.COST_MEMBERSHIP_NO_TX = dt1.Rows[0]["COST_MEMBERSHIP_NO_TX"].ToString();
                        if (Convert.ToBoolean(dt1.Rows[0]["IS_COST_MEMBERSHIP"].ToString()) == true)
                            objModel.IS_COST_MEMBERSHIP = 1;
                        else
                            objModel.IS_COST_MEMBERSHIP = 0;
                        if (Convert.ToBoolean(dt1.Rows[0]["COST_MEMBERSHIP_HC"].ToString()) == true)
                            objModel.COST_MEMBERSHIP_HC = 1;
                        else
                            objModel.COST_MEMBERSHIP_HC = 0;

                        objModel.BAR_COUNCIL_NO_TX = dt1.Rows[0]["BAR_COUNCIL_NO_TX"].ToString();
                        if (Convert.ToBoolean(dt1.Rows[0]["IS_BAR_COUNCIL"].ToString()) == true)
                            objModel.IS_BAR_COUNCIL = 1;
                        else
                            objModel.IS_BAR_COUNCIL = 0;
                        if (Convert.ToBoolean(dt1.Rows[0]["IS_BAR_COUNCIL_HC"].ToString()) == true)
                            objModel.IS_BAR_COUNCIL_HC = 1;
                        else
                            objModel.IS_BAR_COUNCIL_HC = 0;

                        objModel.PROF_INSTITUTE_NO_TX = dt1.Rows[0]["PROF_INSTITUTE_NO_TX"].ToString();
                        if (Convert.ToBoolean(dt1.Rows[0]["IS_PROF_INSTITUTE"].ToString()) == true)
                            objModel.IS_PROF_INSTITUTE = 1;
                        else
                            objModel.IS_PROF_INSTITUTE = 0;
                        if (Convert.ToBoolean(dt1.Rows[0]["IS_PROF_INSTITUTE_HC"].ToString()) == true)
                            objModel.IS_PROF_INSTITUTE_HC = 1;
                        else
                            objModel.IS_PROF_INSTITUTE_HC = 0;
                    }
                }

                sCmd = new SqlCommand("SELECT * FROM MEM_COP_DETAILS WHERE USER_ID=@USER_ID AND ACTIVE_YN=1", sCon);
                sCmd.Parameters.Add("@USER_ID", SqlDbType.BigInt).Value = Convert.ToInt64(User_Id);
                DataTable dtCOP = new DataTable();
                sDa = new SqlDataAdapter(sCmd);
                sDa.Fill(dtCOP);

                if (dtCOP != null)
                {
                    if (dtCOP.Rows.Count > 0)
                    {
                        objModel.Application_Status_Id = Convert.ToInt32(dtCOP.Rows[0]["Application_Status_Id"].ToString());
                    }
                }

            }
            catch (Exception ex)
            {
                status = "ERROR";
            }
            finally
            {
                if (sCon != null)
                {
                    if (sCon.State == ConnectionState.Open)
                        sCon.Close();
                }
            }
            return objModel;
        }

        public string Insert_COP_Other_Firm_Details(Firm_Details_Model obj)
        {
            string status = string.Empty;
            try
            {
                sCon = new SqlConnection(Prod_Training_Stimulate);
                sCmd = new SqlCommand("INSERT INTO MEM_COP_REG_FIRM_DTL_T(USER_ID,FIRM_TYPE_ID,FIRM_NAME_TX,ADDRESS1_TX,"
                    + "ADDRESS2_TX,ADDRESS3_TX,COUNTRY_ID,STATE_ID,DISTRICT_ID,CITY_ID,PINCODE_NM,BUSINESS_NATURE_TX,"
                    + "NAME_OF_PARTNERS_TX,OTHER_DETAILS_TX, ACTIVE_YN, CREATED_DT, CREATED_BY, UPDATED_DT, UPDATED_BY,"
                    + "REF_REF_ID, REMOVE_NM, REF_ID) "
                    + "values(@USER_ID,@FIRM_TYPE_ID,@FIRM_NAME_TX,@ADDRESS1_TX,"
                    + "@ADDRESS2_TX,@ADDRESS3_TX,@COUNTRY_ID,@STATE_ID,@DISTRICT_ID,@CITY_ID,@PINCODE_NM,@BUSINESS_NATURE_TX,"
                    + "@NAME_OF_PARTNERS_TX,@OTHER_DETAILS_TX,@ACTIVE_YN,@CREATED_DT,@CREATED_BY,@UPDATED_DT,@UPDATED_BY,"
                    + "@REF_REF_ID,@REMOVE_NM,@REF_ID)", sCon);

                sCmd.Parameters.Add("@USER_ID", SqlDbType.BigInt).Value = obj.USER_ID;
                sCmd.Parameters.Add("@FIRM_TYPE_ID", SqlDbType.Int).Value = obj.FIRM_TYPE_ID;
                sCmd.Parameters.Add("@FIRM_NAME_TX", SqlDbType.VarChar, 200).Value = obj.FIRM_NAME_TX;
                sCmd.Parameters.Add("@ADDRESS1_TX", SqlDbType.VarChar, 500).Value = obj.ADDRESS1_TX;
                sCmd.Parameters.Add("@ADDRESS2_TX", SqlDbType.VarChar, 500).Value = obj.ADDRESS2_TX;
                sCmd.Parameters.Add("@ADDRESS3_TX", SqlDbType.VarChar, 500).Value = obj.ADDRESS3_TX;
                sCmd.Parameters.Add("@COUNTRY_ID", SqlDbType.Int).Value = obj.COUNTRY_ID;
                sCmd.Parameters.Add("@STATE_ID", SqlDbType.Int).Value = obj.STATE_ID;
                sCmd.Parameters.Add("@DISTRICT_ID", SqlDbType.Int).Value = obj.DISTRICT_ID;
                sCmd.Parameters.Add("@CITY_ID", SqlDbType.Int).Value = obj.CITY_ID;
                sCmd.Parameters.Add("@PINCODE_NM", SqlDbType.Int).Value = obj.PINCODE_NM;
                sCmd.Parameters.Add("@BUSINESS_NATURE_TX", SqlDbType.VarChar, 250).Value = obj.BUSINESS_NATURE_TX;
                sCmd.Parameters.Add("@NAME_OF_PARTNERS_TX", SqlDbType.VarChar, 400).Value = obj.NAME_OF_PARTNERS_TX;
                sCmd.Parameters.Add("@OTHER_DETAILS_TX", SqlDbType.VarChar, 500).Value = obj.OTHER_DETAILS_TX;
                sCmd.Parameters.Add("@REF_REF_ID", SqlDbType.Int).Value = obj.REF_REF_ID;
                sCmd.Parameters.Add("@REMOVE_NM", SqlDbType.Int).Value = obj.REMOVE_NM;
                sCmd.Parameters.Add("@REF_ID", SqlDbType.Int).Value = obj.REF_ID;

                sCmd.Parameters.Add("@ACTIVE_YN", SqlDbType.Bit).Value = true;
                sCmd.Parameters.Add("@CREATED_DT", SqlDbType.DateTime).Value = DateTime.Now;
                sCmd.Parameters.Add("@CREATED_BY", SqlDbType.Int).Value = obj.USER_ID;
                sCmd.Parameters.Add("@UPDATED_DT", SqlDbType.DateTime).Value = DateTime.Now;
                sCmd.Parameters.Add("@UPDATED_BY", SqlDbType.Int).Value = obj.USER_ID;

                sCon.Open();
                int i = sCmd.ExecuteNonQuery();
                if (i >= 0)
                    status = "SUCCESS";
                else
                    status = "ERROR";
            }
            catch (Exception ex)
            {
                status = "ERROR";
            }
            finally
            {
                if (sCon != null)
                {
                    if (sCon.State == ConnectionState.Open)
                        sCon.Close();
                }
            }
            return status;
        }

        public string IOID_Delete_Firm_Details(int ID)
        {
            string status = string.Empty;
            try
            {
                sCon = new SqlConnection(Prod_Training_Stimulate);
                sCmd = new SqlCommand("UPDATE MEM_COP_REG_FIRM_DTL_T SET ACTIVE_YN=0 WHERE ID=@ID", sCon);
                sCmd.Parameters.Add("@ID", SqlDbType.Int).Value = ID;

                sCon.Open();
                int i = sCmd.ExecuteNonQuery();
                if (i >= 0)
                    status = "SUCCESS";
                else
                    status = "ERROR";
            }
            catch (Exception ex)
            {
                status = "ERROR";
            }
            finally
            {
                if (sCon != null)
                {
                    if (sCon.State == ConnectionState.Open)
                        sCon.Close();
                }
            }
            return status;
        }

        public string Insert_COP_Other_Institutions_Details(Other_Institutions_Model obj)
        {
            string status = string.Empty;
            try
            {
                sCon = new SqlConnection(Prod_Training_Stimulate);
                if (obj.ID == 0)
                {
                    sCmd = new SqlCommand("INSERT INTO MEM_COP_OTHER_INSTITUTIONS(USER_ID,IS_ICAI_MEMBER,ICAI_MEMBERSHIP_NO_TX,"
                    + "IS_ICAI_MEMBER_HC,IS_COST_MEMBERSHIP,COST_MEMBERSHIP_NO_TX,COST_MEMBERSHIP_HC,"
                    + "IS_BAR_COUNCIL,BAR_COUNCIL_NO_TX,IS_BAR_COUNCIL_HC,IS_PROF_INSTITUTE,PROF_INSTITUTE_NO_TX,"
                    + "IS_PROF_INSTITUTE_HC,ACTIVE_YN,CREATED_DT,CREATED_BY,UPDATED_DT,UPDATED_BY) "
                    + "values(@USER_ID,@IS_ICAI_MEMBER,@ICAI_MEMBERSHIP_NO_TX,"
                    + "@IS_ICAI_MEMBER_HC,@IS_COST_MEMBERSHIP,@COST_MEMBERSHIP_NO_TX,@COST_MEMBERSHIP_HC,"
                    + "@IS_BAR_COUNCIL,@BAR_COUNCIL_NO_TX,@IS_BAR_COUNCIL_HC,@IS_PROF_INSTITUTE,@PROF_INSTITUTE_NO_TX,"
                    + "@IS_PROF_INSTITUTE_HC,@ACTIVE_YN,@CREATED_DT,@CREATED_BY,@UPDATED_DT,@UPDATED_BY)", sCon);

                    sCmd.Parameters.Add("@USER_ID", SqlDbType.BigInt).Value = obj.USER_ID;

                    sCmd.Parameters.Add("@IS_ICAI_MEMBER", SqlDbType.Int).Value = obj.IS_ICAI_MEMBER;
                    sCmd.Parameters.Add("@ICAI_MEMBERSHIP_NO_TX", SqlDbType.VarChar, 50).Value = obj.ICAI_MEMBERSHIP_NO_TX;
                    sCmd.Parameters.Add("@IS_ICAI_MEMBER_HC", SqlDbType.Int).Value = obj.IS_ICAI_MEMBER_HC;

                    sCmd.Parameters.Add("@IS_COST_MEMBERSHIP", SqlDbType.Int).Value = obj.IS_COST_MEMBERSHIP;
                    sCmd.Parameters.Add("@COST_MEMBERSHIP_NO_TX", SqlDbType.VarChar, 50).Value = obj.COST_MEMBERSHIP_NO_TX;
                    sCmd.Parameters.Add("@COST_MEMBERSHIP_HC", SqlDbType.Int).Value = obj.COST_MEMBERSHIP_HC;

                    sCmd.Parameters.Add("@IS_BAR_COUNCIL", SqlDbType.Int).Value = obj.IS_BAR_COUNCIL;
                    sCmd.Parameters.Add("@BAR_COUNCIL_NO_TX", SqlDbType.VarChar, 50).Value = obj.BAR_COUNCIL_NO_TX;
                    sCmd.Parameters.Add("@IS_BAR_COUNCIL_HC", SqlDbType.Int).Value = obj.IS_BAR_COUNCIL_HC;

                    sCmd.Parameters.Add("@IS_PROF_INSTITUTE", SqlDbType.Int).Value = obj.IS_PROF_INSTITUTE;
                    sCmd.Parameters.Add("@PROF_INSTITUTE_NO_TX", SqlDbType.VarChar, 50).Value = obj.PROF_INSTITUTE_NO_TX;
                    sCmd.Parameters.Add("@IS_PROF_INSTITUTE_HC", SqlDbType.Int).Value = obj.IS_PROF_INSTITUTE_HC;

                    sCmd.Parameters.Add("@ACTIVE_YN", SqlDbType.Bit).Value = true;
                    sCmd.Parameters.Add("@CREATED_DT", SqlDbType.DateTime).Value = DateTime.Now;
                    sCmd.Parameters.Add("@CREATED_BY", SqlDbType.Int).Value = obj.USER_ID;
                    sCmd.Parameters.Add("@UPDATED_DT", SqlDbType.DateTime).Value = DateTime.Now;
                    sCmd.Parameters.Add("@UPDATED_BY", SqlDbType.Int).Value = obj.USER_ID;
                }
                else
                {
                    sCmd = new SqlCommand("UPDATE MEM_COP_OTHER_INSTITUTIONS SET IS_ICAI_MEMBER=@IS_ICAI_MEMBER,"
                        + "ICAI_MEMBERSHIP_NO_TX=@ICAI_MEMBERSHIP_NO_TX,IS_ICAI_MEMBER_HC=@IS_ICAI_MEMBER_HC,"
                        + "IS_COST_MEMBERSHIP=@IS_COST_MEMBERSHIP,COST_MEMBERSHIP_NO_TX=@COST_MEMBERSHIP_NO_TX,"
                        + "COST_MEMBERSHIP_HC=@COST_MEMBERSHIP_HC,IS_BAR_COUNCIL=@IS_BAR_COUNCIL,BAR_COUNCIL_NO_TX=@BAR_COUNCIL_NO_TX,"
                        + "IS_BAR_COUNCIL_HC=@IS_BAR_COUNCIL_HC,IS_PROF_INSTITUTE=@IS_PROF_INSTITUTE,PROF_INSTITUTE_NO_TX=@PROF_INSTITUTE_NO_TX,"
                        + "IS_PROF_INSTITUTE_HC=@IS_PROF_INSTITUTE_HC,UPDATED_DT=@UPDATED_DT,UPDATED_BY=@UPDATED_BY "
                        + "WHERE ID=@ID AND ACTIVE_YN=1", sCon);

                    sCmd.Parameters.Add("@ID", SqlDbType.BigInt).Value = obj.ID;

                    sCmd.Parameters.Add("@IS_ICAI_MEMBER", SqlDbType.Int).Value = obj.IS_ICAI_MEMBER;
                    sCmd.Parameters.Add("@ICAI_MEMBERSHIP_NO_TX", SqlDbType.VarChar, 50).Value = obj.ICAI_MEMBERSHIP_NO_TX;
                    sCmd.Parameters.Add("@IS_ICAI_MEMBER_HC", SqlDbType.Int).Value = obj.IS_ICAI_MEMBER_HC;

                    sCmd.Parameters.Add("@IS_COST_MEMBERSHIP", SqlDbType.Int).Value = obj.IS_COST_MEMBERSHIP;
                    sCmd.Parameters.Add("@COST_MEMBERSHIP_NO_TX", SqlDbType.VarChar, 50).Value = obj.COST_MEMBERSHIP_NO_TX;
                    sCmd.Parameters.Add("@COST_MEMBERSHIP_HC", SqlDbType.Int).Value = obj.COST_MEMBERSHIP_HC;

                    sCmd.Parameters.Add("@IS_BAR_COUNCIL", SqlDbType.Int).Value = obj.IS_BAR_COUNCIL;
                    sCmd.Parameters.Add("@BAR_COUNCIL_NO_TX", SqlDbType.VarChar, 50).Value = obj.BAR_COUNCIL_NO_TX;
                    sCmd.Parameters.Add("@IS_BAR_COUNCIL_HC", SqlDbType.Int).Value = obj.IS_BAR_COUNCIL_HC;

                    sCmd.Parameters.Add("@IS_PROF_INSTITUTE", SqlDbType.Int).Value = obj.IS_PROF_INSTITUTE;
                    sCmd.Parameters.Add("@PROF_INSTITUTE_NO_TX", SqlDbType.VarChar, 50).Value = obj.PROF_INSTITUTE_NO_TX;
                    sCmd.Parameters.Add("@IS_PROF_INSTITUTE_HC", SqlDbType.Int).Value = obj.IS_PROF_INSTITUTE_HC;

                    sCmd.Parameters.Add("@UPDATED_DT", SqlDbType.DateTime).Value = DateTime.Now;
                    sCmd.Parameters.Add("@UPDATED_BY", SqlDbType.Int).Value = obj.USER_ID;
                }

                sCon.Open();
                int i = sCmd.ExecuteNonQuery();
                if (i >= 0)
                    status = "SUCCESS";
                else
                    status = "ERROR";
            }
            catch (Exception ex)
            {
                status = "ERROR";
            }
            finally
            {
                if (sCon != null)
                {
                    if (sCon.State == ConnectionState.Open)
                        sCon.Close();
                }
            }
            return status;
        }

        #endregion

        #region Declaration
        public Declaration_Dependants_Model COP_Issue_Declaration_Details_Load(string User_Id, string MemberNo)
        {
            string status = string.Empty;
            Declaration_Dependants_Model objModel = new Declaration_Dependants_Model();
            List<Declaration_Dependants_Model> lst = new List<Declaration_Dependants_Model>();
            try
            {
                sCon = new SqlConnection(Prod_Training_Stimulate);
                sCmd = new SqlCommand("SELECT * FROM MEM_COP_DEPENDANTS_T WHERE USER_ID=@USER_ID AND ACTIVE_YN=1", sCon);
                sCmd.Parameters.Add("@USER_ID", SqlDbType.BigInt).Value = Convert.ToInt64(User_Id);
                DataTable dt = new DataTable();
                sDa = new SqlDataAdapter(sCmd);
                sDa.Fill(dt);

                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        Declaration_Dependants_Model obj;
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            obj = new Declaration_Dependants_Model();
                            obj.ID = Convert.ToInt32(dt.Rows[i]["ID"].ToString());
                            obj.NAME_TX = dt.Rows[i]["NAME_TX"].ToString();
                            obj.AGE_NM = Convert.ToInt32(dt.Rows[i]["AGE_NM"].ToString());
                            obj.RELATION_TO_SUBSCRIBER_TX = dt.Rows[i]["RELATION_TO_SUBSCRIBER_TX"].ToString();
                            obj.EMAIL_TX = dt.Rows[i]["EMAIL_TX"].ToString();
                            obj.MOBILE_TX = dt.Rows[i]["MOBILE_TX"].ToString();
                            obj.ADDRESS = dt.Rows[i]["ADDRESS"].ToString();

                            lst.Add(obj);
                        }
                    }
                }
                objModel.lst_Dependants_Details = lst;

                sCmd = new SqlCommand("SELECT FIRST_NAME_TX,MIDDLE_NAME_TX,LAST_NAME_TX,FATHER_HUSB_NAME_TX,MOBILE_NO_TX,EMAIL_ID_TX,"
                    + "STUDENT_ID,WEBSITE_TX,PROF_ADD_LINE1_TX FROM MEM_ACS_MEMBERSHIP_REG_T A JOIN MEMBERS_T M ON A.ID = M.ACS_REG_ID "
                    + "AND A.ACTIVE_YN = 1 AND M.ACTIVE_YN = 1 WHERE M.MEMBNO_TX = @MEMNO_NM", sCon);
                sCmd.Parameters.Add("@MEMNO_NM", SqlDbType.Int).Value = MemberNo;
                DataTable dt1 = new DataTable();
                sDa = new SqlDataAdapter(sCmd);
                sDa.Fill(dt1);

                if (dt1 != null)
                {
                    if (dt1.Rows.Count > 0)
                    {
                        objModel.Student_Name = dt1.Rows[0]["FIRST_NAME_TX"].ToString() + " " + dt1.Rows[0]["MIDDLE_NAME_TX"].ToString() + " " + dt1.Rows[0]["LAST_NAME_TX"].ToString();
                    }
                }

                sCmd = new SqlCommand("SELECT * FROM MEM_COP_DETAILS WHERE USER_ID=@USER_ID AND ACTIVE_YN=1", sCon);
                sCmd.Parameters.Add("@USER_ID", SqlDbType.BigInt).Value = Convert.ToInt64(User_Id);
                DataTable dtCOP = new DataTable();
                sDa = new SqlDataAdapter(sCmd);
                sDa.Fill(dtCOP);

                if (dtCOP != null)
                {
                    if (dtCOP.Rows.Count > 0)
                    {
                        objModel.Application_Status_Id = Convert.ToInt32(dtCOP.Rows[0]["Application_Status_Id"].ToString());
                    }
                }


            }
            catch (Exception ex)
            {
                status = "ERROR";
            }
            finally
            {
                if (sCon != null)
                {
                    if (sCon.State == ConnectionState.Open)
                        sCon.Close();
                }
            }
            return objModel;
        }

        public string Insert_COP_Declaration_Dependent(DataTable dtDetails)
        {
            string status = string.Empty;
            try
            {
                sCon = new SqlConnection(Prod_Training_Stimulate);
                if (dtDetails.Rows.Count > 0)
                {
                    sCon.Open();
                    for (int i = 0; i < dtDetails.Rows.Count; i++)
                    {
                        if (dtDetails.Rows[i]["ID"].ToString() == "0")
                        {
                            sCmd = new SqlCommand("INSERT INTO MEM_COP_DEPENDANTS_T(USER_ID,NAME_TX,AGE_NM,"
                                + "RELATION_TO_SUBSCRIBER_TX,EMAIL_TX,MOBILE_TX,ADDRESS,ACTIVE_YN,CREATED_DT,CREATED_BY,UPDATED_DT,UPDATED_BY) "
                                + "VALUES (@USER_ID,@NAME_TX,@AGE_NM,@RELATION_TO_SUBSCRIBER_TX,@EMAIL_TX,@MOBILE_TX,@ADDRESS,"
                                + "@ACTIVE_YN,@CREATED_DT,@CREATED_BY,@UPDATED_DT,@UPDATED_BY)", sCon);

                            sCmd.Parameters.Add("@USER_ID", SqlDbType.BigInt).Value = Convert.ToUInt64(dtDetails.Rows[i]["USER_ID"].ToString());

                            sCmd.Parameters.Add("@NAME_TX", SqlDbType.VarChar, 200).Value = dtDetails.Rows[i]["NAME_TX"].ToString();
                            sCmd.Parameters.Add("@AGE_NM", SqlDbType.Int).Value = dtDetails.Rows[i]["AGE_NM"].ToString();
                            sCmd.Parameters.Add("@RELATION_TO_SUBSCRIBER_TX", SqlDbType.VarChar, 50).Value = dtDetails.Rows[i]["RELATION_TO_SUBSCRIBER_TX"].ToString();
                            sCmd.Parameters.Add("@EMAIL_TX", SqlDbType.VarChar, 200).Value = dtDetails.Rows[i]["EMAIL_TX"].ToString();
                            sCmd.Parameters.Add("@MOBILE_TX", SqlDbType.VarChar, 20).Value = dtDetails.Rows[i]["MOBILE_TX"].ToString();
                            sCmd.Parameters.Add("@ADDRESS", SqlDbType.VarChar, 500).Value = dtDetails.Rows[i]["ADDRESS"].ToString();

                            sCmd.Parameters.Add("@ACTIVE_YN", SqlDbType.Bit).Value = true;
                            sCmd.Parameters.Add("@CREATED_DT", SqlDbType.DateTime).Value = DateTime.Now;
                            sCmd.Parameters.Add("@CREATED_BY", SqlDbType.Int).Value = Convert.ToUInt64(dtDetails.Rows[i]["USER_ID"].ToString());
                            sCmd.Parameters.Add("@UPDATED_DT", SqlDbType.DateTime).Value = DateTime.Now;
                            sCmd.Parameters.Add("@UPDATED_BY", SqlDbType.Int).Value = Convert.ToUInt64(dtDetails.Rows[i]["USER_ID"].ToString());

                            sCmd.ExecuteNonQuery();
                        }
                        else
                        {
                            sCmd = new SqlCommand("UPDATE MEM_COP_DEPENDANTS_T SET NAME_TX=@NAME_TX,AGE_NM=@AGE_NM,"
                                + "RELATION_TO_SUBSCRIBER_TX=@RELATION_TO_SUBSCRIBER_TX,EMAIL_TX=@EMAIL_TX,MOBILE_TX=@MOBILE_TX,"
                                + "ADDRESS=@ADDRESS,UPDATED_DT=@UPDATED_DT,UPDATED_BY=@UPDATED_BY WHERE ID=@ID", sCon);

                            sCmd.Parameters.Add("@ID", SqlDbType.Int).Value = dtDetails.Rows[i]["ID"].ToString();

                            sCmd.Parameters.Add("@NAME_TX", SqlDbType.VarChar, 200).Value = dtDetails.Rows[i]["NAME_TX"].ToString();
                            sCmd.Parameters.Add("@AGE_NM", SqlDbType.Int).Value = dtDetails.Rows[i]["AGE_NM"].ToString();
                            sCmd.Parameters.Add("@RELATION_TO_SUBSCRIBER_TX", SqlDbType.VarChar, 50).Value = dtDetails.Rows[i]["RELATION_TO_SUBSCRIBER_TX"].ToString();
                            sCmd.Parameters.Add("@EMAIL_TX", SqlDbType.VarChar, 200).Value = dtDetails.Rows[i]["EMAIL_TX"].ToString();
                            sCmd.Parameters.Add("@MOBILE_TX", SqlDbType.VarChar, 20).Value = dtDetails.Rows[i]["MOBILE_TX"].ToString();
                            sCmd.Parameters.Add("@ADDRESS", SqlDbType.VarChar, 500).Value = dtDetails.Rows[i]["ADDRESS"].ToString();

                            sCmd.Parameters.Add("@UPDATED_DT", SqlDbType.DateTime).Value = DateTime.Now;
                            sCmd.Parameters.Add("@UPDATED_BY", SqlDbType.Int).Value = Convert.ToUInt64(dtDetails.Rows[i]["USER_ID"].ToString());

                            sCmd.ExecuteNonQuery();
                        }
                    }


                }
                status = "SUCCESS";
            }
            catch (Exception ex)
            {
                status = "ERROR";
            }
            finally
            {
                if (sCon != null)
                {
                    if (sCon.State == ConnectionState.Open)
                        sCon.Close();
                }
            }
            return status;
        }
        #endregion


        public COP_Issue_Personal_Details_Model COP_ISSUE_PREVIEW_LOAD(string User_Id, string Membership_No)
        {
            string status = string.Empty;
            COP_Issue_Personal_Details_Model obj = new COP_Issue_Personal_Details_Model();
            DataSet ds = new DataSet();
            try
            {
                sCon = new SqlConnection(Prod_Training_Stimulate);
                sCmd = new SqlCommand("USP_MEM_COP_ISSUE_PREVIEW", sCon);
                sCmd.CommandType = CommandType.StoredProcedure;
                sCmd.Parameters.Add("@USER_ID", SqlDbType.BigInt).Value = Convert.ToInt64(User_Id);
                sCmd.Parameters.Add("@MEMBERSHIP_NO", SqlDbType.VarChar, 20).Value = Membership_No;
                sDa = new SqlDataAdapter(sCmd);
                sDa.Fill(ds);

                // Personal details
                if (ds.Tables[0].Rows.Count > 0)
                {
                    obj.Member_Name = ds.Tables[0].Rows[0]["FIRST_NAME_TX"].ToString() + " " + ds.Tables[0].Rows[0]["MIDDLE_NAME_TX"].ToString() + " " + ds.Tables[0].Rows[0]["LAST_NAME_TX"].ToString();
                    obj.FATHER_HUSB_NAME_TX = ds.Tables[0].Rows[0]["FATHER_HUSB_NAME_TX"].ToString();
                    obj.MOBILE_NO_TX = ds.Tables[0].Rows[0]["MOBILE_NO_TX"].ToString();
                    obj.EMAIL_ID_TX = ds.Tables[0].Rows[0]["EMAIL_ID_TX"].ToString();
                    obj.WEBSITE_TX = ds.Tables[0].Rows[0]["WEBSITE_TX"].ToString();
                    obj.Membership_No = Membership_No;
                }

                //Details of Orientation Programme
                if (ds.Tables[1].Rows.Count > 0)
                {
                    List<COP_Issue_Orientation_Details_Model> lstOri = new List<COP_Issue_Orientation_Details_Model>();
                    COP_Issue_Orientation_Details_Model ori;
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        ori = new COP_Issue_Orientation_Details_Model();
                        ori.ORGANIZED_BY = ds.Tables[1].Rows[i]["ORGANIZED_BY"].ToString();
                        ori.CERTIFICATE_NO_TX = ds.Tables[1].Rows[i]["CERTIFICATE_NO_TX"].ToString();
                        ori.FROM_DT = ds.Tables[1].Rows[i]["FROM_DT"].ToString();
                        ori.TO_DT = ds.Tables[1].Rows[i]["TO_DT"].ToString();
                        ori.DURATION_TX = ds.Tables[1].Rows[i]["DURATION_TX"].ToString();

                        lstOri.Add(ori);
                    }
                    obj.lst_Orientation_Details = lstOri;
                }

                //Details of last employee details
                if (ds.Tables[2].Rows.Count > 0)
                {
                    List<COP_Issue_Last_Employee_Details_Model> lstLE = new List<COP_Issue_Last_Employee_Details_Model>();
                    COP_Issue_Last_Employee_Details_Model ori;
                    for (int i = 0; i < ds.Tables[2].Rows.Count; i++)
                    {
                        ori = new COP_Issue_Last_Employee_Details_Model();
                        ori.COMPNAY_NAME_TX = ds.Tables[2].Rows[i]["COMPNAY_NAME_TX"].ToString();
                        ori.DIR_TX = ds.Tables[2].Rows[i]["DIR_TX"].ToString();
                        ori.ECSIN_NO_TX = ds.Tables[2].Rows[i]["ECSIN_NO_TX"].ToString();
                        ori.DESIGNATION_TX = ds.Tables[2].Rows[i]["DESIGNATION_TX"].ToString();
                        ori.JOINING_DT = ds.Tables[2].Rows[i]["JOINING_DT"].ToString();
                        ori.LAST_END_DT = ds.Tables[2].Rows[i]["LAST_END_DT"].ToString();
                        ori.DURATION_OF_EMPLOYEEMENT_TX = ds.Tables[2].Rows[i]["DURATION_OF_EMPLOYEEMENT_TX"].ToString();

                        lstLE.Add(ori);
                    }
                    obj.lst_Last_Employee_Details = lstLE;
                }

                //Details of document details
                if (ds.Tables[3].Rows.Count > 0)
                {
                    List<COP_Issue_Document_Details_Model> lstDoc = new List<COP_Issue_Document_Details_Model>();
                    COP_Issue_Document_Details_Model doc;
                    for (int i = 0; i < ds.Tables[3].Rows.Count; i++)
                    {
                        doc = new COP_Issue_Document_Details_Model();
                        doc.ID = Convert.ToInt32(ds.Tables[3].Rows[i]["ID"].ToString());
                        doc.DOCUMENT_TYPE_TX = ds.Tables[3].Rows[i]["DOCUMENT_TYPE_TX"].ToString();
                        doc.FILE_NAME_TX = ds.Tables[3].Rows[i]["FILE_NAME_TX"].ToString();
                        doc.FILE_PATH = ds.Tables[3].Rows[i]["FILE_PATH"].ToString();
                        doc.UPLODED = ds.Tables[3].Rows[i]["UPLODED"].ToString();
                        doc.Approved_Id = Convert.ToInt32(ds.Tables[3].Rows[i]["APPROVE_NM"].ToString());

                        lstDoc.Add(doc);
                    }
                    obj.lst_Document_Details = lstDoc;
                }

                //Details of Firm Details
                if (ds.Tables[4].Rows.Count > 0)
                {
                    List<Firm_Details_Model> lstDoc = new List<Firm_Details_Model>();
                    Firm_Details_Model doc;
                    for (int i = 0; i < ds.Tables[4].Rows.Count; i++)
                    {
                        doc = new Firm_Details_Model();
                        doc.FIRM_TYPE_TX = ds.Tables[4].Rows[i]["FIRM_TYPE_TX"].ToString();
                        doc.FIRM_NAME_TX = ds.Tables[4].Rows[i]["FIRM_NAME_TX"].ToString();
                        doc.BUSINESS_NATURE_TX = ds.Tables[4].Rows[i]["BUSINESS_NATURE_TX"].ToString();
                        doc.NAME_OF_PARTNERS_TX = ds.Tables[4].Rows[i]["NAME_OF_PARTNERS_TX"].ToString();
                        doc.OTHER_DETAILS_TX = ds.Tables[4].Rows[i]["OTHER_DETAILS_TX"].ToString();

                        lstDoc.Add(doc);
                    }
                    obj.lst_Firm_Details = lstDoc;
                }

                // Particulars of COP
                if (ds.Tables[5].Rows.Count > 0)
                {
                    obj.Area_Of_Practice_Id = ds.Tables[5].Rows[0]["Area_Of_Practice_Id"].ToString();
                    obj.Other_Service = ds.Tables[5].Rows[0]["Other_Service"].ToString();
                }

                //Area of Other institutions
                if (ds.Tables[6].Rows.Count > 0)
                {
                    List<Other_Institutions_Model> lst_Other_Institutions = new List<Other_Institutions_Model>();
                    Other_Institutions_Model oIST;
                    for (int i = 0; i < ds.Tables[6].Rows.Count; i++)
                    {
                        oIST = new Other_Institutions_Model();
                        oIST.ICAI_MEMBERSHIP_NO_TX = ds.Tables[6].Rows[i]["ICAI_MEMBERSHIP_NO_TX"].ToString();
                        if (Convert.ToBoolean(ds.Tables[6].Rows[i]["IS_ICAI_MEMBER"].ToString()) == true)
                            oIST.IS_ICAI_MEMBER = 1;
                        else
                            oIST.IS_ICAI_MEMBER = 0;
                        if (Convert.ToBoolean(ds.Tables[6].Rows[i]["IS_ICAI_MEMBER_HC"].ToString()) == true)
                            oIST.IS_ICAI_MEMBER_HC = 1;
                        else
                            oIST.IS_ICAI_MEMBER_HC = 0;

                        oIST.COST_MEMBERSHIP_NO_TX = ds.Tables[6].Rows[i]["COST_MEMBERSHIP_NO_TX"].ToString();
                        if (Convert.ToBoolean(ds.Tables[6].Rows[i]["IS_COST_MEMBERSHIP"].ToString()) == true)
                            oIST.IS_COST_MEMBERSHIP = 1;
                        else
                            oIST.IS_COST_MEMBERSHIP = 0;
                        if (Convert.ToBoolean(ds.Tables[6].Rows[i]["COST_MEMBERSHIP_HC"].ToString()) == true)
                            oIST.COST_MEMBERSHIP_HC = 1;
                        else
                            oIST.COST_MEMBERSHIP_HC = 0;

                        oIST.BAR_COUNCIL_NO_TX = ds.Tables[6].Rows[i]["BAR_COUNCIL_NO_TX"].ToString();
                        if (Convert.ToBoolean(ds.Tables[6].Rows[i]["IS_BAR_COUNCIL"].ToString()) == true)
                            oIST.IS_BAR_COUNCIL = 1;
                        else
                            oIST.IS_BAR_COUNCIL = 0;
                        if (Convert.ToBoolean(ds.Tables[6].Rows[i]["IS_BAR_COUNCIL_HC"].ToString()) == true)
                            oIST.IS_BAR_COUNCIL_HC = 1;
                        else
                            oIST.IS_BAR_COUNCIL_HC = 0;

                        oIST.PROF_INSTITUTE_NO_TX = ds.Tables[6].Rows[i]["PROF_INSTITUTE_NO_TX"].ToString();
                        if (Convert.ToBoolean(ds.Tables[6].Rows[i]["IS_PROF_INSTITUTE"].ToString()) == true)
                            oIST.IS_PROF_INSTITUTE = 1;
                        else
                            oIST.IS_PROF_INSTITUTE = 0;
                        if (Convert.ToBoolean(ds.Tables[6].Rows[i]["IS_PROF_INSTITUTE_HC"].ToString()) == true)
                            oIST.IS_PROF_INSTITUTE_HC = 1;
                        else
                            oIST.IS_PROF_INSTITUTE_HC = 0;

                        lst_Other_Institutions.Add(oIST);
                    }

                    obj.lst_Other_Institutions = lst_Other_Institutions;
                }

                //Area of practice dropdown details
                if (ds.Tables[7].Rows.Count > 0)
                {
                    List<DropDownMappingsModel> lst_Area_Of_Practice = new List<DropDownMappingsModel>();
                    DropDownMappingsModel oDDL;
                    for (int i = 0; i < ds.Tables[7].Rows.Count; i++)
                    {
                        oDDL = new DropDownMappingsModel();
                        oDDL.Id = Convert.ToInt32(ds.Tables[7].Rows[i]["ID"].ToString());
                        oDDL.Name = ds.Tables[7].Rows[i]["AOP_DESC_T"].ToString();
                        lst_Area_Of_Practice.Add(oDDL);
                    }
                    obj.lst_Area_Of_Practice = lst_Area_Of_Practice;
                }

                //COP details
                if (ds.Tables[8].Rows.Count > 0)
                {
                    obj.Application_Status_Id = Convert.ToInt32(ds.Tables[8].Rows[0]["Application_Status_Id"].ToString());

                    if (Convert.ToBoolean(ds.Tables[8].Rows[0]["ECSIN_NOT_APPLICABLE_YN"].ToString()) == true)
                        obj.ECSIN_NOT_APPLICABLE_YN = 1;
                    else
                        obj.ECSIN_NOT_APPLICABLE_YN = 0;
                }

            }
            catch (Exception ex)
            {
                status = "ERROR";
            }
            finally
            {
                if (sCon != null)
                {
                    if (sCon.State == ConnectionState.Open)
                        sCon.Close();
                }
            }
            return obj;
        }

        public COP_ISSUE_PAYMENT_MODEL COP_ISSUE_PAYMENT_LOAD(string User_Id, string Membership_No)
        {
            string status = string.Empty;
            COP_ISSUE_PAYMENT_MODEL objModel = new COP_ISSUE_PAYMENT_MODEL();
            try
            {
                sCon = new SqlConnection(Prod_Training_Stimulate);
                string curDate = DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day;
                sCmd = new SqlCommand("SELECT * FROM MEM_ACS_FEE_MASTER_T WHERE FEE_HEAD_ID=5 AND ACTIVE_YN=1 AND '" + curDate
                    + "'>=START_DT AND (END_DT is null or END_DT<='" + curDate + "')", sCon);
                DataTable dt = new DataTable();
                sDa = new SqlDataAdapter(sCmd);
                sDa.Fill(dt);

                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        objModel.FEE_AMOUNT = dt.Rows[0]["FEE_AMOUNT"].ToString();
                        if (Convert.ToBoolean(dt.Rows[0]["TAXABLE_YN"].ToString()) == true)
                            objModel.TAXABLE_YN = 1;
                        else
                            objModel.TAXABLE_YN = 0;
                    }
                }

                if (objModel.TAXABLE_YN == 1)
                {
                    sCmd = new SqlCommand("Select FIRST_NAME_TX,MIDDLE_NAME_TX,LAST_NAME_TX,FATHER_HUSB_NAME_TX,MOBILE_NO_TX,EMAIL_ID_TX,"
                        + "STUDENT_ID,WEBSITE_TX,PROF_ADD_LINE1_TX, RES_STATE_ID from MEM_ACS_MEMBERSHIP_REG_T A "
                        + "join MEMBERS_T M on A.id = M.acs_reg_id and A.active_YN = 1 and M.Active_YN = 1 where M.membno_tx = @MEMBERSHIP_NO", sCon);
                    sCmd.Parameters.Add("@MEMBERSHIP_NO", SqlDbType.VarChar, 20).Value = Membership_No;
                    DataTable dtMem = new DataTable();
                    sDa = new SqlDataAdapter(sCmd);
                    sDa.Fill(dtMem);

                    if (dtMem != null)
                    {
                        if (dtMem.Rows.Count > 0)
                        {
                            objModel.STATE_ID = Convert.ToInt32(dtMem.Rows[0]["RES_STATE_ID"].ToString());
                            ////268
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                status = "ERROR";
            }
            finally
            {
                if (sCon != null)
                {
                    if (sCon.State == ConnectionState.Open)
                        sCon.Close();
                }
            }
            return objModel;
        }

        public string COP_ISSUE_PROCESS_PAYMENT(string User_Id, string Membership_No)
        {
            string status = string.Empty;
            COP_ISSUE_PAYMENT_MODEL objModel = new COP_ISSUE_PAYMENT_MODEL();
            try
            {
                sCon = new SqlConnection(Prod_Training_Stimulate);
                string curDate = DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day;
                sCmd = new SqlCommand("SELECT * FROM MEM_ACS_FEE_MASTER_T WHERE FEE_HEAD_ID=5 AND ACTIVE_YN=1 AND '" + curDate
                    + "'>=START_DT AND (END_DT is null or END_DT<='" + curDate + "')", sCon);
                DataTable dt_COP = new DataTable();
                sDa = new SqlDataAdapter(sCmd);
                sDa.Fill(dt_COP);

                if (dt_COP != null)
                {
                    if (dt_COP.Rows.Count > 0)
                    {
                        objModel.FEE_AMOUNT = dt_COP.Rows[0]["FEE_AMOUNT"].ToString();
                        if (Convert.ToBoolean(dt_COP.Rows[0]["TAXABLE_YN"].ToString()) == true)
                            objModel.TAXABLE_YN = 1;
                        else
                            objModel.TAXABLE_YN = 0;
                    }
                }

                decimal _gstAmt = (Convert.ToDecimal(objModel.FEE_AMOUNT) * 18) / 100;

                sCmd = new SqlCommand("USP_COP_ISSUE_PROCESS_PAYMENT", sCon);
                sCmd.CommandType = CommandType.StoredProcedure;
                sCmd.Parameters.Add("@REQUEST_TYPE_ID", SqlDbType.Int).Value = 1;
                //sCmd.Parameters.Add("@REF_ID", SqlDbType.Int).Value = dt_COP.Rows[0]["ID"].ToString();
                sCmd.Parameters.Add("@TOTAL_RENEWAL_YEAR_NM", SqlDbType.Decimal).Value = 0;
                sCmd.Parameters.Add("@CONCESSION_TYPE_TX", SqlDbType.VarChar, 50).Value = "";
                sCmd.Parameters.Add("@CONCESSION_AMOUNT_NM", SqlDbType.Decimal).Value = 0;
                sCmd.Parameters.Add("@TOTAL_FEE_WO_GST", SqlDbType.Decimal).Value = dt_COP.Rows[0]["FEE_AMOUNT"].ToString();
                if (objModel.TAXABLE_YN == 1)
                {
                    decimal totAmount = Convert.ToDecimal(dt_COP.Rows[0]["FEE_AMOUNT"].ToString()) + _gstAmt;
                    sCmd.Parameters.Add("@GST_RATE_NM", SqlDbType.Decimal).Value = 18;
                    sCmd.Parameters.Add("@GST_AMOUNT", SqlDbType.Decimal).Value = _gstAmt;
                    sCmd.Parameters.Add("@TOTAL_FEE", SqlDbType.Decimal).Value = totAmount;
                }
                else
                {
                    sCmd.Parameters.Add("@GST_RATE_NM", SqlDbType.Decimal).Value = 0;
                    sCmd.Parameters.Add("@GST_AMOUNT", SqlDbType.Decimal).Value = 0;
                    sCmd.Parameters.Add("@TOTAL_FEE", SqlDbType.Decimal).Value = dt_COP.Rows[0]["FEE_AMOUNT"].ToString();
                }
                sCmd.Parameters.Add("@PURPOSE_TX", SqlDbType.VarChar, 200).Value = "COP Issue";
                sCmd.Parameters.Add("@User_Id", SqlDbType.Int).Value = Convert.ToInt32(User_Id);

                sCmd.Parameters.Add("@FEE_HEAD_ID", SqlDbType.Int).Value = dt_COP.Rows[0]["FEE_HEAD_ID"].ToString();
                sCmd.Parameters.Add("@FEE_HEAD_AMOUNT", SqlDbType.Decimal).Value = dt_COP.Rows[0]["FEE_AMOUNT"].ToString();

                sCmd.Parameters.Add("@Id", SqlDbType.Int).Direction = ParameterDirection.Output;
                sCmd.Parameters.Add("@message", SqlDbType.VarChar, 500).Value = ParameterDirection.Output;

                sCon.Open();
                sCmd.ExecuteNonQuery();

                int i = Convert.ToInt32(sCmd.Parameters["@Id"].Value);
                if (i > 0)
                    status = "SUCCESS";
                else
                    status = "ERROR";

                #region Old
                ////sCmd = new SqlCommand("insert into MEM_COP_PAYMENT_T values(REQUEST_TYPE_ID,REF_ID,TOTAL_RENEWAL_YEAR_NM,CONCESSION_TYPE_TX,CONCESSION_AMOUNT_NM,"
                ////    + "TOTAL_FEE_WO_GST,GST_RATE_NM,GST_AMOUNT,TOTAL_FEE,PURPOSE_TX,PAYMENT_STATUS_ID,TRANSACTION_ID,TRANSACTION_DT,RECIEPT_NO,"
                ////    + " ACTIVE_YN,CREATED_DT,CREATED_BY,UPDATED_DT,UPDATED_BY)"
                ////    + " values (@REQUEST_TYPE_ID,@REF_ID,@TOTAL_RENEWAL_YEAR_NM,@CONCESSION_TYPE_TX,@CONCESSION_AMOUNT_NM,@TOTAL_FEE_WO_GST,"
                ////    + "@GST_RATE_NM,@GST_AMOUNT,@TOTAL_FEE,@PURPOSE_TX,@PAYMENT_STATUS_ID,@TRANSACTION_ID,@TRANSACTION_DT,@RECIEPT_NO,"
                ////    + "@ACTIVE_YN,@CREATED_DT,@CREATED_BY,@UPDATED_DT,@UPDATED_BY)", sCon);

                ////sCmd.Parameters.Add("@REQUEST_TYPE_ID", SqlDbType.Int).Value = dt_COP.Rows[0]["REQUEST_TYPE_ID"].ToString();
                ////sCmd.Parameters.Add("@REF_ID", SqlDbType.Int).Value = dt_COP.Rows[0]["ID"].ToString();
                ////sCmd.Parameters.Add("@TOTAL_RENEWAL_YEAR_NM", SqlDbType.Decimal).Value = 0;
                ////sCmd.Parameters.Add("@CONCESSION_TYPE_TX", SqlDbType.VarChar, 50).Value = "";
                ////sCmd.Parameters.Add("@CONCESSION_AMOUNT_NM", SqlDbType.Decimal).Value = 0;
                ////sCmd.Parameters.Add("@TOTAL_FEE_WO_GST", SqlDbType.Decimal).Value = dt_COP.Rows[0]["FEE_AMOUNT"].ToString();
                ////if (objModel.TAXABLE_YN == 1)
                ////{
                ////    decimal totAmount = Convert.ToDecimal(dt_COP.Rows[0]["FEE_AMOUNT"].ToString()) + _gstAmt;
                ////    sCmd.Parameters.Add("@GST_RATE_NM", SqlDbType.Decimal).Value = 18;
                ////    sCmd.Parameters.Add("@GST_AMOUNT", SqlDbType.Decimal).Value = _gstAmt;
                ////    sCmd.Parameters.Add("@TOTAL_FEE", SqlDbType.Decimal).Value = totAmount;
                ////}
                ////else
                ////{
                ////    sCmd.Parameters.Add("@GST_RATE_NM", SqlDbType.Decimal).Value = 0;
                ////    sCmd.Parameters.Add("@GST_AMOUNT", SqlDbType.Decimal).Value = 0;
                ////    sCmd.Parameters.Add("@TOTAL_FEE", SqlDbType.Decimal).Value = dt_COP.Rows[0]["FEE_AMOUNT"].ToString();
                ////}
                ////sCmd.Parameters.Add("@PURPOSE_TX", SqlDbType.VarChar, 200).Value = "COP Issue";
                ////sCmd.Parameters.Add("@PAYMENT_STATUS_ID", SqlDbType.Int).Value = 0;
                ////sCmd.Parameters.Add("@TRANSACTION_ID", SqlDbType.VarChar, 200).Value = "";
                ////////sCmd.Parameters.Add("@TRANSACTION_DT", SqlDbType.).Value =;
                ////sCmd.Parameters.Add("@RECIEPT_NO", SqlDbType.VarChar, 100).Value = "";

                ////sCmd.Parameters.Add("@ACTIVE_YN", SqlDbType.Bit).Value = true;
                ////sCmd.Parameters.Add("@CREATED_DT", SqlDbType.DateTime).Value = DateTime.Now;
                ////sCmd.Parameters.Add("@CREATED_BY", SqlDbType.Int).Value = Convert.ToInt32(User_Id);
                ////sCmd.Parameters.Add("@UPDATED_DT", SqlDbType.DateTime).Value = DateTime.Now;
                ////sCmd.Parameters.Add("@UPDATED_BY", SqlDbType.Int).Value = Convert.ToInt32(User_Id);

                ////sCon.Open();
                ////int i = sCmd.ExecuteNonQuery();
                ////if (i > 0)
                ////{

                ////    sCmd = new SqlCommand("INSERT INTO MEM_COP_PAYMENT_DETAILS_T(PAYMENT_ID,FEE_HEAD_ID,FEE_HEAD_AMOUNT,FEE_HEAD_GST_AMOUNT,"
                ////        +"ACTIVE_YN,CREATED_DT,CREATED_BY,UPDATED_DT,UPDATED_BY) "
                ////        +"values(@PAYMENT_ID,@FEE_HEAD_ID,@FEE_HEAD_AMOUNT,@FEE_HEAD_GST_AMOUNT,@ACTIVE_YN,@CREATED_DT,@CREATED_BY,"
                ////        +"@UPDATED_DT,@UPDATED_BY)", sCon);

                ////    sCmd.Parameters.Add("@PAYMENT_ID", SqlDbType.Int).Value = true;
                ////    sCmd.Parameters.Add("@FEE_HEAD_ID", SqlDbType.Int).Value = true;
                ////    sCmd.Parameters.Add("@FEE_HEAD_AMOUNT", SqlDbType.Decimal).Value = true;
                ////    sCmd.Parameters.Add("@FEE_HEAD_GST_AMOUNT", SqlDbType.Decimal).Value = true;

                ////    sCmd.Parameters.Add("@ACTIVE_YN", SqlDbType.Bit).Value = true;
                ////    sCmd.Parameters.Add("@CREATED_DT", SqlDbType.DateTime).Value = DateTime.Now;
                ////    sCmd.Parameters.Add("@CREATED_BY", SqlDbType.Int).Value = Convert.ToInt32(User_Id);
                ////    sCmd.Parameters.Add("@UPDATED_DT", SqlDbType.DateTime).Value = DateTime.Now;
                ////    sCmd.Parameters.Add("@UPDATED_BY", SqlDbType.Int).Value = Convert.ToInt32(User_Id);
                ////}
                #endregion


            }
            catch (Exception ex)
            {
                status = "ERROR";
            }
            finally
            {
                if (sCon != null)
                {
                    if (sCon.State == ConnectionState.Open)
                        sCon.Close();
                }
            }
            return status;
        }

        public string COP_ISSUE_PROCESS_PAYMENT(string User_Id, string Membership_No, string Payment_Status)
        {
            string status = string.Empty;
            try
            {
                if (Payment_Status == "1")
                {
                    sCon = new SqlConnection(Prod_Training_Stimulate);
                    sCmd = new SqlCommand("UPDATE MEM_COP_DETAILS SET APPLICATION_STATUS_ID=2,REQUESTED_DATE_DT=@REQUESTED_DATE_DT "
                        + "WHERE User_Id=@User_Id AND ACTIVE_YN=1 and REQUEST_TYPE_ID=1", sCon);
                    sCmd.Parameters.Add("@REQUESTED_DATE_DT", SqlDbType.DateTime).Value = DateTime.Now;
                    sCmd.Parameters.Add("@User_Id", SqlDbType.Int).Value = User_Id;
                    sCon.Open();
                    int i = sCmd.ExecuteNonQuery();

                    if (i > 0)
                        status = "SUCCESS";
                    else
                        status = "ERROR";
                }
                else
                    status = "FAILED";

            }
            catch (Exception ex)
            {
                status = "ERROR";
            }
            finally
            {
                if (sCon != null)
                {
                    if (sCon.State == ConnectionState.Open)
                        sCon.Close();
                }
            }
            return status;
        }




        public List<Admin_COP_Track_Search_Model> Admin_COP_Review_Search(string User_Id, string Name, string Request_From, string Request_To,
            string Email_Id, string Admission_From, string Admission_To, string MemberShip_Number, string MobileNo, string COP_Number,
            int Application_Status_Id, int Request_Type_Id, int Internal_Dept_Status_Id)
        {
            string status = string.Empty;
            List<Admin_COP_Track_Search_Model> lst = new List<Admin_COP_Track_Search_Model>();
            try
            {
                sCon = new SqlConnection(Prod_Training_Stimulate);
                sCmd = new SqlCommand("USP_MEM_COP_ADMIN_SEARCH", sCon);
                sCmd.CommandType = CommandType.StoredProcedure;
                sCmd.Parameters.Add("@USER_ID", SqlDbType.BigInt).Value = Convert.ToInt64(User_Id);
                sCmd.Parameters.Add("@Name", SqlDbType.VarChar, 200).Value = Name;
                sCmd.Parameters.Add("@Request_From", SqlDbType.VarChar, 10).Value = Request_From;
                sCmd.Parameters.Add("@Request_To", SqlDbType.VarChar, 10).Value = Request_To;
                sCmd.Parameters.Add("@Email_Id", SqlDbType.VarChar, 200).Value = Email_Id;
                sCmd.Parameters.Add("@Admission_From", SqlDbType.VarChar, 10).Value = Admission_From;
                sCmd.Parameters.Add("@Admission_To", SqlDbType.VarChar, 10).Value = Admission_To;
                sCmd.Parameters.Add("@MemberShip_Number", SqlDbType.VarChar, 200).Value = MemberShip_Number;
                sCmd.Parameters.Add("@MobileNo", SqlDbType.VarChar, 10).Value = MobileNo;
                sCmd.Parameters.Add("@COP_Number", SqlDbType.VarChar, 200).Value = COP_Number;
                sCmd.Parameters.Add("@Application_Status_Id", SqlDbType.Int).Value = Application_Status_Id;
                sCmd.Parameters.Add("@Request_Type_Id", SqlDbType.Int).Value = Request_Type_Id;
                sCmd.Parameters.Add("@Internal_Dept_Status_Id", SqlDbType.Int).Value = Internal_Dept_Status_Id;
                DataTable dt = new DataTable();
                sDa = new SqlDataAdapter(sCmd);
                sDa.Fill(dt);

                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        Admin_COP_Track_Search_Model obj;
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            obj = new Admin_COP_Track_Search_Model();

                            obj.User_Id = Convert.ToInt64(dt.Rows[i]["User_Id"].ToString());
                            obj.Request_Type = dt.Rows[i]["Request_Type_Tx"].ToString();
                            obj.Name = dt.Rows[i]["FIRST_NAME_TX"].ToString() + dt.Rows[i]["MIDDLE_NAME_TX"].ToString() + " " + dt.Rows[i]["LAST_NAME_TX"].ToString();
                            obj.Email_Id = dt.Rows[i]["EMAIL_ID_TX"].ToString();
                            obj.MobileNo = dt.Rows[i]["MOBILE_NO_TX"].ToString();
                            if (dt.Rows[i]["REQUESTED_DATE_DT"].ToString() != "")
                                obj.Request_Date = string.Format("{0:dd MMM yyyy}", dt.Rows[i]["REQUESTED_DATE_DT"].ToString());
                            else
                                obj.Request_Date = "";
                            if (dt.Rows[i]["Admission_Date"].ToString() != "")
                                obj.Admission_Date = string.Format("{0:dd MMM yyyy}", dt.Rows[i]["Admission_Date"].ToString());
                            else
                                obj.Admission_Date = "";
                            if (dt.Rows[i]["Approval_Date"].ToString() != "")
                                obj.Approval_Date = string.Format("{0:dd MMM yyyy}", dt.Rows[i]["Approval_Date"].ToString());
                            else
                                obj.Approval_Date = "";
                            obj.Fee_Paid = dt.Rows[i]["Fee_Paid"].ToString();
                            obj.Application_Status = dt.Rows[i]["Application_Status"].ToString();
                            obj.COP_Number = dt.Rows[i]["COP_NO"].ToString();
                            if (dt.Rows[i]["COP_ISSUE_DATE_DT"].ToString() != "")
                                obj.COP_Issue_Date = string.Format("{0:dd MMM yyyy}", dt.Rows[i]["COP_ISSUE_DATE_DT"].ToString());
                            else
                                obj.COP_Issue_Date = "";
                            obj.Internal_Dept_Status = dt.Rows[i]["Internal_Deptt_Status"].ToString();
                            obj.MemberShip_Number = dt.Rows[i]["MembNo_Tx"].ToString();

                            obj.Transcation_Id = dt.Rows[i]["Transcation_ID"].ToString();
                            if (dt.Rows[i]["Transcation_Date"].ToString() != "")
                                obj.Transcation_Date = string.Format("{0:dd MMM yyyy}", dt.Rows[i]["Transcation_Date"].ToString());
                            else
                                obj.Transcation_Date = "";
                            obj.Mode_Of_Payment = dt.Rows[i]["Mode_Of_Payment"].ToString();
                            obj.Payment_Status = dt.Rows[i]["Payment_Status"].ToString();

                            obj.Application_Status_Id = Convert.ToInt32(dt.Rows[i]["Application_Status_Id"].ToString());
                            obj.APPROVAL_LEVEL_NO = Convert.ToInt32(dt.Rows[i]["APPROVAL_LEVEL_NO"].ToString());
                            obj.COP_DETAILS_ID = Convert.ToInt32(dt.Rows[i]["COP_DETAILS_ID"].ToString());

                            lst.Add(obj);
                        }
                    }
                }

                for (int i = 0; i < lst.Count; i++)
                {
                    sCmd = new SqlCommand("SELECT* FROM MEM_COP_APPROVE_PROCESS_DETAILS_T WHERE ACTIVE_YN = 1 AND COP_DETAILS_ID = @COP_DETAILS_ID", sCon);
                    sCmd.Parameters.Add("@COP_DETAILS_ID", SqlDbType.BigInt).Value = lst[i].COP_DETAILS_ID;
                    DataTable dtProcess = new DataTable();
                    sDa = new SqlDataAdapter(sCmd);
                    sDa.Fill(dtProcess);

                    if (dtProcess != null)
                    {
                        if (dtProcess.Rows.Count > 0)
                        {
                            lst[i].Next_Role_ID = Convert.ToInt32(dtProcess.Rows[dtProcess.Rows.Count - 1]["NEXT_ROLE_ID"].ToString());
                        }
                    }
                }

                sCon = new SqlConnection(Prod_Stimulate);
                sCon.Open();
                sCmd = new SqlCommand("SELECT ROLE_ID FROM USER_ROLE_T WHERE USER_ID=@USER_ID AND ACTIVE_YN=1", sCon);
                sCmd.Parameters.Add("@USER_ID", SqlDbType.BigInt).Value = User_Id;
                int _role_ID = 0;
                object _role = sCmd.ExecuteScalar();
                if (_role != null)
                {
                    _role_ID = Convert.ToInt32(_role.ToString());
                }
                for (int i = 0; i < lst.Count; i++)
                {
                    lst[i].User_Role_ID = _role_ID;
                }



            }
            catch (Exception ex)
            {
                status = "ERROR";
            }
            finally
            {
                if (sCon != null)
                {
                    if (sCon.State == ConnectionState.Open)
                        sCon.Close();
                }
            }
            return lst;
        }

        public string Get_MemberShip_No(string User_Id)
        {
            string status = string.Empty;
            try
            {
                sCon = new SqlConnection(Prod_Training_Stimulate);
                sCmd = new SqlCommand("SELECT * FROM MEM_COP_DETAILS WHERE USER_ID=@USER_ID AND ACTIVE_YN=1", sCon);
                sCmd.Parameters.Add("@USER_ID", SqlDbType.BigInt).Value = Convert.ToInt64(User_Id);
                DataTable dt = new DataTable();
                sDa = new SqlDataAdapter(sCmd);
                sDa.Fill(dt);

                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        status = dt.Rows[0]["MEMBNO_TX"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                status = "ERROR";
            }
            finally
            {
                if (sCon != null)
                {
                    if (sCon.State == ConnectionState.Open)
                        sCon.Close();
                }
            }
            return status;
        }

        public string Insert_Admin_COP_Personal_Details_Issue_View(DataTable dt)
        {
            string status = string.Empty;
            try
            {
                sCon = new SqlConnection(Prod_Training_Stimulate);
                sCon.Open();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    sCmd = new SqlCommand("UPDATE MEM_COP_ISSUE_DOCUMENT_TYPE_MASTER_T  SET APPROVE_NM=@APPROVE_NM where Active_YN=1 and ID=@ID", sCon);
                    sCmd.Parameters.Add("@ID", SqlDbType.Int).Value = Convert.ToInt32(dt.Rows[i]["Id"].ToString());
                    sCmd.Parameters.Add("@APPROVE_NM", SqlDbType.Int).Value = Convert.ToInt32(dt.Rows[i]["Approved_Id"].ToString());
                    sCmd.ExecuteNonQuery();
                }
                status = "SUCCESS";
            }
            catch (Exception ex)
            {
                status = "ERROR";
            }
            finally
            {
                if (sCon != null)
                {
                    if (sCon.State == ConnectionState.Open)
                        sCon.Close();
                }
            }
            return status;
        }

        public Admin_COP_Approval_Load Admin_COP_Declaration_Issue_View_Load(string User_Id)
        {
            string status = string.Empty;
            Admin_COP_Approval_Load obj = new Admin_COP_Approval_Load();
            DataSet ds = new DataSet();
            try
            {
                sCon = new SqlConnection(Prod_Training_Stimulate);
                sCmd = new SqlCommand("USP_MEM_COP_APPROVAL_PROCESS", sCon);
                sCmd.CommandType = CommandType.StoredProcedure;
                sCmd.Parameters.Add("@USER_ID", SqlDbType.BigInt).Value = Convert.ToInt64(User_Id);
                //sCmd.Parameters.Add("@MEMBERSHIP_NO", SqlDbType.VarChar, 20).Value = Membership_No;
                sDa = new SqlDataAdapter(sCmd);
                sDa.Fill(ds);

                // Personal details
                if (ds.Tables[0].Rows.Count > 0)
                {
                    obj.Member_Name = ds.Tables[0].Rows[0]["FIRST_NAME_TX"].ToString() + " " + ds.Tables[0].Rows[0]["MIDDLE_NAME_TX"].ToString() + " " + ds.Tables[0].Rows[0]["LAST_NAME_TX"].ToString();
                    obj.RES_STATE_ID = ds.Tables[0].Rows[0]["RES_STATE_ID"].ToString();
                    obj.MOBILE_NO_TX = ds.Tables[0].Rows[0]["MOBILE_NO_TX"].ToString();
                    obj.EMAIL_ID_TX = ds.Tables[0].Rows[0]["EMAIL_ID_TX"].ToString();
                }

                //Details of Orientation Programme
                if (ds.Tables[1].Rows.Count > 0)
                {
                    List<COP_Details_Model> lstOri = new List<COP_Details_Model>();
                    COP_Details_Model ori;
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        ori = new COP_Details_Model();
                        ori.ID = Convert.ToInt32(ds.Tables[1].Rows[i]["ID"].ToString());
                        ori.Request_Type = ds.Tables[1].Rows[i]["REQUEST_TYPE_TX"].ToString();
                        ori.MemberShip_Number = ds.Tables[1].Rows[i]["MEMBNO_TX"].ToString();
                        ori.Duration = 0;
                        ori.Request_Date = ds.Tables[1].Rows[i]["REQUESTED_DATE_DT"].ToString();
                        ori.Application_Status = ds.Tables[1].Rows[i]["STATUS_TX"].ToString();
                        ori.MemberShip_Number = ds.Tables[1].Rows[i]["MEMBNO_TX"].ToString();
                        ori.APPROVAL_LEVEL_NO = Convert.ToInt32(ds.Tables[1].Rows[i]["APPROVAL_LEVEL_NO"].ToString());
                        lstOri.Add(ori);
                    }
                    obj.lst_COP_Details_Details = lstOri;
                }

                obj.APPROVAL_LEVEL_NO = obj.lst_COP_Details_Details.Last().APPROVAL_LEVEL_NO;
                obj.Membership_No = obj.lst_COP_Details_Details.Last().MemberShip_Number;
                obj.COP_Details_Id = obj.lst_COP_Details_Details.Last().ID;

                //Forward history details
                if (ds.Tables[2].Rows.Count > 0)
                {
                    List<COP_Forward_Details_Model> lstLE = new List<COP_Forward_Details_Model>();
                    COP_Forward_Details_Model ori;
                    for (int i = 0; i < ds.Tables[2].Rows.Count; i++)
                    {
                        ori = new COP_Forward_Details_Model();
                        ori.Forwarded_By = ds.Tables[2].Rows[i]["ROLE_NAME"].ToString();
                        ori.Forwarded_To = ds.Tables[2].Rows[i]["NEXT_ROLE_NAME"].ToString();
                        ori.Forwarded_Date = ds.Tables[2].Rows[i]["CREATED_DT"].ToString();
                        ori.Remarks = ds.Tables[2].Rows[i]["INTERNAL_REMARKS"].ToString();
                        ori.APPLICATION_STATUS_ID = Convert.ToInt32(ds.Tables[2].Rows[i]["APPLICATION_STATUS_ID"].ToString());
                        ori.ROLE_ID = Convert.ToInt32(ds.Tables[2].Rows[i]["ROLE_ID"].ToString());
                        ori.NEXT_ROLE_ID = Convert.ToInt32(ds.Tables[2].Rows[i]["NEXT_ROLE_ID"].ToString());
                        lstLE.Add(ori);
                    }
                    obj.lst_COP_Forward_Detail = lstLE;
                }

                //Details of document details
                if (ds.Tables[3].Rows.Count > 0)
                {
                    List<COP_Issue_Document_Details_Model> lstDoc = new List<COP_Issue_Document_Details_Model>();
                    COP_Issue_Document_Details_Model doc;
                    for (int i = 0; i < ds.Tables[3].Rows.Count; i++)
                    {
                        doc = new COP_Issue_Document_Details_Model();
                        doc.ID = Convert.ToInt32(ds.Tables[3].Rows[i]["ID"].ToString());
                        doc.DOCUMENT_TYPE_TX = ds.Tables[3].Rows[i]["DOCUMENT_TYPE_TX"].ToString();
                        doc.FILE_NAME_TX = ds.Tables[3].Rows[i]["FILE_NAME_TX"].ToString();
                        doc.FILE_PATH = ds.Tables[3].Rows[i]["FILE_PATH"].ToString();
                        doc.UPLODED = ds.Tables[3].Rows[i]["UPLODED"].ToString();
                        doc.Approved_Id = Convert.ToInt32(ds.Tables[3].Rows[i]["APPROVE_NM"].ToString());

                        lstDoc.Add(doc);
                    }
                    obj.lst_Document_Details = lstDoc;
                }

            }
            catch (Exception ex)
            {
                status = "ERROR";
            }
            finally
            {
                if (sCon != null)
                {
                    if (sCon.State == ConnectionState.Open)
                        sCon.Close();
                }
            }
            return obj;
        }

        public string Insert_Admin_COP_Approval_Details(COP_Forward_Details_Model obj)
        {
            string status = string.Empty;
            try
            {
                sCon = new SqlConnection(Prod_Training_Stimulate);
                sCon.Open();

                sCmd = new SqlCommand("SELECT * FROM MEM_COP_DETAILS WHERE USER_ID=@USER_ID AND ACTIVE_YN=1 ORDER BY ID DESC", sCon);
                sCmd.Parameters.Add("@USER_ID", SqlDbType.BigInt).Value = obj.USER_ID;
                sDa = new SqlDataAdapter(sCmd);
                DataTable dt_COP_Details = new DataTable();
                sDa.Fill(dt_COP_Details);

                // Personal details
                if (dt_COP_Details.Rows.Count > 0)
                {
                    int cop_Count = 0;
                    sCmd = new SqlCommand("SELECT COUNT(*) FROM MEM_COP_DETAILS WHERE ACTIVE_YN=1 AND COP_NO IS NOT NULL", sCon);
                    object _role = sCmd.ExecuteScalar();
                    if (_role != null)
                    {
                        cop_Count = Convert.ToInt32(_role.ToString());
                    }
                    cop_Count = cop_Count + 1;


                    // Application 
                    string _cmdText = string.Empty;
                    int SendMail = 0;
                    string COP_Number = string.Empty;

                    ////sCmd = new SqlCommand("UPDATE MEM_COP_DETAILS SET APPLICATION_STATUS_ID=@APPLICATION_STATUS_ID,"
                    ////    + "APPROVAL_LEVEL_NO=@APPROVAL_LEVEL_NO WHERE ID=@ID", sCon);
                    sCmd = new SqlCommand();

                    if (obj.ROLE_ID == 17 && (obj.APPLICATION_STATUS_ID == 3 || obj.APPLICATION_STATUS_ID == 5))
                    {
                        SendMail = obj.APPLICATION_STATUS_ID;

                        if (obj.APPLICATION_STATUS_ID == 3)
                        {
                            COP_Number = "I" + DateTime.Now.Year + obj.RES_STATE_ID + cop_Count + "00";

                            sCmd.Parameters.Add("@COP_NO", SqlDbType.VarChar, 50).Value = COP_Number;
                            sCmd.Parameters.Add("@COP_ISSUE_DATE_DT", SqlDbType.DateTime).Value = DateTime.Now;
                            _cmdText = "COP_NO=@COP_NO,COP_ISSUE_DATE_DT=@COP_ISSUE_DATE_DT";
                        }

                        sCmd.Parameters.Add("@APPLICATION_STATUS_ID", SqlDbType.Int).Value = obj.APPLICATION_STATUS_ID;
                        if (_cmdText == "")
                            _cmdText = "APPLICATION_STATUS_ID=@APPLICATION_STATUS_ID";
                        else
                            _cmdText = _cmdText + ",APPLICATION_STATUS_ID=@APPLICATION_STATUS_ID";
                    }

                    if ((obj.ROLE_ID == 16 || obj.ROLE_ID == 17 || obj.ROLE_ID == 18) && obj.APPLICATION_STATUS_ID == 4)
                    {
                        SendMail = 4;

                        if (obj.ROLE_ID == 16)
                        {
                            sCmd.Parameters.Add("@APPLICATION_STATUS_ID", SqlDbType.Int).Value = obj.APPLICATION_STATUS_ID;
                            _cmdText = "APPLICATION_STATUS_ID=@APPLICATION_STATUS_ID";
                        }

                        sCmd.Parameters.Add("@APPROVAL_LEVEL_NO", SqlDbType.Int).Value = 0;
                        if (_cmdText == "")
                            _cmdText = "APPROVAL_LEVEL_NO=@APPROVAL_LEVEL_NO";
                        else
                            _cmdText = _cmdText + ",APPROVAL_LEVEL_NO=@APPROVAL_LEVEL_NO";
                    }

                    if ((obj.APPLICATION_STATUS_ID == 3 || obj.APPLICATION_STATUS_ID == 5) && (obj.ROLE_ID != 17))
                    {
                        if (obj.ROLE_ID == 16)
                            sCmd.Parameters.Add("@APPROVAL_LEVEL_NO", SqlDbType.Int).Value = 1;
                        else if (obj.ROLE_ID == 18)
                            sCmd.Parameters.Add("@APPROVAL_LEVEL_NO", SqlDbType.Int).Value = 2;

                        if (_cmdText == "")
                            _cmdText = "APPROVAL_LEVEL_NO=@APPROVAL_LEVEL_NO";
                        else
                            _cmdText = _cmdText + ",APPROVAL_LEVEL_NO=@APPROVAL_LEVEL_NO";
                    }

                    string finalText = "UPDATE MEM_COP_DETAILS SET " + _cmdText + " WHERE ID=@ID";
                    sCmd.CommandText = finalText;
                    sCmd.Connection = sCon;

                    sCmd.Parameters.Add("@ID", SqlDbType.Int).Value = Convert.ToInt32(dt_COP_Details.Rows[0]["ID"].ToString());
                    sCmd.ExecuteNonQuery();

                    //Process table
                    sCmd = new SqlCommand("INSERT INTO MEM_COP_APPROVE_PROCESS_DETAILS_T(COP_DETAILS_ID,APPLICATION_STATUS_ID,"
                    + "ROLE_ID,ROLE_NAME,NEXT_ROLE_ID,NEXT_ROLE_NAME,INTERNAL_REMARKS,REMARKS_FOR_MEMBER,"
                    + "ACTIVE_YN, CREATED_DT, CREATED_BY, UPDATED_DT, UPDATED_BY) "
                    + "values(@COP_DETAILS_ID,@APPLICATION_STATUS_ID,"
                    + "@ROLE_ID,@ROLE_NAME,@NEXT_ROLE_ID,@NEXT_ROLE_NAME,@INTERNAL_REMARKS,@REMARKS_FOR_MEMBER,"
                    + "@ACTIVE_YN,@CREATED_DT,@CREATED_BY,@UPDATED_DT,@UPDATED_BY)", sCon);

                    sCmd.Parameters.Add("@COP_DETAILS_ID", SqlDbType.Int).Value = dt_COP_Details.Rows[0]["ID"].ToString();

                    ////sCmd.Parameters.Add("@USER_ID", SqlDbType.BigInt).Value = obj.USER_ID;
                    sCmd.Parameters.Add("@APPLICATION_STATUS_ID", SqlDbType.Int).Value = obj.APPLICATION_STATUS_ID;
                    sCmd.Parameters.Add("@ROLE_ID", SqlDbType.Int).Value = obj.ROLE_ID;
                    sCmd.Parameters.Add("@ROLE_NAME", SqlDbType.VarChar, 200).Value = obj.ROLE_NAME;
                    sCmd.Parameters.Add("@NEXT_ROLE_ID", SqlDbType.Int).Value = obj.NEXT_ROLE_ID;
                    sCmd.Parameters.Add("@NEXT_ROLE_NAME", SqlDbType.VarChar, 200).Value = obj.NEXT_ROLE_NAME;
                    sCmd.Parameters.Add("@INTERNAL_REMARKS", SqlDbType.VarChar, 500).Value = obj.INTERNAL_REMARKS;
                    sCmd.Parameters.Add("@REMARKS_FOR_MEMBER", SqlDbType.VarChar, 500).Value = obj.REMARKS_FOR_MEMBER;

                    sCmd.Parameters.Add("@ACTIVE_YN", SqlDbType.Bit).Value = true;
                    sCmd.Parameters.Add("@CREATED_DT", SqlDbType.DateTime).Value = DateTime.Now;
                    sCmd.Parameters.Add("@CREATED_BY", SqlDbType.Int).Value = obj.USER_ID;
                    sCmd.Parameters.Add("@UPDATED_DT", SqlDbType.DateTime).Value = DateTime.Now;
                    sCmd.Parameters.Add("@UPDATED_BY", SqlDbType.Int).Value = obj.USER_ID;


                    int i = sCmd.ExecuteNonQuery();
                    if (i >= 0)
                    {
                        status = "SUCCESS";

                        ////if (SendMail != 0)
                        ////{
                        ////    sCmd = new SqlCommand("SELECT * FROM EMAIL_TEMPLATE_T WHERE ACTIVE_YN=1 AND SCREEN_ID=@SCREEN_ID AND STATUS_TYPE_NM=@STATUS_TYPE_NM", sCon);
                        ////    sCmd.Parameters.Add("@SCREEN_ID", SqlDbType.Int).Value = obj.SCREEN_ID;
                        ////    sCmd.Parameters.Add("@STATUS_TYPE_NM", SqlDbType.Int).Value = SendMail;
                        ////    sDa = new SqlDataAdapter(sCmd);
                        ////    DataTable dt_Email = new DataTable();
                        ////    sDa.Fill(dt_Email);

                        ////    if (dt_Email != null)
                        ////    {
                        ////        if (dt_Email.Rows.Count > 0)
                        ////        {                                   
                        ////            sCmd = new SqlCommand("SELECT QRY_TX FROM QUERY_T WHERE ACTIVE_YN=1 and ID=@ID", sCon);
                        ////            sCmd.Parameters.Add("@ID", SqlDbType.Int).Value =Convert.ToInt32(dt_Email.Rows[0]["QUERY_ID"].ToString());
                        ////            object _qry = sCmd.ExecuteScalar();
                        ////            if (_qry != null)
                        ////            {
                        ////                cop_Count = Convert.ToInt32(_qry.ToString());
                        ////            }

                        ////        }
                        ////    }

                        ////}

                    }
                    else
                        status = "ERROR";
                }
                else
                    status = "ERROR";
            }
            catch (Exception ex)
            {
                status = "ERROR";
            }
            finally
            {
                if (sCon != null)
                {
                    if (sCon.State == ConnectionState.Open)
                        sCon.Close();
                }
            }
            return status;
        }


        /// <summary>
        /// Common method for insert application error message
        /// </summary>
        /// <param name="errorMsg"></param>
        /// <param name="controller"></param>
        /// <param name="action"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public int ErrorLog_Insert(string errorMsg, string controller, string action, string username = null)
        {
            try
            {
                string strLog = DateTime.Now.ToString("yyyy-dd-MM hh:mm:ss tt") + " - Controller Name : " + controller + ", Action Name : " + action + ", Error : " + errorMsg + ", UserName : " + username;

                StreamWriter log;
                FileStream fileStream = null;
                //DirectoryInfo logDirInfo = null;
                FileInfo logFileInfo;

                string logFilePath = System.Web.Hosting.HostingEnvironment.MapPath("~/Log/");

                logFilePath = logFilePath + "Log-" + System.DateTime.Today.ToString("yyyy-MM-dd") + "." + "txt";
                logFileInfo = new FileInfo(logFilePath);
                //logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);
                //if (!logDirInfo.Exists)
                //{
                //    logDirInfo.Create();
                //}
                if (!logFileInfo.Exists)
                {
                    fileStream = logFileInfo.Create();
                }
                else
                {
                    fileStream = new FileStream(logFilePath, FileMode.Append);
                }
                log = new StreamWriter(fileStream);
                log.WriteLine(strLog);
                log.Close();

                return 1;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }


    }
}