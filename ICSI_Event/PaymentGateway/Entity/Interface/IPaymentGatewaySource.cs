using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentGateway.Entity.Interface
{
    public interface IPaymentGatewaySource
    {
        string GetRequestData(PaymentTransactionEntity objPmtEntity, PaymentGatewayMaster objPG);
        string GetEncryptedRequestData(PaymentTransactionEntity objPmtEntity, PaymentGatewayMaster objPG);
        string encrypttext(string key, string textToEncrypt);
    }
}
