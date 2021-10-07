using System;
using System.Collections.Generic;
using System.Text;
using PaymentGateway.Entity;
using PaymentGateway.DAL;

namespace PaymentGateway.BAL
{
    public class PayMentBal
    {
        public int ExecutePaymentBal(string connectionString, PaymentTransactionEntity objPmtEntity)
        {
            PaymentDAL objPMDal = new PaymentDAL();
            return objPMDal.ExecutePaymentDal(connectionString,objPmtEntity);
        }

        public int UpdatePaymentBal(string connectionString,string TxnID, string TxnDate, string TxnCode, string TxnResponse, int RequestID, int paymentstatus,string paymode,double ST,double ProcFee,double totamt,double convfee)
        {
            PaymentDAL objPMDal = new PaymentDAL();
            return objPMDal.UpdatePaymentDal(connectionString,TxnID, TxnDate, TxnCode, TxnResponse, RequestID, paymentstatus,paymode,ST,ProcFee,totamt,convfee);
        }

        public int UpdatePaymentStatusBal(string connectionString,int paymentstatus, int RequestID)
        {
            PaymentDAL objPMDal = new PaymentDAL();
            return objPMDal.UpdatePaymentStatusDal(connectionString,paymentstatus, RequestID);
        }
        public string GetRegIdBAL(string connectionstringpg2, string regid)
        {
            PaymentDAL objPMDal = new PaymentDAL();
            return objPMDal.GetRegIdDAL(connectionstringpg2, regid);
        }
    }
}
