using System;
using System.Collections.Generic;
using System.Text;
using PaymentGateway.Entity;

namespace PaymentGateway.BAL
{
    public class PaymentGatewayData
    {
        public PaymentTransactionEntity GetPaymentRequestData(string connectionStringPG1, string connectionStringPG2,string mobileNumber,string TotAmount, string StallID, string PayMode, string Route, string PGType, ref string ReqMsg, string MobileNo, string Name, int reqid)
        {
            PaymentTransactionEntity objPmtEntity = new PaymentTransactionEntity();
            MerchantBAL objMerchantBAL = new MerchantBAL();
            PayMentBal objPayment = new PayMentBal();
            PaymentGatewayMaster objPG = objMerchantBAL.GetMerchant(connectionStringPG1,mobileNumber,PGType);
            objPG.Name = Name;
            PGResourceClient objPGClient = new PGResourceClient(objPG);
            objPG.MobNo = MobileNo;
            objPmtEntity.Amount = Convert.ToDouble(TotAmount);
            objPmtEntity.CreatedBy = 1;
            objPmtEntity.CreatedDate = DateTime.Now;
            objPmtEntity.PaymentMode = PayMode;
            objPmtEntity.PaymentStatusID = Convert.ToInt32(ePaymentStatus.Draft);
            objPmtEntity.ProcessRequestID = Convert.ToInt32(StallID);
            objPmtEntity.PGCode = objPG.GateWayCode;
            objPmtEntity.ProcessRoute = Route;
            objPmtEntity.RequestID = reqid;
            ReqMsg = objPGClient.GetRequestData(objPmtEntity, objPG);
            objPmtEntity.RequestData = ReqMsg;
            ReqMsg = objPGClient.GetEncryptedRequestData(objPmtEntity, objPG);

            if(!string.IsNullOrEmpty(ReqMsg))
            {
                int o = objPayment.UpdatePaymentStatusBal(connectionStringPG2,Convert.ToInt32(ePaymentStatus.Draft), reqid);
            }
            return objPmtEntity;
        }

        public int InsertOnlinePaymentTransaction(string connectionStringPG1, string connectionStringPG2, string mobileNumber, string TotAmount, string StallID, string PayMode, string Route, string PGType, string MobileNo, string Name)
        {
            PaymentTransactionEntity objPmtEntity = new PaymentTransactionEntity();
            MerchantBAL objMerchantBAL = new MerchantBAL();
            PayMentBal objPayment = new PayMentBal();
            PaymentGatewayMaster objPG = objMerchantBAL.GetMerchant(connectionStringPG1,mobileNumber,PGType);
            objPG.Name = Name;
            PGResourceClient objPGClient = new PGResourceClient(objPG);
            objPG.MobNo = MobileNo;
            objPmtEntity.Amount = Convert.ToDouble(TotAmount);
            objPmtEntity.CreatedBy = 1;
            objPmtEntity.CreatedDate = DateTime.Now;
            objPmtEntity.PaymentMode = PayMode;
            objPmtEntity.PaymentStatusID = Convert.ToInt32(ePaymentStatus.Init);
            objPmtEntity.ProcessRequestID = Convert.ToInt32(StallID);
            objPmtEntity.PGCode = objPG.GateWayCode;
            objPmtEntity.ProcessRoute = Route;
            objPmtEntity.RequestID = objPayment.ExecutePaymentBal(connectionStringPG2,objPmtEntity);            
            return objPmtEntity.RequestID;
        }
    }
}
