using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ICSI_eCSIN.Models;

namespace ICSI_eCSIN.Repository
{
    public interface IUserRepository
    {       
        tblUser GetUserByUserName(string UserName);
        bool CheckLogin(tblUser objuser);
        bool CheckUdn(tblUser objuser);
        int checkPasswordExist(tblUser User);
        int checkMemberExist(tblUser UserMember); // check member exist or not at member registration time 
        int InsertTblUser(tblUser User);
        int updatetblUserById(tblUser objtblUser);
        string FogotPassword(string MemmbershipNumber, DateTime DOB);
        void sendMail(string MailTo, string Subject, string Body);
        void UpdatePassword(tblUser objuser);
        tblUser GetUserByID(int UserId);
        string UDINGeneration(string MembershipNo);
        bool CheckUdnExistance(string UDNNumber);
        int InserttbleCSINGeneration(tbleCSINGeneration User);
        void InsertGenerateUDIN();
        string eCSINGenerationEmailBody(eCSINGeneration objeCSINGeneration);
        string SubeCSINGenerationEmailBody(SubeCSINGeneration objeCSINGeneration);
        eCSINDetails GetUDINVerification(eCSINVerification obj);

        List<eCSINDetails> GeteCSINList(SearcheCSIN obj);
        List<eCSINDetails> GeteAllCSINList(SearcheCSIN obj);
        bool RevokeeCSIN(eCSINDetails obj);
        bool CheckeCSINGeneration(string eCSINNumber);
        EditeCSIN GeteCSINDetails(string eCSINNumber);
        int UpdateeCSINDetails(EditeCSIN objEditeCSIN);
        bool checkExisteCSIN(int UserId);

        GetTotalUsereCSIN_Result GetTotaleCSINUser();

        int ChangePassword(tblUser objtblUser);
        bool CheckOldPassword(tblUser tblUser);

        UpdateDetails GetEmployeeUpdateDetails (string eCSINNumber);
        int EmployeeUpdateDetailseCSIN(UpdateDetails objUpdateCSIN);
       
        string GetExisteCSINForSub(int UserId);
        bool checkExisteCSINforMSub(int UserId);
        bool CheckSubeCSINExistance(string SubeCSINNumber);
        bool checkExistUserSubsidiary(string UserName);

        bool checkExistRevokedeCSINforMSub(string RevokedeCSINNumber);

        string GetExistEmployerName(int UserId);

        bool checkExistSameUser(int objuserId, string objeCSINNumber);

        bool CheckMembershipUpdate(int UserId);

        List<eCSINDetails> GeteAllMembereCSINList(int UserId);
        int EmployeeeCSINUpdateByAdmin(eCSINDetails objeCSINUpdate);

        int checkSubMemberExist(tblSubsidiaryUser UserMember); // check member exist or not for Subsidiary eCSIN - Admin Panel 
        int InsertTblSubUser(tblSubsidiaryUser User);

    }
}