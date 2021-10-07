using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
        int StallPayment(StallBooking stallBooking);

        List<BookedStall> BookedStall();

        PaymentDetail GetPaymentDetail(int StallRegisterId);

        StallReceipt GetStallReceipt(int Id);

        bool CheckExistStallNo(string stallNo);
        string GetSateByCode(string Code);
        string GetICSIGST();
        int GetStallNumberId(int RegId);
        void UpdateStallStatus(int Id);
        int GetRegId(int PayReqId);
        void UpdateStallStatus(int Id, int Status);
        void Update_STALL_TRANSACTION_T_ID(int Id, string txnid,bool status);
        void UpdateStallStatusTimeMore10Mints();

        ReceiptDetails GetReceiptTransaction(SearchReceiptTransaction obj);
    }
}