using System;
using System.Data;
using PwC.ICSI.DataAccessLayer;

namespace PwC.ICSI.BusinessLayer
{
    
    public class MemberBL
    {
        // New Method for HTML letters --Anil-Filix
        public DataSet GetMemberDetails(String regno, String reportname,string pre)
        {
            MemberDAL objMember = new MemberDAL();
            return objMember.GetMemberDetails(regno, reportname, pre);
        }
       
        public bool InsertPaymentDetails(DataTable StudentPayment)
        {
            MemberDAL objMember = new MemberDAL();
            return objMember.InsertPaymentDetails(StudentPayment);           
        }


        public DataSet GetMemberInfo1()
        {
            MemberDAL objMember = new MemberDAL();
            return objMember.GetMemberInfo1();
        }

        public DataSet GetAddressDetails(string RegNo, string PreMembNo)
        {
            MemberDAL objMember = new MemberDAL();
            return objMember.GetAddressDetails(RegNo, PreMembNo);
        }

        public DataSet UpdateAddress(string[] addressdetails)
        {
            MemberDAL objMember = new MemberDAL();
            return objMember.UpdateAddress(addressdetails); 
        }

        public DataSet QueryRequest(string RegNo,string PreMembNo, string request, string User)
        {
            MemberDAL objMember = new MemberDAL();
            return objMember.QueryRequest(RegNo, PreMembNo, request, User);
        }

        public DataSet GetCourseDetails(string RegNo)
        {
            MemberDAL objMember = new MemberDAL();
            return objMember.GetCourseDetails(RegNo); 
        }

        public DataSet GetMonthYear()
        {
            MemberDAL objMember = new MemberDAL();
            return objMember.GetMonthYear(); 
        }

        public DataSet GetRequestNo(string RegNo, string request)
        {
            MemberDAL objMember = new MemberDAL();
            return objMember.GetRequestNo(RegNo, request); 
        }

        public DataSet CheckOnlineMembAddRegion(string chapcd, string regionnm)
        {
            MemberDAL objMember = new MemberDAL();
            return objMember.CheckOnlineMembAddRegion(chapcd, regionnm);
        }

        public DataSet GetMemberInfo(string RegNo, string PreMembNo)
        {
            MemberDAL objMember = new MemberDAL();
            return objMember.GetMemberInfo(RegNo, PreMembNo); 
        }

        public DataSet GetData(string RegNo, string type)
        {
            MemberDAL objMember = new MemberDAL();
            return objMember.GetData(RegNo,type);
        }


        public DataSet GetMemberOnlineAddDetails(string UserID, string PreMembNo)
        {
            MemberDAL objMember = new MemberDAL();
            return objMember.GetMemberOnlineAddDetails(UserID, PreMembNo);
        }

        

        //public string InsertOracle(string[] OracleValues)
        //{
        //    MemberDAL objMember = new MemberDAL();
        //    return objMember.InsertOracle(OracleValues);
        //}

        public DataSet ChangePassword(string[] paramvalues)
        {
            MemberDAL objMember = new MemberDAL();
            return objMember.ChangePassword(paramvalues);
        }


        public void InsertAuditRecord(string RegNo, string AckNo, string request, string User, string PreMembNo)
        {
            MemberDAL objMember = new MemberDAL();
            objMember.InsertAuditRecord(RegNo, AckNo, request, User, PreMembNo);
        }

        public DataSet GetMemberLoginData(string RegNo, string PreMembNo)
        {
            MemberDAL objMember = new MemberDAL();
            return objMember.GetMemberLoginData(RegNo, PreMembNo);
        }
        public DataSet GetCreditCertNo(string UserID, string MemberType, string PreMembNo)
        {
            MemberDAL objMember = new MemberDAL();
            return objMember.GetCreditCertNo(UserID, MemberType, PreMembNo);
        }

        public DataSet GetFirstBlockYear(string UserID, string MemberType, string PreMembNo)
        {
            MemberDAL objMember = new MemberDAL();
            return objMember.GetFirstBlockYear(UserID, MemberType, PreMembNo);
        }

        public DataSet GetSecondBlockYear(string UserID, string MemberType, string PreMembNo)
        {
            MemberDAL objMember = new MemberDAL();
            return objMember.GetSecondBlockYear(UserID, MemberType, PreMembNo);
        }

        public DataSet GetThirdBlockYear(string UserID, string MemberType, string PreMembNo)
        {
            MemberDAL objMember = new MemberDAL();
            return objMember.GetThirdBlockYear(UserID, MemberType, PreMembNo);
        }
        public DataSet GetFourBlockYear(string UserID, string MemberType, string PreMembNo)
        {
            MemberDAL objMember = new MemberDAL();
            return objMember.GetFourBlockYear(UserID, MemberType, PreMembNo);
        }
        public DataSet GetFifthBlockYear(string UserID, string MemberType, string PreMembNo)
        {
            MemberDAL objMember = new MemberDAL();
            return objMember.GetFifthBlockYear(UserID, MemberType, PreMembNo);
        }
	public DataSet GetsixBlockYear(string UserID, string MemberType, string PreMembNo)
        {
            MemberDAL objMember = new MemberDAL();
            return objMember.GetsixBlockYear(UserID, MemberType, PreMembNo);
        }

        public DataSet AddNonMemberInfo_NC(int fees, string hotelname, int hotelamt, int total, string other, string occup, string spouse, int sveg, string secdel, int secveg, string thirdel, int thirveg, string fname, string lname, string mobile, string add1, string add2, string add3, string city, string state, string pin, string phone, string email, string fax, int mem, int cp, int sc, int spou, int spouamt, int hcode, int nomember, string checkin, string checkout)
        {
            MemberDAL objMember = new MemberDAL();
            return objMember.AddNonMemberInfo_NC(fees, hotelname, hotelamt, total, other, occup, spouse, sveg, secdel, secveg, thirdel, thirveg, fname, lname, mobile, add1, add2, add3, city, state, pin, phone, email, fax, mem, cp, sc, spou, spouamt, hcode, nomember, checkin, checkout);
        }





        public DataSet AddOtherMemberInfo_NC(string regno, int fees, string hotelname, int hotelamt, int total, string other, string occup, string spouse, int sveg, string secdel, int secveg, string thirdel, int thirveg, string fname, string lname, string mobile, string add1, string add2, string add3, string city, string state, string pin, string phone, string email, string fax, int mem, int cp, int sc, int spou, int spouamt, int hcode, string checkin, string checkout)
        {
            MemberDAL objMember = new MemberDAL();
            return objMember.AddOtherMemberInfo_NC(regno, fees, hotelname, hotelamt, total, other, occup, spouse, sveg, secdel, secveg, thirdel, thirveg, fname, lname, mobile, add1, add2, add3, city, state, pin, phone, email, fax, mem, cp, sc, spou, spouamt, hcode, checkin, checkout);
        }

        public DataSet AddMemberInfo_NC(string regno, string premembno, int fees, string hotelname, int hotelamt, int total, string other, string occup, string spouse, int sveg, string secdel, int secveg, string thirdel, int thirveg, string mobile, string add1, string add2, string add3, string city, string state, string pin, string phone, string email, string fax, int mem, int cp, int sc, int spou, int spouamt, int hcode, string checkin, string checkout, int addp, int chil)
        {
            MemberDAL objMember = new MemberDAL();
            return objMember.AddMemberInfo_NC(regno, premembno, fees, hotelname, hotelamt, total, other, occup, spouse, sveg, secdel, secveg, thirdel, thirveg, mobile, add1, add2, add3, city, state, pin, phone, email, fax, mem, cp, sc, spou, spouamt, hcode, checkin, checkout, addp, chil);
        }



        public DataSet GetDay(string date1, string date2)
        {
            MemberDAL objMember = new MemberDAL();
            return objMember.GetDay(date1, date2);
        }


        public DataSet GetMemberName(string RegNo, string PreMembNo)
        {
            MemberDAL objMember = new MemberDAL();
            return objMember.GetMemberName(RegNo, PreMembNo);
        }

        public DataSet CheckMemberInfo(string RegNo, string PreMembNo)
        {
            MemberDAL objMember = new MemberDAL();
            return objMember.CheckMemberInfo(RegNo, PreMembNo);
        }

        public int  InsertRequestDetails(string MembNo, string Request, bool isTaskGenerated,int id)
        {
            MemberDAL objMember = new MemberDAL();
            return objMember.InsertRequestDetails(MembNo, Request,isTaskGenerated,id);
        }

    }
}