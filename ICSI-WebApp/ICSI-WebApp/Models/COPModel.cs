using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ICSI_WebApp.Models
{
    public class COPModel
    {
    }


    public class DropDownMappingsModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }


    public class COP_Issue_Personal_Details_Model
    {
        public string Member_Name { get; set; }
        public string FATHER_HUSB_NAME_TX { get; set; }
        public string MOBILE_NO_TX { get; set; }
        public string EMAIL_ID_TX { get; set; }
        public string WEBSITE_TX { get; set; }
        public string Membership_No { get; set; }
        public string Address { get; set; }

        public string Area_Of_Practice_Id { get; set; }
        public string Other_Service { get; set; }

        public int MEM_COP_DETAILS_ID { get; set; }
        public int ECSIN_NOT_APPLICABLE_YN { get; set; }
        public int ECSIN_EXEMPTED { get; set; }
        public int ECSIN_EXEMPTION_CATERGORY_ID { get; set; }

        public int Application_Status_Id { get; set; }
        public int Application_Process_Status_Id { get; set; }

        public List<COP_Issue_Personal_Details_Model> lst_Personal_Details { get; set; }
        public List<COP_Issue_Orientation_Details_Model> lst_Orientation_Details { get; set; }
        public List<COP_Issue_Last_Employee_Details_Model> lst_Last_Employee_Details { get; set; }
        public List<COP_Issue_Document_Details_Model> lst_Document_Details { get; set; }

        public List<Other_Institutions_Model> lst_Other_Institutions { get; set; }
        public List<Firm_Details_Model> lst_Firm_Details { get; set; }

        public List<DropDownMappingsModel> lst_Area_Of_Practice { get; set; }

        public List<DropDownMappingsModel> lst_ECSIN_Exempted { get; set; }

    }

    public class COP_Issue_Orientation_Details_Model
    {
        public string ORGANIZED_BY { get; set; }
        public string CERTIFICATE_NO_TX { get; set; }
        public string FROM_DT { get; set; }
        public string TO_DT { get; set; }
        public string DURATION_TX { get; set; }

        public List<COP_Issue_Orientation_Details_Model> lst_Orientation_Details { get; set; }
    }

    public class COP_Issue_Last_Employee_Details_Model
    {
        public string COMPNAY_NAME_TX { get; set; }
        public string DIR_TX { get; set; }
        public string ECSIN_NO_TX { get; set; }
        public string DESIGNATION_TX { get; set; }
        public string JOINING_DT { get; set; }
        public string LAST_END_DT { get; set; }
        public string DURATION_OF_EMPLOYEEMENT_TX { get; set; }

        public List<COP_Issue_Last_Employee_Details_Model> lst_Last_Employee_Details { get; set; }
    }

    public class COP_Issue_Document_Details_Model
    {
        public int ID { get; set; }
        public string DOCUMENT_TYPE_TX { get; set; }
        public string FILE_NAME_TX { get; set; }
        public string FILE_PATH { get; set; }
        public string UPLODED { get; set; }
        public int Approved_Id { get; set; }



        public List<COP_Issue_Document_Details_Model> lst_Document_Details { get; set; }
    }


    public class Particulars_Of_COP_Model
    {
        public int COP_PARTICULARS_ID { get; set; }
        public string Area_Of_Practice_Ids { get; set; }
        public string Other_Service { get; set; }

        public int Application_Status_Id { get; set; }

        public List<DropDownMappingsModel> lst_Area_Of_Practice { get; set; }

        public List<Particulars_Of_COP_Model> lst_Particulars_Of_COP { get; set; }
    }

    public class Other_Institutions_Model
    {
        public int ID { get; set; }
        public Int64 USER_ID { get; set; }

        public int IS_ICAI_MEMBER { get; set; }
        public string ICAI_MEMBERSHIP_NO_TX { get; set; }
        public int IS_ICAI_MEMBER_HC { get; set; }

        public int IS_COST_MEMBERSHIP { get; set; }
        public string COST_MEMBERSHIP_NO_TX { get; set; }
        public int COST_MEMBERSHIP_HC { get; set; }

        public int IS_BAR_COUNCIL { get; set; }
        public string BAR_COUNCIL_NO_TX { get; set; }
        public int IS_BAR_COUNCIL_HC { get; set; }

        public int IS_PROF_INSTITUTE { get; set; }
        public string PROF_INSTITUTE_NO_TX { get; set; }
        public int IS_PROF_INSTITUTE_HC { get; set; }


        public int Application_Status_Id { get; set; }

        public List<Firm_Details_Model> lst_Firm_Details { get; set; }
    }

    public class Firm_Details_Model
    {
        public int ID { get; set; }
        public Int64 USER_ID { get; set; }
        public int FIRM_TYPE_ID { get; set; }
        public string FIRM_NAME_TX { get; set; }
        public string ADDRESS1_TX { get; set; }
        public string ADDRESS2_TX { get; set; }
        public string ADDRESS3_TX { get; set; }
        public int COUNTRY_ID { get; set; }
        public int STATE_ID { get; set; }
        public int DISTRICT_ID { get; set; }
        public int CITY_ID { get; set; }
        public int PINCODE_NM { get; set; }
        public string BUSINESS_NATURE_TX { get; set; }
        public string NAME_OF_PARTNERS_TX { get; set; }
        public string OTHER_DETAILS_TX { get; set; }

        public int REF_REF_ID { get; set; }
        public int REMOVE_NM { get; set; }
        public int REF_ID { get; set; }

        public string FIRM_TYPE_TX { get; set; }
    }

    public class Declaration_Dependants_Model
    {
        public int ID { get; set; }
        public Int64 USER_ID { get; set; }
        public string NAME_TX { get; set; }
        public int AGE_NM { get; set; }
        public string RELATION_TO_SUBSCRIBER_TX { get; set; }
        public string EMAIL_TX { get; set; }
        public string MOBILE_TX { get; set; }
        public string ADDRESS { get; set; }

        public string Student_Name { get; set; }
        public int Application_Status_Id { get; set; }

        public List<Declaration_Dependants_Model> lst_Dependants_Details { get; set; }
    }

    public class COP_ISSUE_PAYMENT_MODEL
    {
        public int ID { get; set; }
        public Int64 USER_ID { get; set; }

        public string FEE_AMOUNT { get; set; }
        public int TAXABLE_YN { get; set; }

        public string GST_AMOUNT { get; set; }
        public string TOTAL_FEE_AMOUNT { get; set; }
        public int STATE_ID { get; set; }
    }

    //...................Admin..........................
    public class Admin_COP_Track_Search_Model
    {
        public Int64 User_Id { get; set; }
        public string Request_Type { get; set; }
        public string Name { get; set; }
        public string Email_Id { get; set; }
        public string MobileNo { get; set; }
        public string Request_Date { get; set; }
        public string Admission_Date { get; set; }
        public string Approval_Date { get; set; }
        public string Fee_Paid { get; set; }
        public string Application_Status { get; set; }
        public string COP_Number { get; set; }
        public string COP_Issue_Date { get; set; }
        public string Internal_Dept_Status { get; set; }
        public string MemberShip_Number { get; set; }

        public string Transcation_Id { get; set; }
        public string Transcation_Date { get; set; }
        public string Mode_Of_Payment { get; set; }
        public string Payment_Status { get; set; }

        public int Request_Type_Id { get; set; }
        public int Application_Status_Id { get; set; }
        public int APPROVAL_LEVEL_NO { get; set; }
        public int User_Role_ID { get; set; }
        public int Next_Role_ID { get; set; }
        public int COP_DETAILS_ID { get; set; }

        public List<Admin_COP_Track_Search_Model> lst_Admin_COP_Search_Details { get; set; }

    }

    public class COP_Details_Model
    {
        public Int64 User_Id { get; set; }
        public int ID { get; set; }
        public int APPROVAL_LEVEL_NO { get; set; }

        public string Name { get; set; }
        public string MemberShip_Number { get; set; }
        public string Request_Date { get; set; }
        public string Transcation_Id { get; set; }
        public string Transcation_Date { get; set; }

        public int Duration { get; set; }

        public int Request_Type_Id { get; set; }
        public string Request_Type { get; set; }
        public int Application_Status_Id { get; set; }
        public string Application_Status { get; set; }
        public string Internal_Dept_Status { get; set; }

        public int MEM_COP_DETAILS_ID { get; set; }
        public int ECSIN_NOT_APPLICABLE_YN { get; set; }
        public int ECSIN_EXEMPTED { get; set; }
        public int ECSIN_EXEMPTION_CATERGORY_ID { get; set; }

        public string Validation_Status { get; set; }

        public List<COP_Details_Model> lst_COP_Details_Model { get; set; }
    }

    public class COP_Forward_Details_Model
    {
        public int ID { get; set; }
        public Int64 USER_ID { get; set; }
        public int COP_DETAILS_ID { get; set; }
        public int APPLICATION_STATUS_ID { get; set; }
        public int ROLE_ID { get; set; }
        public string ROLE_NAME { get; set; }
        public int NEXT_ROLE_ID { get; set; }
        public string NEXT_ROLE_NAME { get; set; }
        public string INTERNAL_REMARKS { get; set; }
        public string REMARKS_FOR_MEMBER { get; set; }

        public string Forwarded_By { get; set; }
        public string Forwarded_To { get; set; }
        public string Forwarded_Date { get; set; }
        public string Remarks { get; set; }

        public int SCREEN_ID { get; set; }
        public string RES_STATE_ID { get; set; }
        public string MOBILE_NO_TX { get; set; }
        public string EMAIL_ID_TX { get; set; }

        public List<COP_Forward_Details_Model> lst_COP_Forward_Details_Model { get; set; }
    }

    public class Admin_COP_Approval_Load
    {
        public string Member_Name { get; set; }
        public string Membership_No { get; set; }
        public string Area_Of_Practice_Id { get; set; }

        public int APPROVAL_LEVEL_NO { get; set; }
        public int COP_Details_Id { get; set; }

        public string RES_STATE_ID { get; set; }
        public string MOBILE_NO_TX { get; set; }
        public string EMAIL_ID_TX { get; set; }

        public List<Admin_COP_Approval_Load> lst_Personal_Details { get; set; }
        public List<COP_Details_Model> lst_COP_Details_Details { get; set; }
        public List<COP_Forward_Details_Model> lst_COP_Forward_Detail { get; set; }
        public List<COP_Issue_Document_Details_Model> lst_Document_Details { get; set; }

    }


}