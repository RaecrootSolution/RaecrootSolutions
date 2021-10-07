using System;
using System.Collections.Generic;
using System.Text;
using PaymentGateway.Entity;
using PaymentGateway.Entity.Interface;

namespace PaymentGateway.BAL
{
    public class PGResourceClient
    {
        IPaymentGatewaySource objPGResource;
        public PGResourceClient(PaymentGatewayMaster objPG)
        {
            if (objPG.GateWayCode == Enum.GetName(typeof(Gateway), (int)Gateway.ICICI))
            {
                objPGResource = new ICICI();
            }
        }
        public string GetRequestData(PaymentTransactionEntity objPmtEntity, PaymentGatewayMaster objPG)
        {
            return objPGResource.GetRequestData(objPmtEntity, objPG);
        }
        public string GetEncryptedRequestData(PaymentTransactionEntity objPmtEntity, PaymentGatewayMaster objPG)
        {
            return objPGResource.GetEncryptedRequestData(objPmtEntity, objPG);
        }
        public string encrypttext(string key, string textToEncrypt)
        {
            return objPGResource.encrypttext(key, textToEncrypt);
        }
    }
}
