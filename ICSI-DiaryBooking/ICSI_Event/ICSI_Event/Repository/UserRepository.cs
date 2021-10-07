using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Web;
using ICSI_Event.Models;
using System.Text;
using ICSI_Event.DBContext;
namespace ICSI_Event.Repository
{
    public class UserRepository : IUserRepository
    {
        private ICSI_EventsEntities DBcontext;
        public UserRepository(ICSI_EventsEntities objusercontext)
        {
            this.DBcontext = objusercontext;
        }
        public bool StallRegistration(EventsRegistration eventsRegistration)
        {
            EVENT_REGISTRATION_T eVENT_REGISTRATION_T = new EVENT_REGISTRATION_T();
            eVENT_REGISTRATION_T.COMP_NAME_TX = eventsRegistration.CompanyName;
            eVENT_REGISTRATION_T.USER_ID = eventsRegistration.UserId;
            eVENT_REGISTRATION_T.BILLING_ADDRESS_TX = eventsRegistration.BillingAdd;
            eVENT_REGISTRATION_T.SHIPPING_ADDRESS_TX = eventsRegistration.ShipingAdd;
            eVENT_REGISTRATION_T.PASSWORD_TX = eventsRegistration.Password;
            eVENT_REGISTRATION_T.MOB_NM_TX = eventsRegistration.MobileNo;
            eVENT_REGISTRATION_T.PIN_CODE_TX = eventsRegistration.PinCode;
            eVENT_REGISTRATION_T.STATE_CODE_NM = eventsRegistration.StateCode;
            eVENT_REGISTRATION_T.CITY_TX = eventsRegistration.City;
            eVENT_REGISTRATION_T.EMAIL_TX = eventsRegistration.Email;
            eVENT_REGISTRATION_T.EVENT_ID = eventsRegistration.EventId;
            eVENT_REGISTRATION_T.GSTIN = eventsRegistration.GSTN;
            eVENT_REGISTRATION_T.APPROVED_YN = eventsRegistration.Approved;
            eVENT_REGISTRATION_T.ACTIVE_YN = true;
            eVENT_REGISTRATION_T.APPROVED_YN = true;
            eVENT_REGISTRATION_T.CREATED_BY = 1;
            eVENT_REGISTRATION_T.CREATED_DT = DateTime.Now;
            eVENT_REGISTRATION_T.UPDATED_BY = 1;
            eVENT_REGISTRATION_T.UPDATED_DT = DateTime.Now;
            DBcontext.EVENT_REGISTRATION_T.Add(eVENT_REGISTRATION_T);
            DBcontext.SaveChanges();

            EVENT_USER_T vENT_USER_T = new EVENT_USER_T();
            vENT_USER_T.REGISTER_ID = eVENT_REGISTRATION_T.ID;
            vENT_USER_T.COMP_NAME_TX = eventsRegistration.CompanyName;
            vENT_USER_T.USER_ID = eventsRegistration.UserId;
            vENT_USER_T.BILLING_ADDRESS_TX = eventsRegistration.BillingAdd;
            vENT_USER_T.SHIPPING_ADDRESS_TX = eventsRegistration.ShipingAdd;
            vENT_USER_T.PASSWORD_TX = eventsRegistration.Password;
            vENT_USER_T.MOB_NM_TX = eventsRegistration.MobileNo;
            vENT_USER_T.PIN_CODE_TX = eventsRegistration.PinCode;
            vENT_USER_T.STATE_CODE_NM = eventsRegistration.StateCode;
            vENT_USER_T.CITY_NM = eventsRegistration.CityId;
            vENT_USER_T.EMAIL_TX = eventsRegistration.Email;
            vENT_USER_T.EVENT_ID = eventsRegistration.EventId;
            vENT_USER_T.GSTIN = eventsRegistration.GSTN;
            vENT_USER_T.ACTIVE_YN = true;
            vENT_USER_T.CREATED_BY = 1;
            vENT_USER_T.CREATED_DT = DateTime.Now;
            vENT_USER_T.UPDATED_BY = 1;
            vENT_USER_T.UPDATED_DT = DateTime.Now;
            DBcontext.EVENT_USER_T.Add(vENT_USER_T);
            DBcontext.SaveChanges();

            return true;
        }

        public List<Event> GetAllEvents()
        {
            return DBcontext.EVENT_T.ToList().Select(x => new Event
            {
                Id = x.ID,
                Name = x.EVENT_NAME_TX
            }).ToList();
        }

        public bool ExistRegistration(EventsRegistration eventsRegistration)
        {
            List<EVENT_REGISTRATION_T> lst = this.DBcontext.EVENT_REGISTRATION_T.Where(x => x.USER_ID == eventsRegistration.UserId).ToList();
            if (lst != null && lst.Count > 0)

                return true;
            else
                return false;
        }

        public List<State> GetAllState()
        {
            return DBcontext.STATE_T.OrderBy(x=>x.NAME_TX).ToList().Select(x => new State
            {
                Id = x.ID,
                Name = x.NAME_TX,
                State_Code = x.STATE_CODE_TX
                
            }).ToList();
        }
        public List<City> GetALLCity(int SatetId)
        {
            return DBcontext.CITY_T.Where(x => x.STATE_ID == SatetId).ToList().Select(x => new City
            {
                Id = x.ID,
                Name = x.NAME
            }).ToList();
        }

        public string GetStateCode(int SatetId)
        {
            return DBcontext.STATE_T.Where(x => x.ID == SatetId).Select(y => y.STATE_CODE_TX).FirstOrDefault();
        }

        public bool AddStallNumber(StallNumber stallNumber)
        {
            STALL_T sTALL_NUMBER_T = new STALL_T();
            sTALL_NUMBER_T.STALL_NUMBER_NM = stallNumber.StallNo;
            sTALL_NUMBER_T.STALL_DESCRIPTION_TX = stallNumber.StallDescription;
            sTALL_NUMBER_T.AMOUNT_NM = Math.Round(stallNumber.StallAmount, 2);
            sTALL_NUMBER_T.STATUS_TX = 0;
            sTALL_NUMBER_T.CREATED_BY = 1;
            sTALL_NUMBER_T.CREATED_DT = DateTime.Now;
            sTALL_NUMBER_T.UPDATED_BY = 1;
            sTALL_NUMBER_T.UPDATED_DT = DateTime.Now;
            sTALL_NUMBER_T.ACTIVE_YN = true;
            this.DBcontext.STALL_T.Add(sTALL_NUMBER_T);
            this.DBcontext.SaveChanges();
            return true;

        }

        public List<StallNumber> GetAllStallNumber()
        {
            //DateTime stallupdatedtime = this.DBcontext.STALL_T.Where(x => x.STATUS_TX == 2).Select(s => s.UPDATED_DT).FirstOrDefault();
            //DateTime dt = DateTime.Now;
            //DateTime time1 = DateTime.Parse(stallupdatedtime.ToString());
            //DateTime time2 = DateTime.Parse(dt.ToString());
            //double totalminutes = (time2 - time1).TotalMinutes;
            //if (totalminutes > 10)
            //{
            //STALL_T st = new STALL_T();
            //st = this.DBcontext.STALL_T.Where(x => x.STATUS_TX == 2).FirstOrDefault();
            //st.STATUS_TX = 0;
            //DBcontext.SaveChanges();

            //STALL_T statuspdate = (from x in DBcontext.STALL_T
            //                       where x.STATUS_TX == 2
            //             select x).First();
            //statuspdate.STATUS_TX = 0;
            //DBcontext.SaveChanges();

            //}


            return (
                from sn in this.DBcontext.STALL_T.AsEnumerable()
                //where (!this.DBcontext.EVENT_REGISTRATION_T.AsEnumerable().Any(x => x.Stall_Nmber_Id == sn.ID))
                //|| (sn.STATUS_TX != 3)




                select new StallNumber
                {
                    Id = sn.ID,
                    StallAmount = Math.Round(Convert.ToDecimal(sn.AMOUNT_NM), 2),
                    StallNo = sn.STALL_NUMBER_NM,
                    StallDescription = sn.STALL_DESCRIPTION_TX,
                    Status = Convert.ToInt32(sn.STATUS_TX)
                }).ToList();


        }

        //for Diary
        public List<StallNumber> GetAllDIARYNumber()
        {
          
            return (
                from sn in this.DBcontext.DIARY_T.AsEnumerable()
                //where (!this.DBcontext.DIARY_REGISTRATION_T.AsEnumerable().Any(x => x.DIARY_Nmber_Id == sn.ID))
                //|| (sn.STATUS_TX != 3)

                select new StallNumber
                {
                    Id = sn.ID,
                    StallAmount = Math.Round(Convert.ToDecimal(sn.AMOUNT_NM), 2),
                    StallNo = sn.DIARY_NUMBER_NM,
                    StallDescription = sn.DIARY_DESCRIPTION_TX,
                    Status = Convert.ToInt32(sn.STATUS_TX)
                }).ToList();

        }

        public int StallPayment(StallBooking stallBooking)
        {
            EVENT_REGISTRATION_T eVENT_REGISTRATION_T = new EVENT_REGISTRATION_T();
            eVENT_REGISTRATION_T.COMP_NAME_TX = stallBooking.CompanyName;
            eVENT_REGISTRATION_T.Stall_Nmber_Id = stallBooking.StallNumberId;
            eVENT_REGISTRATION_T.USER_ID = stallBooking.UserId;
            eVENT_REGISTRATION_T.CITY_TX = stallBooking.City;
            eVENT_REGISTRATION_T.BILLING_ADDRESS_TX = stallBooking.Add1;
            eVENT_REGISTRATION_T.SHIPPING_ADDRESS_TX = stallBooking.Add2;
            eVENT_REGISTRATION_T.PASSWORD_TX = stallBooking.Password;
            eVENT_REGISTRATION_T.MOB_NM_TX = stallBooking.MobileNo;
            eVENT_REGISTRATION_T.PIN_CODE_TX = stallBooking.PinCode;
            eVENT_REGISTRATION_T.STATE_CODE_NM = stallBooking.StateId;
            eVENT_REGISTRATION_T.CITY_TX = stallBooking.City;
            eVENT_REGISTRATION_T.EMAIL_TX = stallBooking.Email;
            eVENT_REGISTRATION_T.EVENT_ID = stallBooking.EventId;
            eVENT_REGISTRATION_T.GSTIN = stallBooking.GSTN;
            eVENT_REGISTRATION_T.APPROVED_YN = stallBooking.Approved;
            eVENT_REGISTRATION_T.STALLDESCRIPTION = stallBooking.StatllDescription;
            eVENT_REGISTRATION_T.CONTACTPERSON = stallBooking.ContactPersion;
            eVENT_REGISTRATION_T.AMOUNT = stallBooking.Amount;
            eVENT_REGISTRATION_T.GSTAMOUNT = stallBooking.GST18Amount;
            string GSTType = Config.AppConfig.GetGST(Convert.ToString(stallBooking.StateCode), "09");
            eVENT_REGISTRATION_T.IGSTAMOUNT = stallBooking.IGSTAmount;

            eVENT_REGISTRATION_T.SGSTAMOUNT = stallBooking.SGSTAmount;
            eVENT_REGISTRATION_T.CGSTAMOUNT = stallBooking.CGSTAmount;

            eVENT_REGISTRATION_T.TOTALAMOUNT = stallBooking.TotalAmount;
            eVENT_REGISTRATION_T.ACTIVE_YN = true;
            eVENT_REGISTRATION_T.APPROVED_YN = true;
            eVENT_REGISTRATION_T.CREATED_BY = 1;
            eVENT_REGISTRATION_T.CREATED_DT = DateTime.Now;
            eVENT_REGISTRATION_T.UPDATED_BY = 1;
            eVENT_REGISTRATION_T.UPDATED_DT = DateTime.Now;            
            DBcontext.EVENT_REGISTRATION_T.Add(eVENT_REGISTRATION_T);
            DBcontext.SaveChanges();

            STALL_TRANSACTION_T sTALL_TRANSACTION_DETAIL = new STALL_TRANSACTION_T();

            // string numericPhone = new String(phone.Where(Char.IsDigit).ToArray());
            sTALL_TRANSACTION_DETAIL.TOTALAMOUNT = stallBooking.TotalAmount;
            sTALL_TRANSACTION_DETAIL.EVENTREGISTRATION_ID = eVENT_REGISTRATION_T.ID;
            sTALL_TRANSACTION_DETAIL.REQUEST_ID = "CDA" + GenerateId(eVENT_REGISTRATION_T.ID);
            sTALL_TRANSACTION_DETAIL.TRANSACTION_ID = "Tran" + GenerateId(eVENT_REGISTRATION_T.ID);
            sTALL_TRANSACTION_DETAIL.INVOICE_NO = "CDA" + GenerateId(eVENT_REGISTRATION_T.ID);
            sTALL_TRANSACTION_DETAIL.INVOICE_DT = DateTime.Now;
            sTALL_TRANSACTION_DETAIL.SAC_CODE = stallBooking.SACCODE;
            sTALL_TRANSACTION_DETAIL.ACTIVE_YN = true;
            sTALL_TRANSACTION_DETAIL.CREATED_BY = 1;
            sTALL_TRANSACTION_DETAIL.CREATED_DT = DateTime.Now;
            sTALL_TRANSACTION_DETAIL.UPDATED_BY = 1;
            sTALL_TRANSACTION_DETAIL.UPDATED_DT = DateTime.Now;
            DBcontext.STALL_TRANSACTION_T.Add(sTALL_TRANSACTION_DETAIL);
            DBcontext.SaveChanges();

            //book stall status change
            STALL_T sTALL_T = this.DBcontext.STALL_T.Where(x => x.ID == eVENT_REGISTRATION_T.Stall_Nmber_Id).FirstOrDefault();
            if (sTALL_T != null)
            {
                sTALL_T.STATUS_TX = 1;
                sTALL_T.UPDATED_DT = DateTime.Now;
                sTALL_T.UPDATED_BY = 1;
                DBcontext.SaveChanges();
            }

            return eVENT_REGISTRATION_T.ID;
        }

        //Diary Payment
        public int DiaryPayment(DiaryBooking DiaryBooking)
        {
            DIARY_REGISTRATION_T dIARY_REGISTRATION_T = new DIARY_REGISTRATION_T();
            dIARY_REGISTRATION_T.COMP_NAME_TX = HttpContext.Current.Session["FullName"].ToString(); //DiaryBooking.MembershipName;
            dIARY_REGISTRATION_T.DIARY_Nmber_Id = DiaryBooking.StallNumberId;
            dIARY_REGISTRATION_T.USER_ID = HttpContext.Current.Session["MembershipNo"].ToString();  //DiaryBooking.UserId;
            dIARY_REGISTRATION_T.CITY_TX = DiaryBooking.City;

            dIARY_REGISTRATION_T.DiaryPrint_Name1 = DiaryBooking.DiaryPName1;
            dIARY_REGISTRATION_T.DiaryPrint_Name2 = DiaryBooking.DiaryPName2;
            dIARY_REGISTRATION_T.DiaryPrint_Name3 = DiaryBooking.DiaryPName3;
            dIARY_REGISTRATION_T.DiaryPrint_Name4 = DiaryBooking.DiaryPName4;
            dIARY_REGISTRATION_T.DiaryPrint_Name5 = DiaryBooking.DiaryPName5;

            dIARY_REGISTRATION_T.BILLING_ADDRESS_TX = DiaryBooking.Add1;
            dIARY_REGISTRATION_T.SHIPPING_ADDRESS_TX = DiaryBooking.Add2;
            dIARY_REGISTRATION_T.PASSWORD_TX = DiaryBooking.Password;
            dIARY_REGISTRATION_T.MOB_NM_TX = DiaryBooking.MobileNo;
            dIARY_REGISTRATION_T.PIN_CODE_TX = DiaryBooking.PinCode;
            dIARY_REGISTRATION_T.STATE_CODE_NM = DiaryBooking.StateId;
            dIARY_REGISTRATION_T.CITY_TX = DiaryBooking.City;
            dIARY_REGISTRATION_T.EMAIL_TX = DiaryBooking.Email;
            dIARY_REGISTRATION_T.DIARY_ID = DiaryBooking.EventId;
            dIARY_REGISTRATION_T.GSTIN = DiaryBooking.GSTN;
            dIARY_REGISTRATION_T.APPROVED_YN = DiaryBooking.Approved;
            dIARY_REGISTRATION_T.DIARYDESCRIPTION = "ICSI Diary 2020";
            dIARY_REGISTRATION_T.CONTACTPERSON = DiaryBooking.ContactPersion;
            dIARY_REGISTRATION_T.AMOUNT = DiaryBooking.Amount;
            dIARY_REGISTRATION_T.GSTAMOUNT = DiaryBooking.GST18Amount;
            string GSTType = Config.AppConfig.GetGST(Convert.ToString(DiaryBooking.StateCode), "09");
            dIARY_REGISTRATION_T.IGSTAMOUNT = DiaryBooking.IGSTAmount;

            dIARY_REGISTRATION_T.SGSTAMOUNT = DiaryBooking.SGSTAmount;
            dIARY_REGISTRATION_T.CGSTAMOUNT = DiaryBooking.CGSTAmount;

            dIARY_REGISTRATION_T.TOTALAMOUNT = DiaryBooking.TotalAmount;
            dIARY_REGISTRATION_T.ACTIVE_YN = true;
            dIARY_REGISTRATION_T.APPROVED_YN = true;
            dIARY_REGISTRATION_T.CREATED_BY = 1;
            dIARY_REGISTRATION_T.CREATED_DT = DateTime.Now;
            dIARY_REGISTRATION_T.UPDATED_BY = 1;
            dIARY_REGISTRATION_T.UPDATED_DT = DateTime.Now;
            DBcontext.DIARY_REGISTRATION_T.Add(dIARY_REGISTRATION_T);
            DBcontext.SaveChanges();

            DIARY_TRANSACTION_T dIARY_TRANSACTION_DETAIL = new DIARY_TRANSACTION_T();

            // string numericPhone = new String(phone.Where(Char.IsDigit).ToArray());
            dIARY_TRANSACTION_DETAIL.TOTALAMOUNT = DiaryBooking.TotalAmount;
            dIARY_TRANSACTION_DETAIL.DIARYREGISTRATION_ID = dIARY_REGISTRATION_T.ID;
            dIARY_TRANSACTION_DETAIL.REQUEST_ID = "CDA" + GenerateId(dIARY_REGISTRATION_T.ID);
            dIARY_TRANSACTION_DETAIL.TRANSACTION_ID = "Tran" + GenerateId(dIARY_REGISTRATION_T.ID);
            //dIARY_TRANSACTION_DETAIL.TRANSACTION_ICSI_ID = HttpContext.Current.Session["RequetID"].ToString(); 
            dIARY_TRANSACTION_DETAIL.INVOICE_NO = "CDA" + GenerateId(dIARY_REGISTRATION_T.ID);
            dIARY_TRANSACTION_DETAIL.INVOICE_DT = DateTime.Now;
            dIARY_TRANSACTION_DETAIL.SAC_CODE = DiaryBooking.SACCODE;
            dIARY_TRANSACTION_DETAIL.ACTIVE_YN = true;
            dIARY_TRANSACTION_DETAIL.CREATED_BY = 1;
            dIARY_TRANSACTION_DETAIL.CREATED_DT = DateTime.Now;
            dIARY_TRANSACTION_DETAIL.UPDATED_BY = 1;
            dIARY_TRANSACTION_DETAIL.UPDATED_DT = DateTime.Now;
            DBcontext.DIARY_TRANSACTION_T.Add(dIARY_TRANSACTION_DETAIL);
            DBcontext.SaveChanges();

            //book stall status change
            DIARY_T dIARY_T = this.DBcontext.DIARY_T.Where(x => x.ID == dIARY_REGISTRATION_T.DIARY_Nmber_Id).FirstOrDefault();
            if (dIARY_T != null)
            {
                dIARY_T.STATUS_TX = 1;
                dIARY_T.UPDATED_DT = DateTime.Now;
                dIARY_T.UPDATED_BY = 1;
                DBcontext.SaveChanges();
            }

            return dIARY_REGISTRATION_T.ID;
        }

        public List<BookedStall> BookedStall()
        {
             var result= (from er in this.DBcontext.EVENT_REGISTRATION_T.AsEnumerable()
                    join sn in this.DBcontext.STALL_T.AsEnumerable()
                    on er.Stall_Nmber_Id equals sn.ID
                  
                    select new BookedStall
                    {
                        Id = er.ID,
                        CompanyName = er.COMP_NAME_TX,
                        Amount = Convert.ToDecimal(er.AMOUNT),
                        TotalAmount = Convert.ToDecimal(er.TOTALAMOUNT)
       ,
                        StallNumber = sn.STALL_NUMBER_NM,
                        GST18Amount = Convert.ToDecimal(er.GSTAMOUNT),
                        status=sn.STATUS_TX
                    }).ToList();
          return  result.Where(x => x.status == 3).ToList();

        }

        public PaymentDetail GetPaymentDetail(int DiaryRegisterId)    // for Diary1
        {
            return (from er in this.DBcontext.DIARY_REGISTRATION_T.AsEnumerable()
                    join tr in this.DBcontext.DIARY_TRANSACTION_T.AsEnumerable()
                    on er.ID equals tr.DIARYREGISTRATION_ID
                    where er.ID == DiaryRegisterId
                    select new PaymentDetail
                    {
                        Id = er.ID,
                        RequestId = tr.REQUEST_ID,
                        TransactonId = tr.TRANSACTION_ID,
                        Name = er.CONTACTPERSON,
                        MobileNuber = er.MOB_NM_TX,
                        Email = er.EMAIL_TX,
                        TotalAmount = Math.Round(Convert.ToDecimal(er.TOTALAMOUNT), 2)
                    }).FirstOrDefault();

        }

        private int GenerateId(int number)
        {
            return number = number + 1;
        }

        public StallReceipt GetStallReceipt(int Id)
        {
            return (from er in this.DBcontext.EVENT_REGISTRATION_T.AsEnumerable()
                    join tr in this.DBcontext.STALL_TRANSACTION_T.AsEnumerable()
                    on er.ID equals tr.EVENTREGISTRATION_ID
                    join st in this.DBcontext.STATE_T.AsEnumerable()
                    on er.STATE_CODE_NM equals st.ID
                    join sr in this.DBcontext.STALL_T.AsEnumerable()
                    on er.Stall_Nmber_Id equals sr.ID
                    where er.ID == Id
                    select new StallReceipt
                    {
                        ID = er.ID,
                        INVOICE_NO = Convert.ToString(tr.INVOICE_NO),
                        INVOICE_DT = Convert.ToDateTime(tr.INVOICE_DT),
                        BILLING_ADDRESS_TX = er.BILLING_ADDRESS_TX,
                        SHIPPING_ADDRESS_TX = er.SHIPPING_ADDRESS_TX,
                        GSTIN = er.GSTIN,
                        CONTACTPERSON = er.CONTACTPERSON,
                        STALLDESCRIPTION = sr.STALL_DESCRIPTION_TX,  //er.STALLDESCRIPTION,
                        SAC_CODE = tr.SAC_CODE,
                        AMOUNT = Convert.ToDecimal(er.AMOUNT),
                        GSTAMOUNT = Convert.ToDecimal(er.GSTAMOUNT),
                        IGSTAmount = Convert.ToDecimal(er.IGSTAMOUNT),
                        CGSTAmount = Convert.ToDecimal(er.CGSTAMOUNT),
                        SGSTAmount = Convert.ToDecimal(er.SGSTAMOUNT),
                        TransactionId= tr.TRANSACTION_ID,
                        TOTALAMOUNT = Convert.ToDecimal(er.TOTALAMOUNT),
                        StallNumber = sr.STALL_NUMBER_NM,
                        RECEIPT_NO = tr.RECEIPT_NO,
                        RECEIPT_DT = Convert.ToDateTime(tr.RECEIPT_DT),
                        STATE_CODE = st.STATE_CODE_TX,
                        Stata_Name = st.NAME_TX,
                        CustomerName=er.COMP_NAME_TX
                    }).FirstOrDefault();

        }

        // for Diary 5

        public StallReceipt GetDIARYReceipt(int Id)
        {
            return (from er in this.DBcontext.DIARY_REGISTRATION_T.AsEnumerable()
                    join tr in this.DBcontext.DIARY_TRANSACTION_T.AsEnumerable()
                    on er.ID equals tr.DIARYREGISTRATION_ID
                    join st in this.DBcontext.STATE_T.AsEnumerable()
                    on er.STATE_CODE_NM equals st.ID
                    join sr in this.DBcontext.DIARY_T.AsEnumerable()
                    on er.DIARY_Nmber_Id equals sr.ID
                    where er.ID == Id
                    select new StallReceipt
                    {
                        ID = er.ID,
                        INVOICE_NO = Convert.ToString(tr.INVOICE_NO),
                        INVOICE_DT = Convert.ToDateTime(tr.INVOICE_DT),
                        BILLING_ADDRESS_TX = er.BILLING_ADDRESS_TX,
                        SHIPPING_ADDRESS_TX = er.SHIPPING_ADDRESS_TX,
                        GSTIN = er.GSTIN,
                        CONTACTPERSON = er.CONTACTPERSON,
                        STALLDESCRIPTION = sr.DIARY_DESCRIPTION_TX,  //er.STALLDESCRIPTION,
                        SAC_CODE = tr.SAC_CODE,
                        AMOUNT = Convert.ToDecimal(er.AMOUNT),
                        GSTAMOUNT = Convert.ToDecimal(er.GSTAMOUNT),
                        IGSTAmount = Convert.ToDecimal(er.IGSTAMOUNT),
                        CGSTAmount = Convert.ToDecimal(er.CGSTAMOUNT),
                        SGSTAmount = Convert.ToDecimal(er.SGSTAMOUNT),
                        TransactionId = tr.TRANSACTION_ID,
                        TOTALAMOUNT = Convert.ToDecimal(er.TOTALAMOUNT),
                        StallNumber = sr.DIARY_NUMBER_NM,
                        RECEIPT_NO = tr.RECEIPT_NO,
                        RECEIPT_DT = Convert.ToDateTime(tr.RECEIPT_DT),
                        STATE_CODE = st.STATE_CODE_TX,
                        Stata_Name = st.NAME_TX,
                        MembershiNumber=er.USER_ID,
                        CustomerName=er.COMP_NAME_TX,
                        ICSITransactionID_R = tr.TRANSACTION_ICSI_ID,
                        QtyDiaryNumber=Convert.ToInt32(er.DIARY_Nmber_Id),
                        PinCode = er.PIN_CODE_TX,
                        DCIty=er.CITY_TX

                    }).FirstOrDefault();

        }

        public bool CheckExistStallNo(string stallNo)
        {
            STALL_T sTALL_NUMBER_T = this.DBcontext.STALL_T.Where(x => x.STALL_NUMBER_NM == stallNo).FirstOrDefault();

            if (sTALL_NUMBER_T != null)
            {
                return true;
            }

            else
            {

                return false;
            }
        }
        public string GetSateByCode(string Code)
        {

            return this.DBcontext.STATE_T.Where(x => x.STATE_CODE_TX == Code).Select(x => x.ID).FirstOrDefault().ToString();
        }
   
        public string GetICSIGST()
        {
            return this.DBcontext.EVENT_T.Select(x => x.GSTIN).FirstOrDefault();
        }

        public int GetStallNumberId(int RegId)   
        {
            return Convert.ToInt32(this.DBcontext.EVENT_REGISTRATION_T.Where(x => x.ID == RegId).Select(x => x.Stall_Nmber_Id).FirstOrDefault());
        }


        public int GetDIARYNumberId(int RegId)   //for Diary 2
        {
            return Convert.ToInt32(this.DBcontext.DIARY_REGISTRATION_T.Where(x => x.ID == RegId).Select(x => x.DIARY_Nmber_Id).FirstOrDefault());
        }

        
        public void UpdateStallStatus(int Id)
        {
            STALL_T sTALL_T = this.DBcontext.STALL_T.Where(x => x.ID == Id).FirstOrDefault();
            if (sTALL_T != null)
            {
                sTALL_T.STATUS_TX = 2;
                sTALL_T.UPDATED_DT = DateTime.Now;
                sTALL_T.UPDATED_BY = 1;
                this.DBcontext.SaveChanges();
            }
        }

        public void UpdateDIARYStatus(int Id)
        {
            DIARY_T sTALL_T = this.DBcontext.DIARY_T.Where(x => x.ID == Id).FirstOrDefault();
            if (sTALL_T != null)
            {
                sTALL_T.STATUS_TX = 2;
                sTALL_T.UPDATED_DT = DateTime.Now;
                sTALL_T.UPDATED_BY = 1;
                this.DBcontext.SaveChanges();
            }
        }

        public void UpdateStallStatus(int Id, int Status)
        {
            STALL_T sTALL_T = this.DBcontext.STALL_T.Where(x => x.ID == Id).FirstOrDefault();
            if (sTALL_T != null)
            {
                sTALL_T.STATUS_TX = Status;
                sTALL_T.UPDATED_DT = DateTime.Now;
                sTALL_T.UPDATED_BY = 1;
                this.DBcontext.SaveChanges();
            }
        }

        // for Diary 3
        public void UpdateDIARYStatus(int Id, int Status)
        {
            DIARY_T dIARY_T = this.DBcontext.DIARY_T.Where(x => x.ID == Id).FirstOrDefault();
            if (dIARY_T != null)
            {
                dIARY_T.STATUS_TX = Status;
                dIARY_T.UPDATED_DT = DateTime.Now;
                dIARY_T.UPDATED_BY = 1;
                this.DBcontext.SaveChanges();
            }
        }

        public int GetRegId(int PayReqId)
        {
            return Convert.ToInt32(this.DBcontext.tblPaymentDetails.Where(x => x.Id == PayReqId).Select(x => x.ProcessRequestId).FirstOrDefault());
        }

        public void Update_STALL_TRANSACTION_T_ID(int Id, string txnid, bool status)
        {
            STALL_TRANSACTION_T sTALL_TRANSACTION_T = this.DBcontext.STALL_TRANSACTION_T.Where(x => x.EVENTREGISTRATION_ID == Id).FirstOrDefault();
            if (sTALL_TRANSACTION_T != null)
            {
                sTALL_TRANSACTION_T.TRANSACTION_ID = txnid;
                sTALL_TRANSACTION_T.STATUS = status;
                this.DBcontext.SaveChanges();
            }
        }

        // for Diary 4 
        public void Update_DIARY_TRANSACTION_T_ID(int Id, string txnid, bool status,string request_id)
        {
            DIARY_TRANSACTION_T dIARY_TRANSACTION_T = this.DBcontext.DIARY_TRANSACTION_T.Where(x => x.DIARYREGISTRATION_ID == Id).FirstOrDefault();
            if (dIARY_TRANSACTION_T != null)
            {
                dIARY_TRANSACTION_T.TRANSACTION_ID = txnid;
                dIARY_TRANSACTION_T.STATUS = status;
                dIARY_TRANSACTION_T.TRANSACTION_ICSI_ID= request_id;
                this.DBcontext.SaveChanges();
            }
        }

        public void UpdateStallStatusTimeMore10Mints()
        {
            List<STALL_T> lst = this.DBcontext.STALL_T.Where(x => x.STATUS_TX == 4 || x.STATUS_TX == 1 || x.STATUS_TX == 2).ToList();
            if (lst != null)
            {
                foreach (STALL_T sTALL_T in lst)
                {
                    double totalminutes = (DateTime.Now - sTALL_T.UPDATED_DT).TotalMinutes;
                    if (totalminutes > 10)
                    {
                        sTALL_T.STATUS_TX = 0;
                        this.DBcontext.SaveChanges();
                    }

                }
            }
        }

        // for Diary 6  
        public void UpdateDIARYStatusTimeMore10Mints()
        {
            List<DIARY_T> lst = this.DBcontext.DIARY_T.Where(x => x.STATUS_TX == 4 || x.STATUS_TX == 1 || x.STATUS_TX == 2).ToList();
            if (lst != null)
            {
                foreach (DIARY_T dIARY_T in lst)
                {
                    double totalminutes = (DateTime.Now - dIARY_T.UPDATED_DT).TotalMinutes;
                    if (totalminutes > 10)
                    {
                        dIARY_T.STATUS_TX = 0;
                        this.DBcontext.SaveChanges();
                    }

                }
            }
        }


        //public ReceiptDetails GetReceiptTransaction(SearchReceiptTransaction obj)
        //{

        //    var result = (from er in this.DBcontext.EVENT_REGISTRATION_T.AsEnumerable()

        //                  join st in this.DBcontext.STATE_T.AsEnumerable()
        //                  on er.STATE_CODE_NM equals st.ID
        //                  join sr in this.DBcontext.STALL_T.AsEnumerable()
        //                  on er.Stall_Nmber_Id equals sr.ID
        //                  join tr in this.DBcontext.STALL_TRANSACTION_T.AsEnumerable()
        //                  on er.ID equals tr.EVENTREGISTRATION_ID
        //                  where tr.TRANSACTION_ID == obj.TransactionID
        //                  select new ReceiptDetails
        //                  {
        //                      ID = er.ID,
        //                      INVOICE_NO = Convert.ToString(tr.INVOICE_NO),
        //                      INVOICE_DT = Convert.ToDateTime(tr.INVOICE_DT),
        //                      BILLING_ADDRESS_TX = er.BILLING_ADDRESS_TX,
        //                      SHIPPING_ADDRESS_TX = er.SHIPPING_ADDRESS_TX,
        //                      GSTIN = er.GSTIN,
        //                      CONTACTPERSON = er.CONTACTPERSON,
        //                      STALLDESCRIPTION = sr.STALL_DESCRIPTION_TX,  //er.STALLDESCRIPTION,
        //                      SAC_CODE = tr.SAC_CODE,
        //                      AMOUNT = Convert.ToDecimal(er.AMOUNT),
        //                      GSTAMOUNT = Convert.ToDecimal(er.GSTAMOUNT),
        //                      IGSTAmount = Convert.ToDecimal(er.IGSTAMOUNT),
        //                      CGSTAmount = Convert.ToDecimal(er.CGSTAMOUNT),
        //                      SGSTAmount = Convert.ToDecimal(er.SGSTAMOUNT),
        //                      TransactionId = tr.TRANSACTION_ID,
        //                      TOTALAMOUNT = Convert.ToDecimal(er.TOTALAMOUNT),
        //                      StallNumber = sr.STALL_NUMBER_NM,
        //                      RECEIPT_NO = tr.RECEIPT_NO,
        //                      RECEIPT_DT = Convert.ToDateTime(tr.RECEIPT_DT),
        //                      STATE_CODE = st.STATE_CODE_TX,
        //                      Stata_Name = st.NAME_TX
        //                  }).ToList<ReceiptDetails>();

        //    if (result != null && result.Count > 0)
        //        return result[0];
        //    else
        //        return null;
        //}

        // for Diary
        public ReceiptDetails GetReceiptTransaction(SearchReceiptTransaction obj)
        {

            var result = (from er in this.DBcontext.DIARY_REGISTRATION_T.AsEnumerable()

                          join st in this.DBcontext.STATE_T.AsEnumerable()
                          on er.STATE_CODE_NM equals st.ID
                          join sr in this.DBcontext.DIARY_T.AsEnumerable()
                          on er.DIARY_Nmber_Id equals sr.ID
                          join tr in this.DBcontext.DIARY_TRANSACTION_T.AsEnumerable()
                          on er.ID equals tr.DIARYREGISTRATION_ID
                          //where tr.TRANSACTION_ID == obj.TransactionID
                          where tr.TRANSACTION_ICSI_ID == obj.TransactionID && tr.STATUS == true
                          select new ReceiptDetails
                          {
                              ID = er.ID,
                              INVOICE_NO = Convert.ToString(tr.INVOICE_NO),
                              INVOICE_DT = Convert.ToDateTime(tr.INVOICE_DT),
                              BILLING_ADDRESS_TX = er.BILLING_ADDRESS_TX,
                              SHIPPING_ADDRESS_TX = er.SHIPPING_ADDRESS_TX,
                              GSTIN = er.GSTIN,
                              CONTACTPERSON = er.CONTACTPERSON,
                              STALLDESCRIPTION = sr.DIARY_DESCRIPTION_TX,  //er.STALLDESCRIPTION,
                              SAC_CODE = tr.SAC_CODE,
                              AMOUNT = Convert.ToDecimal(er.AMOUNT),
                              GSTAMOUNT = Convert.ToDecimal(er.GSTAMOUNT),
                              IGSTAmount = Convert.ToDecimal(er.IGSTAMOUNT),
                              CGSTAmount = Convert.ToDecimal(er.CGSTAMOUNT),
                              SGSTAmount = Convert.ToDecimal(er.SGSTAMOUNT),
                              TransactionId = tr.TRANSACTION_ID,
                              TOTALAMOUNT = Convert.ToDecimal(er.TOTALAMOUNT),
                              StallNumber = sr.DIARY_NUMBER_NM,
                              RECEIPT_NO = tr.RECEIPT_NO,
                              RECEIPT_DT = Convert.ToDateTime(tr.RECEIPT_DT),
                              STATE_CODE = st.STATE_CODE_TX,
                              Stata_Name = st.NAME_TX,
                              CustomerName = er.COMP_NAME_TX,
                              MembershipNo = er.USER_ID,
                              ICSITransactionID_R=tr.TRANSACTION_ICSI_ID,
                              QtyDiaryNumber = Convert.ToInt32(er.DIARY_Nmber_Id),
                              PinCode=er.PIN_CODE_TX,
                              DCIty = er.CITY_TX


                          }).ToList<ReceiptDetails>();

            if (result != null && result.Count > 0)
                return result[0];
            else
                return null;
        }


       
        public string GetDIARYDes(string StallNumber)
        {
            var reseDesc = DBcontext.DIARY_T.Where(x => x.DIARY_NUMBER_NM == StallNumber).FirstOrDefault();
            if (reseDesc != null)
                return reseDesc.DIARY_DESCRIPTION_TX;

            return null;
        }

         
        public void sendMail(string MailTo, string Subject, string Body)
        {
            try
            {
                //MailTo = "akumar@gemini-us.com";
                string host = ConfigurationManager.AppSettings["host"].ToString();
                int port = Convert.ToInt32(ConfigurationManager.AppSettings["port"]);
                string username = ConfigurationManager.AppSettings["username"].ToString();
                string password = ConfigurationManager.AppSettings["password"].ToString();

                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient(host);

                mail.From = new MailAddress(username);
                mail.To.Add(MailTo);
                mail.Subject = Subject;
                mail.Body = Body;
                mail.IsBodyHtml = true;

                SmtpServer.Port = port;
                SmtpServer.Credentials = new System.Net.NetworkCredential(username, password);
                SmtpServer.EnableSsl = false;

                SmtpServer.Send(mail);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public DIARY_REGISTRATION_T GetEmailTransByID(string MemebershipNumber)
        {
            return DBcontext.DIARY_REGISTRATION_T.Where(x => x.USER_ID == MemebershipNumber)
                             .OrderByDescending(x=> x.ID)
                             .Take(1)
                             .First();                        
        }

        public DIARY_REGISTRATION_T GetEmailTransByID(int regid)
        {
            return DBcontext.DIARY_REGISTRATION_T.Where(x => x.ID == regid).FirstOrDefault();                             
        }
    }
}