using System;
using System.Collections.Generic;
using System.Text;
using PaymentGateway.Entity;

namespace PaymentGateway.BAL
{
    public class PaymentGatewayData
    {
        public PaymentTransactionEntity GetPaymentRequestData(string TotAmount, string ReqID, string PayMode, string Route, string PGType,int transreqid, ref string ReqMsg)
        {
            PaymentTransactionEntity objPmtEntity = new PaymentTransactionEntity();
            MerchantBAL objMerchantBAL = new MerchantBAL();           
            PaymentGatewayMaster objPG = objMerchantBAL.GetMerchant(PGType);
            PGResourceClient objPGClient = new PGResourceClient(objPG);
            objPmtEntity.Amount = Convert.ToDouble(TotAmount);
            objPmtEntity.CreatedBy = 1;
            objPmtEntity.CreatedDate = DateTime.Now;
            objPmtEntity.PaymentMode = PayMode;
            objPmtEntity.PaymentStatusID = Convert.ToInt32(ePaymentStatus.Draft);
            objPmtEntity.ProcessRequestID = Convert.ToInt32(ReqID);
            objPmtEntity.PGCode = objPG.GateWayCode;
            objPmtEntity.ProcessRoute = Route;
            objPmtEntity.RequestID = transreqid;
            ReqMsg = objPGClient.GetRequestData(objPmtEntity, objPG);
            objPmtEntity.RequestData = ReqMsg;
            ReqMsg = objPGClient.GetEncryptedRequestData(objPmtEntity, objPG);
            return objPmtEntity;
        }
    }
}
