using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentGateway.Entity.Interface
{
    public interface IPaymentGatewaySource
    {
        string GetRequestData(PaymentTransactionEntity objPmtEntity, PaymentGatewayMaster objPG);
        string GetEncryptedRequestData(PaymentTransactionEntity objPmtEntity, PaymentGatewayMaster objPG);
        string encrypttext(string key, string textToEncrypt);
    }
}
