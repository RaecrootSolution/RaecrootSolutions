using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ICSI_Event.DBContext;
using ICSI_Event.Models;

namespace ICSI_Event.Repository
{
    public interface IUserRepository
    {
       
        bool StallRegistration(EventsRegistration eventsRegistration);
        List<Event> GetAllEvents();

        bool ExistRegistration(EventsRegistration eventsRegistration);
        List<State> GetAllState();
        List<City> GetALLCity(int SatetId);
        string GetStateCode(int SatetId);
        bool AddStallNumber(StallNumber stallNumber);

        List<StallNumber> GetAllStallNumber();
        List<StallNumber> GetAllDIARYNumber();  // for Diary 1
        
        int StallPayment(StallBooking stallBooking);
        int DiaryPayment(DiaryBooking DiaryBooking);  //diary booking

        List<BookedStall> BookedStall();

        PaymentDetail GetPaymentDetail(int StallRegisterId);

        StallReceipt GetStallReceipt(int Id);
        StallReceipt GetDIARYReceipt(int Id);  // for Diary 5
        
        bool CheckExistStallNo(string stallNo);
        string GetSateByCode(string Code);
        string GetICSIGST();
        int GetStallNumberId(int RegId);
        int GetDIARYNumberId(int RegId);  // for Diary 2

        void UpdateStallStatus(int Id);
        void UpdateDIARYStatus(int Id); // for Diry 2.1

        int GetRegId(int PayReqId);

        void UpdateStallStatus(int Id, int Status);
        void UpdateDIARYStatus(int Id, int Status);  // for Diary 3
        
        void Update_STALL_TRANSACTION_T_ID(int Id, string txnid,bool status);
        void Update_DIARY_TRANSACTION_T_ID(int Id, string txnid, bool status,string request_id);  // for Diary 4
        void UpdateStallStatusTimeMore10Mints();
        void UpdateDIARYStatusTimeMore10Mints(); // for Diary 6

        ReceiptDetails GetReceiptTransaction(SearchReceiptTransaction obj);

        string GetDIARYDes(string StallNumber);

        void sendMail(string MailTo, string Subject, string Body);
        DIARY_REGISTRATION_T GetEmailTransByID(string MemebershipNumber);

        DIARY_REGISTRATION_T GetEmailTransByID(int MemebershipNumber);

    }
}