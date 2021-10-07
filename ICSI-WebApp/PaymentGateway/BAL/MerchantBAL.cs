using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PaymentGateway.Entity;
using ICSI_WebApp.Util;

namespace PaymentGateway.BAL
{
      public class MerchantBAL
        {
            public PaymentGatewayMaster GetMerchant(string PGType)
            {
                PaymentGatewayMaster objPW = new PaymentGatewayMaster();   
           
            //////*********COMMENTED BELOW CODE AS WE ARE NOT USING THIS PAYMENT GATEWAY STRUCTURE RIGHTN NOW**********/////////
            
                //if (DBTable.PAYMENT_GW_MASTER_T.Rows.Count > 0)
                //{
                //    PaymentGatewayMaster objPG = new PaymentGatewayMaster
                //    {
                //        GateWayCode = DBTable.PAYMENT_GW_MASTER_T.Rows[0]["PGCODE_TX"].ToString(),
                //        GatewayName = DBTable.PAYMENT_GW_MASTER_T.Rows[0]["PGNAME_TX"].ToString(),                        
                //        GatewayURL = DBTable.PAYMENT_GW_MASTER_T.Rows[0]["GATEWAY_URL_TX"].ToString(),
                //        CheckSumKey = DBTable.PAYMENT_GW_MASTER_T.Rows[0]["CHECKSUM_KEY_TX"].ToString(),
                //        MerchantId = DBTable.PAYMENT_GW_MASTER_T.Rows[0]["MERCHANT_ID_TX"].ToString(),                        
                //        MobNo = "9999999999",
                //        RefNo = DBTable.PAYMENT_GW_MASTER_T.Rows[0]["REFERENCE_NO_TX"].ToString(),
                //        ResponseURL = DBTable.PAYMENT_GW_MASTER_T.Rows[0]["RETURN_URL_TX"].ToString(),
                //        SubMerchantID = DBTable.PAYMENT_GW_MASTER_T.Rows[0]["SUBMERCHANT_ID_TX"].ToString()
                //    };
                //    objPW = objPG;
                //}
                return objPW;
            }
        }
    
}
