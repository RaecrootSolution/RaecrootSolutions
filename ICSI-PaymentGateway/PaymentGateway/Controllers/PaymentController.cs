using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data;
using PaymentGateway.Models;
using System.Collections;
using ICSI.PG.DB;
using System.Text;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using PaymentGateway.Util;
using System.Threading;

namespace PaymentGateway.Controllers
{

    public class PaymentController : ApiController
    {
        private Database _database;
        private Dictionary<string, ICSI.PG.DB.Database> _databases;
        public string strTransId = string.Empty;
        public StringBuilder sbResp = new StringBuilder();
        public SqlCommand sqlCmd = new SqlCommand();
        public Mutex mutext = new Mutex();
        public PaymentController()
        {
            DatabaseFactory.setConfiguration();
        }

        string strPaymentData = string.Empty;
        string strMessage = string.Empty;
        string strKey = string.Empty;
        string strUserID = string.Empty;
        string strOfficeID, strScreenId, strType = string.Empty;
        int iItemId = 0;
        DataTable dtData = new DataTable();
        public PGDetails pgDtls;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="strJsonData"></param>
        /// <returns></returns>
        [Route("api/Payment/Initiate")]
        [HttpPost]
        public object InitiateRequest(object strJsonData)
        {

            _database = DatabaseFactory.GetDatabase();
            Database tDb = DatabaseFactory.GetDatabase("Training");
            IDbConnection con = _database.CreateOpenConnection();
            IDbConnection trainigCon = tDb.CreateOpenConnection();

            string ptype = "";
            string scid = "";
            string sid = "";
            double amt = 0;
            string officeid = "";
            string desc = "";
            string custName = "";
            string email = "";
            string mobile = "";
            string billingAddr = "";
            string shippingAddr = "";
            string returnURL = "";
            string stdRegNo = "";
            double taxAmout = 0;
            string session = "";
            string gstnm = "";
            int refId = 0;
            string remarks = "";
            string additionalInfo1 = string.Empty;
            string additionalInfo2 = string.Empty;
            string additionalInfo3 = string.Empty;
            int recTmplId = 0;
            string loginId = string.Empty;
            Dictionary<string, object> items = null;
            List<SubItems> lstSubItems = new List<SubItems>();
            string iStateGSTCd = string.Empty;
            try
            {
                JObject req = null;
                req = JObject.Parse(strJsonData.ToString());
                if (req != null && req.HasValues && req.ContainsKey("ptype") && req.ContainsKey("scid") && req.ContainsKey("sid") && req.ContainsKey("amt") && req.ContainsKey("officeid") && req.ContainsKey("desc") && req.ContainsKey("items"))
                {
                    ptype = Convert.ToString(req.GetValue("ptype"));
                    scid = Convert.ToString(req.GetValue("scid"));
                    sid = Convert.ToString(req.GetValue("sid"));
                    amt = Convert.ToDouble(req.GetValue("amt"));
                    officeid = Convert.ToString(req.GetValue("officeid"));
                    custName = Convert.ToString(req.GetValue("custname"));
                    email = Convert.ToString(req.GetValue("email"));
                    mobile = Convert.ToString(req.GetValue("mobile"));
                    billingAddr = Convert.ToString(req.GetValue("billing"));
                    shippingAddr = Convert.ToString(req.GetValue("shipping"));
                    desc = Convert.ToString(req.GetValue("desc"));
                    taxAmout = Convert.ToDouble(req.GetValue("taxamt"));
                    gstnm = Convert.ToString(req.GetValue("gstnm"));
                    session = Convert.ToString(req.GetValue("session"));
                    remarks = Convert.ToString(req.GetValue("remarks")).Trim();
                    returnURL = Convert.ToString(req.GetValue("returnurl"));
                    stdRegNo = Convert.ToString(req.GetValue("stdregno"));
                    refId = Convert.ToInt32(req.GetValue("refId"));
                    additionalInfo1 = Convert.ToString(req.GetValue("addinfo1"));
                    additionalInfo2 = Convert.ToString(req.GetValue("addinfo2"));
                    additionalInfo3 = Convert.ToString(req.GetValue("addinfo3"));
                    loginId = Convert.ToString(req.GetValue("loginId"));
                    iStateGSTCd = Convert.ToString(req.GetValue("stategstcd"));
                    int.TryParse(Convert.ToString(req.GetValue("recTmplId")), out recTmplId);
                    if (recTmplId == 0) recTmplId = 1;
                    // customer name, email, mobile,billing addreess, shipping address

                    items = JsonConvert.DeserializeObject<Dictionary<string, object>>(Convert.ToString(req.GetValue("items")));
                    PGDetails pgDtls = new PGDetails();

                    //mutext.WaitOne();
                    pgDtls = ItemTransactions(items, session, Convert.ToInt32(scid), Convert.ToInt32(sid), officeid, amt,
                    taxAmout, custName, shippingAddr, billingAddr, email, mobile, gstnm, remarks, "Request", stdRegNo
                    , con, trainigCon, refId, recTmplId, iStateGSTCd);
                    //mutext.ReleaseMutex();

                    if (pgDtls != null)
                    {
                        pgDtls.StrReturnURL = returnURL;
                        pgDtls.StrAddInfo1 = additionalInfo1;
                        pgDtls.StrAddInfo2 = additionalInfo2;
                        pgDtls.StrAddInfo3 = additionalInfo3;
                        pgDtls.RecTmplId = recTmplId;
                        pgDtls.StrLoginId = loginId;
                        strKey = "5f4dcc3b5aa765d61d8327deb882cf99";
                        strUserID = sid;
                        strOfficeID = officeid;
                        strScreenId = scid;
                        strType = ptype;
                        bool bInfo = true;
                        strTransId = pgDtls.StrTransId;
                        string strInitiates = PGLogic.GetPGModeHTML(con, strType, sid, strOfficeID, strScreenId, iItemId, pgDtls, bInfo);
                        if (!string.IsNullOrEmpty(strInitiates))
                        {
                            var jsonobj = new
                            {
                                strInitiate = strInitiates,
                                pgDetails = pgDtls
                            };
                            return Json(jsonobj);
                        }
                        else
                        {
                            var jsonobj = new
                            {
                                strInitiate = "<br/><span ><strong> No PaymentGateway Configured for this Screen!Please contact System Administrator </strong></span> ",
                                pgDetails = pgDtls
                            };
                            return Json(jsonobj);
                        }
                    }
                    else
                    {
                        var jsonobj = new
                        {
                            strInitiate = "<br/><span ><strong> No PaymentGateway Configured for this Screen!Please contact System Administrator </strong></span> ",
                            pgDetails = pgDtls
                        };
                        return Json(jsonobj);
                    }
                }
                else
                {
                    strTransId = "Transaction Id is not generated, because of invalid data sent from web app";
                    var jsonobj = new
                    {
                        strInitiate = "<br/><span ><strong> No PaymentGateway Configured for this Screen!Please contact System Administrator </strong></span> ",
                        pgDetails = pgDtls

                    };
                    return Json(jsonobj);
                }
            }
            catch (Exception ex)
            {
                strTransId = "Transaction Id is not generated, because of invalid data sent from web app";
                var jsonobj = new
                {
                    strInitiate = "<br/><span ><strong> No PaymentGateway Configured for this Screen!Please contact System Administrator </strong></span> ",
                    pgDetails = pgDtls
                };
                sbResp.Clear();
                sbResp.Append("INSERT INTO [dbo].[PG_RESPONSE_LOG_T]([PG_TYPE],[RESPONSE_TX])")
                 .Append(" VALUES (@PG_TYPE, @RESPONSE_TX)");
                sqlCmd = Util.UtilService.CreateCommand(sbResp.ToString(), con);
                sqlCmd.Parameters.AddWithValue("@PG_TYPE", 1);
                sqlCmd.Parameters.AddWithValue("@RESPONSE_TX", "InitiateRequest exception: TransactionId - " + strTransId + " Exception - " + ex.Message);
                sqlCmd.ExecuteNonQuery();
                return Json(jsonobj);
            }
            finally
            {
                if(con.State == ConnectionState.Open) con.Close();
                if (trainigCon.State == ConnectionState.Open) trainigCon.Close();
                //if (mutext != null)
                //{
                //    mutext.ReleaseMutex();
                //    mutext.Close();
                //}
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <param name="session"></param>
        /// <param name="scid"></param>
        /// <param name="userid"></param>
        /// <param name="ofid"></param>
        /// <param name="amt"></param>
        /// <param name="taxAmount"></param>
        /// <param name="custName"></param>
        /// <param name="shipping"></param>
        /// <param name="billing"></param>
        /// <param name="email"></param>
        /// <param name="mobile"></param>
        /// <param name="gstnm"></param>
        /// <param name="remarks"></param>
        /// <param name="requestdata"></param>
        /// <param name="stdRegNo"></param>
        /// <param name="con"></param>
        /// <param name="trainigCon"></param>
        /// <param name="refId"></param>
        /// <returns></returns>
        private PGDetails ItemTransactions(Dictionary<string, object> items, string session, int scid, int userid, string ofid
            , double amt, double taxAmount, string custName, string shipping, string billing, string email, string mobile
            , string gstnm, string remarks, string requestdata, string stdRegNo, IDbConnection con, IDbConnection trainigCon
            , int refId, int recTmplId, string iStateGSTCd)
        {
            int gst_nm = 0;
            int.TryParse(gstnm, out gst_nm);
            gstnm = Convert.ToString(gst_nm);
            int tranId = 0, receiptDbId = 0;
            string strLocationIdentifier = string.Empty;
            string TransactionID = string.Empty;
            string strMsg = string.Empty, strItemType = string.Empty;
            long ackId = 0;
            Decimal IGST = 0, CGST = 0, SGST = 0, UTGST = 0, taxable_amt = 0, non_taxable_Amt = 0;
            string IGST_GL_CODE = "NA", SGST_GL_CODE = "NA", CGST_GL_CODE = "NA", UTGST_GL_CODE = "NA";
            PGDetails pGDetails = new PGDetails();
            try
            {
                List<Dictionary<string, object>> data = PaymentGateway.Util.UtilService.GetData("SELECT RECEIPT_START_SEED,OFFICE_NAME_TX,SERVICE_TAX_TX,SHORT_CODE_TX,LOCATION_IDENTIFIER_TX FROM CHAPTER_T WHERE ID =" + ofid, trainigCon);
                string startSeed = Convert.ToString(data[0]["RECEIPT_START_SEED"]);
                string officeName = Convert.ToString(data[0]["OFFICE_NAME_TX"]);
                string serviceName = Convert.ToString(data[0]["SERVICE_TAX_TX"]);
                string shortCode = Convert.ToString(data[0]["SHORT_CODE_TX"]);
                if (!string.IsNullOrEmpty(Convert.ToString(data[0]["LOCATION_IDENTIFIER_TX"])))
                {
                    strLocationIdentifier = Convert.ToString(data[0]["LOCATION_IDENTIFIER_TX"]);
                }
                else
                {
                    strLocationIdentifier = "NA";
                }
                Hashtable htCond = new Hashtable();
                htCond.Add("SCID", scid);
                htCond.Add("SESSIONID", session);
                htCond.Add("AMOUNT", Convert.ToDecimal(amt));
                htCond.Add("CUSTOMER_NAME_TX", custName);
                htCond.Add("OFFICE_ID", ofid);
                htCond.Add("USER_BILLING_ADDR_TX", billing);
                htCond.Add("USER_SHIPPING_ADDR_TX", shipping);
                htCond.Add("EMAIL_TX", email);
                htCond.Add("MOBILE_TX", mobile);
                htCond.Add("REQUEST_DATA_TX", "Request Initiated");
                htCond.Add("REMARK_TX", remarks);
                htCond.Add("USER_ID", userid);
                htCond.Add("REF_ID", refId);
                htCond.Add("PG_STATUS_TX", "initiated");
                htCond.Add("PG_STATUS_ID", 1);
                htCond.Add("TAX_AMOUNT", Convert.ToDecimal(taxAmount));
                htCond.Add("START_SEED", startSeed);
                htCond.Add("TEMPLATE_ID", recTmplId);

                DataTable dtTranData = new DataTable();

                dtTranData = Utils.GetDataFromProc("[dbo].[PMT_INSERT_TRANSACTION]", htCond, con);

                if (dtTranData != null && dtTranData.Rows != null && dtTranData.Rows.Count > 0)
                {
                    TransactionID = Convert.ToString(dtTranData.Rows[0]["NEW_TX_ID"]);
                    tranId = Convert.ToInt32(dtTranData.Rows[0]["PG_ID"]);
                    ackId = Convert.ToInt64(dtTranData.Rows[0]["ACK_ID"]);
                    strMsg = Convert.ToString(dtTranData.Rows[0]["ERROR"]);
                    receiptDbId = Convert.ToInt32(dtTranData.Rows[0]["RECEIPT_ID"]);
                    strItemType = Convert.ToString(dtTranData.Rows[0]["ITEM_TYPE_TX"]);
                }

                if (tranId > 0 && strMsg == "Record Updated Successfully")
                {
                    Dictionary<int, List<SubItems>> itemDict = new Dictionary<int, List<SubItems>>();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("SELECT B.ID AS ITEM_TYPE_ID, B.ITEM_TYPE_CODE, A.ID AS SUB_ITEM_ID,A.ITEM_CODE_DESC AS SUB_ITEM_CODE_DESC,")
                      .Append("A.ITEM_CODE AS SUB_ITEM_CODE, C.ID AS ITEM_ID, C.ITEM_CODE,C.ITEM_CODE_DESC FROM PG_ITEM_T A, PG_ITEM_TYPE_T B,")
                      .Append("PG_ITEM_T C WHERE A.ID IN (");
                    int i = 0;
                    string ids = "";

                    //TODO: TESTING
                    foreach (var item in items)
                    {
                        if (item.Key.Equals("items"))
                        {
                            JArray arr = (JArray)item.Value;
                            foreach (var a in arr)
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(a["item"])) && Convert.ToDouble(a["itemamt"]) != 0)
                                {

                                    SubItems subItem = new SubItems();
                                    subItem.ItemId = Convert.ToInt32(a["item"]);

                                    if (i == 0)
                                    {
                                        sb.Append(subItem.ItemId);
                                        ids = Convert.ToString(subItem.ItemId);
                                    }
                                    else
                                    {
                                        sb.Append(",").Append(subItem.ItemId);
                                        ids = ids + "," + Convert.ToString(subItem.ItemId);
                                    }
                                    subItem.ItemAmt = Convert.ToDouble(a["itemamt"]);
                                    i++;
                                }
                            }
                        }

                    }
                    sb.Append(") AND A.ACTIVE_YN=1 AND B.ACTIVE_YN=1 AND C.ACTIVE_YN=1 AND A.REF_ID=C.ID AND C.ITEM_TYPE_ID = B.ID AND A.ITEM_TYPE_ID = B.ID");
                    List<Dictionary<string, object>> result = PaymentGateway.Util.UtilService.GetData(sb.ToString(), con);
                    sb.Clear();
                    sb.Append("INSERT INTO PG_TRANSACTION_ITEM_T (PG_TRAN_ID,ITEM_ID,ITEM_DESC_TX,AMT_NM,ACTIVE_YN,CREATED_BY,UPDATED_BY) SELECT @PG_TRAN_ID,@ITEM_ID,@ITEM_DESC_TX,@AMT_NM,1,@CREATED_BY,@UPDATED_BY ");
                    sb.Append(" WHERE NOT EXISTS (SELECT * FROM PG_TRANSACTION_ITEM_T WHERE PG_TRAN_ID=@PG_TRAN_ID AND ITEM_ID=@ITEM_ID)");
                    StringBuilder sbb = new StringBuilder();
                    sbb.Append("INSERT INTO PG_TRANSACTION_SUB_ITEM_T(REF_ID,SUB_ITEM_ID,ITEM_DESC_TX,AMT_NM,ACTIVE_YN,CREATED_BY,UPDATED_BY,GL_CODE,TAX_RATE_NM,GST_YN,IGST_AMT,CGST_AMT,SGST_AMT,UTGST_AMT,TOTAL_AMT,IGST_GL_CODE,CGST_GL_CODE,SGST_GL_CODE,UTGST_GL_CODE)")
                        .Append("VALUES(@REF_ID,@SUB_ITEM_ID,@ITEM_DESC_TX,@AMT_NM,1,@CREATED_BY,@UPDATED_BY,@GL_CODE,@TAX_RATE_NM,@GST_YN,@IGST_AMT,@CGST_AMT,@SGST_AMT,@UTGST_AMT,@TOTAL_AMT,@IGST_GL_CODE,@CGST_GL_CODE,@SGST_GL_CODE,@UTGST_GL_CODE)");
                    SqlCommand cmd = PaymentGateway.Util.UtilService.CreateCommand(sb.ToString(), con);
                    SqlCommand cmdb = PaymentGateway.Util.UtilService.CreateCommand(sbb.ToString(), con);


                    foreach (var item in items)
                    {
                        if (item.Key.Equals("items"))
                        {
                            JArray arr = (JArray)item.Value;
                            List<Dictionary<string, object>> lstDict = new List<Dictionary<string, object>>();
                            foreach (var a in arr)
                            {
                                for (i = 0; result != null && i < result.Count; i++)
                                {
                                    if (!string.IsNullOrEmpty(Convert.ToString(a["item"])) && Convert.ToDouble(a["itemamt"]) != 0)
                                    {
                                        if (Convert.ToInt32(result[i]["SUB_ITEM_ID"]) == Convert.ToInt32(a["item"]))
                                        {
                                            int itemId = Convert.ToInt32(result[i]["SUB_ITEM_ID"]);
                                            decimal amount = Convert.ToDecimal(a["itemamt"]);
                                            lstDict = PaymentGateway.Util.UtilService.GetData("select dbo.GetGLCode(" + itemId + "," + amount + ") as GL_CODE", con);
                                            string strGlCode = Convert.ToString(lstDict[0]["GL_CODE"]);
                                            if (string.IsNullOrEmpty(strGlCode)) strGlCode = "NA";

                                            htCond.Clear();

                                            htCond.Add("STATE_GST_CODE", Convert.ToString(iStateGSTCd));
                                            htCond.Add("ITEM_ID", Convert.ToInt32(a["item"]));
                                            htCond.Add("ITEM_AMOUNT", amount);

                                            DataTable dtTaxData = new DataTable();
                                            int taxRate = 0;
                                            if (taxAmount != 0 && taxAmount > 0) dtTaxData = Utils.GetDataFromProc("[dbo].[PMT_TAX_RATES]", htCond, con);

                                            if (dtTaxData != null && dtTaxData.Rows != null && dtTaxData.Rows.Count > 0)
                                            {
                                                IGST = Convert.ToDecimal(dtTaxData.Rows[0]["IGST_AMT"]);
                                                IGST_GL_CODE = Convert.ToString(dtTaxData.Rows[0]["IGST_GL_CODE"]);
                                                CGST = Convert.ToDecimal(dtTaxData.Rows[0]["CGST_AMT"]);
                                                CGST_GL_CODE = Convert.ToString(dtTaxData.Rows[0]["CGST_GL_CODE"]);
                                                SGST = Convert.ToDecimal(dtTaxData.Rows[0]["SGST_AMT"]);
                                                SGST_GL_CODE = Convert.ToString(dtTaxData.Rows[0]["SGST_GL_CODE"]);
                                                UTGST = Convert.ToDecimal(dtTaxData.Rows[0]["UTGST_AMT"]);
                                                UTGST_GL_CODE = Convert.ToString(dtTaxData.Rows[0]["UTGST_GL_CODE"]);
                                                if (dtTaxData.Rows[0]["TAX_RATE"] != DBNull.Value) taxRate = Convert.ToInt32(dtTaxData.Rows[0]["TAX_RATE"]);
                                                gstnm = Convert.ToString(taxRate);
                                            }
                                            else
                                            {
                                                gstnm = "0";
                                            }
                                            if (IGST > 0 || CGST > 0 || SGST > 0 || UTGST > 0) taxable_amt = taxable_amt + amount;
                                            else non_taxable_Amt = non_taxable_Amt + amount;


                                            if (taxAmount > 0)
                                            {
                                                result[i].Add("AMOUNT_NM", Convert.ToDouble(a["itemamt"]) + taxAmount);
                                            }
                                            else
                                            {
                                                result[i].Add("AMOUNT_NM", Convert.ToDouble(a["itemamt"]));
                                            }
                                            cmd.Parameters.AddWithValue("@PG_TRAN_ID", tranId);
                                            cmd.Parameters.AddWithValue("@ITEM_ID", result[i]["ITEM_ID"]);
                                            cmd.Parameters.AddWithValue("@ITEM_DESC_TX", result[i]["ITEM_CODE_DESC"]);
                                            cmd.Parameters.AddWithValue("@AMT_NM", amt);
                                            cmd.Parameters.AddWithValue("@CREATED_BY", userid);
                                            cmd.Parameters.AddWithValue("@UPDATED_BY", userid);
                                            cmd.ExecuteNonQuery();
                                            cmd.Parameters.Clear();
                                            int id = PaymentGateway.Util.UtilService.GetLastInsertedId("PG_TRANSACTION_ITEM_T", con);
                                            result[i].Add("PKEY", id);

                                            cmdb.Parameters.AddWithValue("@REF_ID", id);
                                            cmdb.Parameters.AddWithValue("@SUB_ITEM_ID", result[i]["SUB_ITEM_ID"]);
                                            cmdb.Parameters.AddWithValue("@ITEM_DESC_TX", result[i]["SUB_ITEM_CODE_DESC"]);
                                            cmdb.Parameters.AddWithValue("@AMT_NM", Convert.ToDouble(a["itemamt"]));
                                            cmdb.Parameters.AddWithValue("@CREATED_BY", userid);
                                            cmdb.Parameters.AddWithValue("@UPDATED_BY", userid);
                                            cmdb.Parameters.AddWithValue("@GL_CODE", strGlCode);
                                            cmdb.Parameters.AddWithValue("@TAX_RATE_NM", gstnm);
                                            cmdb.Parameters.AddWithValue("@GST_YN", Convert.ToInt32(gstnm) > 0 ? 1 : 0);
                                            cmdb.Parameters.AddWithValue("@IGST_AMT", IGST);
                                            cmdb.Parameters.AddWithValue("@CGST_AMT", CGST);
                                            cmdb.Parameters.AddWithValue("@SGST_AMT", SGST);
                                            cmdb.Parameters.AddWithValue("@UTGST_AMT", UTGST);
                                            cmdb.Parameters.AddWithValue("@TOTAL_AMT", IGST + CGST + SGST + Convert.ToDecimal(a["itemamt"]));
                                            cmdb.Parameters.AddWithValue("@IGST_GL_CODE", IGST_GL_CODE);
                                            cmdb.Parameters.AddWithValue("@CGST_GL_CODE", CGST_GL_CODE);
                                            cmdb.Parameters.AddWithValue("@SGST_GL_CODE", SGST_GL_CODE);
                                            cmdb.Parameters.AddWithValue("@UTGST_GL_CODE", UTGST_GL_CODE);


                                            cmdb.ExecuteNonQuery();
                                            cmdb.Parameters.Clear();
                                        }
                                    }
                                }
                            }
                        }
                    }
                    string SQL = "UPDATE PG_TRANSACTION_ITEM_T SET PG_TRANSACTION_ITEM_T.AMT_NM = A.TOT_AMOUNT FROM PG_TRANSACTION_ITEM_T INNER JOIN (SELECT REF_ID, SUM(TOTAL_AMT) AS TOT_AMOUNT FROM PG_TRANSACTION_SUB_ITEM_T WHERE PG_TRANSACTION_SUB_ITEM_T.SUB_ITEM_ID IN(" + ids + ") AND PG_TRANSACTION_SUB_ITEM_T.ACTIVE_YN = 1 GROUP BY REF_ID)  A on PG_TRANSACTION_ITEM_T.ID = A.REF_ID AND PG_TRANSACTION_ITEM_T.ACTIVE_YN = 1 AND PG_TRANSACTION_ITEM_T.PG_TRAN_ID=" + tranId;
                    cmd = PaymentGateway.Util.UtilService.CreateCommand(SQL, con);
                    cmd.ExecuteNonQuery();
                    int hdrid = PaymentGateway.Util.UtilService.GetLastInsertedId("PG_PAYMENT_RECEIPT_HDR_T", con);
                    sb.Clear();
                    sb.Append("INSERT INTO PG_PAYMENT_RECEIPT_DTL_T(REF_ID,GL_CODE,ITEM_CODE,ITEM_DESC,ITEM_PRICE,AMOUNT_TYPE,IS_TAXABLE,QTY,TAX_RATE,TOTAL_AMOUNT,TRANSACTION_ID,IS_GST_YN,ACTIVE_YN,CREATED_DT,CREATED_BY,UPDATED_DT,UPDATED_BY,IGST_AMT,CGST_AMT,SGST_AMT,UTGST_AMT,IGST_GL_CODE,CGST_GL_CODE,SGST_GL_CODE,UTGST_GL_CODE,ACKNOWLEDGEMENT_ID) ");
                    sb.Append("SELECT ").Append(hdrid).Append(" as HDR_ID,B.GL_CODE,E.ITEM_CODE,E.ITEM_CODE_DESC,B.AMT_NM,'Main',B.GST_YN,B.QTY_NM,B.TAX_RATE_NM,A.AMT_NM,F.TRANSACTION_ID,C.GST_YN,1,CURRENT_TIMESTAMP,F.CREATED_BY,CURRENT_TIMESTAMP,F.UPDATED_BY,B.IGST_AMT,B.CGST_AMT,B.SGST_AMT,B.UTGST_AMT,B.IGST_GL_CODE,B.CGST_GL_CODE,B.SGST_GL_CODE,B.UTGST_GL_CODE,")
                        .Append(ackId).Append(" as ACKNOWLEDGEMENT_ID FROM PG_TRANSACTION_T F,PG_TRANSACTION_ITEM_T A,PG_TRANSACTION_SUB_ITEM_T B,PG_ITEM_T C,PG_ITEM_T E ");
                    sb.Append("WHERE A.PG_TRAN_ID=F.ID AND A.ID=B.REF_ID AND C.ID=A.ITEM_ID AND C.ID=E.REF_ID AND E.ID=B.SUB_ITEM_ID AND A.ACTIVE_YN=1 AND B.ACTIVE_YN=1 AND C.ACTIVE_YN=1 AND F.ID=").Append(tranId);
                    cmd = PaymentGateway.Util.UtilService.CreateCommand(sb.ToString(), con);
                    cmd.ExecuteNonQuery();

                    hdrid = PaymentGateway.Util.UtilService.GetLastInsertedId("PG_PAYMENT_RECEIPT_HDRF_T", con);
                    sb.Clear();

                    sb.Append("UPDATE PG_PAYMENT_RECEIPT_HDR_T SET TAXABLE_AMT = ").Append(taxable_amt)
                        .Append(", NON_TAXABLE_AMT = ").Append(non_taxable_Amt).Append(" WHERE TRANSACTION_ID = '")
                        .Append(TransactionID).Append("'");
                    cmd = PaymentGateway.Util.UtilService.CreateCommand(sb.ToString(), con);
                    cmd.ExecuteNonQuery();

                    pGDetails.StrCustomerName = custName;
                    pGDetails.StrAmt = amt.ToString();
                    pGDetails.StrEmail = email;

                    //PARSE MOBILE NUMBER 
                    //TODO: TESTING
                    int parseMob = 0;
                    int.TryParse(mobile, out parseMob);

                    if (parseMob != 0)
                    {
                        if (Convert.ToInt32(mobile.Substring(0, 2)) == 91)
                        {
                            mobile = mobile.Substring(mobile.Length - 10);
                        }
                    }

                    pGDetails.StrMob = mobile;
                    pGDetails.StrTransId = TransactionID;
                    pGDetails.StrAckId = Convert.ToString(ackId);
                    pGDetails.StrReqId = Convert.ToString(tranId);
                    pGDetails.StrOfficeName = officeName;
                    pGDetails.StrServiceTax = serviceName;
                    pGDetails.StrShortCode = shortCode;
                    pGDetails.Billing = billing;
                    pGDetails.Shipping = shipping;
                    pGDetails.GstNm = gstnm;
                    pGDetails.ReceiptIncSeed = startSeed;
                    pGDetails.StrUserId = Convert.ToString(userid);
                    pGDetails.Strofid = ofid;
                    pGDetails.StrRegNumber = stdRegNo;
                    pGDetails.StrRemarks = remarks;
                    pGDetails.StrLocationIdentifier = strLocationIdentifier;
                    pGDetails.StrReceiptDbId = Convert.ToString(ackId);
                    pGDetails.IRefId = refId;
                    pGDetails.StrItemType = strItemType;
                }
                else
                {
                    pGDetails = null;
                }
                return pGDetails;
            }
            catch (Exception ex)
            {
                pGDetails = null;
                sbResp.Clear();
                sbResp.Append("INSERT INTO [dbo].[PG_RESPONSE_LOG_T]([PG_TYPE],[RESPONSE_TX])")
                 .Append(" VALUES (@PG_TYPE, @RESPONSE_TX)");
                sqlCmd = Util.UtilService.CreateCommand(sbResp.ToString(), con);
                sqlCmd.Parameters.AddWithValue("@PG_TYPE", 1);
                sqlCmd.Parameters.AddWithValue("@RESPONSE_TX", "Item Transactions exception: Exception - " + ex.Message);
                sqlCmd.ExecuteNonQuery();
                return pGDetails;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [Route("api/Payment/LoadNextScreen")]
        [HttpPost]
        public object LoadNextScreen(object json)
        {
            IDbConnection con = DatabaseFactory.GetDatabase().CreateOpenConnection();
            JObject req = null;
            string strhtml = string.Empty;
            PGDetails pgDtl = new PGDetails();
            try
            {
                string StrBankMerchantId = string.Empty;
                req = JObject.Parse(json.ToString());

                if (req != null && req.HasValues && req.ContainsKey("StrBankMerchantId") && req.ContainsKey("pgDetails") && req.ContainsKey("scid"))
                {
                    StrBankMerchantId = Convert.ToString(req.GetValue("StrBankMerchantId"));
                    pgDtl = req.GetValue("pgDetails").ToObject<PGDetails>();
                    strScreenId = Convert.ToString(req.GetValue("scid"));
                }
                string[] strArr = StrBankMerchantId.Split(';');
                if (!string.IsNullOrEmpty(StrBankMerchantId))
                {
                    pgDtl.StrBankMerchantId = strArr[0];
                    pgDtl.StrPgmode = strArr[1];
                    string sql = "UPDATE PG_TRANSACTION_T SET PG_ID=" + pgDtl.StrBankMerchantId + ", PG_CODE = '" + pgDtl.StrPgmode + "' WHERE ID=" + pgDtl.StrReqId;
                    SqlCommand cmd = UtilService.CreateCommand(sql, con);
                    cmd.ExecuteNonQuery();
                    strhtml = PGLogic.GetPGConfirmHTML(con, pgDtl, pgDtl.StrCustomerName);
                }
                else
                {
                    strhtml = "<br><Strong><span color='red'>Unable to process further</span></strong>";
                }
            }
            catch (Exception ex)
            {
                strhtml = "<br><Strong><span color='red'>Unable to process further</span></strong>";
                sbResp.Clear();
                sbResp.Append("INSERT INTO [dbo].[PG_RESPONSE_LOG_T]([PG_TYPE],[RESPONSE_TX])")
                 .Append(" VALUES (@PG_TYPE, @RESPONSE_TX)");
                sqlCmd = Util.UtilService.CreateCommand(sbResp.ToString(), con);
                sqlCmd.Parameters.AddWithValue("@PG_TYPE", 1);
                sqlCmd.Parameters.AddWithValue("@RESPONSE_TX", "Load next screen exception: Transaction Id - " + pgDtl.StrTransId + " Exception - " + ex.Message);
                sqlCmd.ExecuteNonQuery();
            }

            con.Close();
            var jsonobj = new
            {
                strInitiate = strhtml,
                pgDetails = pgDtl
            };
            return jsonobj;
        }
    }
}
