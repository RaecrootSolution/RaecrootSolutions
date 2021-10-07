using System;
using System.Collections.Generic;
using System.Text;
using PaymentGateway.Entity;
using PaymentGateway.DAL;

namespace PaymentGateway.BAL
{
    public class MerchantBAL
    {
        public PaymentGatewayMaster GetMerchant(string coonectionString, string mobileNumber, string PGType)
        {
            MerChentDAL objMerChent = new MerChentDAL();
            return objMerChent.GetPGURLData(coonectionString, mobileNumber,PGType);
        }
    }
}
