using System;
using System.Collections.Generic;
using System.Text;
using PaymentGateway.Entity;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Configuration;

namespace PaymentGateway.DAL
{
    public class MerChentDAL
    {
        // PG1
        public PaymentGatewayMaster GetPGURLData(string coonectionString, string mobileNumber, string PGType)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(coonectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("EXEC USPGetPGSourceData '" + PGType + "'", con))
                    {
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataSet ds = new DataSet();
                        da.Fill(ds);
                        if (ds.Tables.Count > 0)
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                PaymentGatewayMaster objPG = new PaymentGatewayMaster
                                {
                                    GateWayCode = ds.Tables[0].Rows[0]["PGCode"].ToString(),
                                    GatewayName = ds.Tables[0].Rows[0]["PGName"].ToString(),
                                    GatewayURL = ds.Tables[0].Rows[0]["GatewayURL"].ToString(),
                                    CheckSumKey = ds.Tables[0].Rows[0]["CheckSumKey"].ToString(),
                                    MerchantId = ds.Tables[0].Rows[0]["MerchantId"].ToString(),
                                    MobNo = mobileNumber,// "9999999999",
                                    RefNo = ds.Tables[0].Rows[0]["RefernceNo"].ToString(),
                                    ResponseURL = ds.Tables[0].Rows[0]["ReturnURL"].ToString(),
                                    SubMerchantID = ds.Tables[0].Rows[0]["RefernceNo"].ToString()
                                };
                                return objPG;
                            }
                            else
                            {
                                return null;
                            }
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
