using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Net.Mail;
using System.Data;
using System.Data.SqlClient;
using ICSI_EmailSmsService.Utility;

namespace ICSI_EmailSmsService.Utility
{
    public class PCSService
    {
        DataAccessService objDataAccessService = new DataAccessService();
        //bool EnableDebug = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableDebug"]);

        public void PCSUpdateData()
        {
            try
            {
                DataTable dtPCSdata = new DataTable();
                StringBuilder sbquery = new StringBuilder();
                StringBuilder sbUpdateQuery = new StringBuilder();
                Dictionary<string, SqlParameter> cmdParameters;

                sbquery.Append("SELECT PREMEMBNO,MEMBNO,TITLE,FIRST_NAME,LAST_NAME,DOB,P_ADD1,P_ADD2,P_ADD3,P_CITY,P_PIN,P_TELNO1,P_TELNO2,P_EMAIL,");
                sbquery.Append("P_CELLU,R_EMAIL,R_TELNO1,R_TELNO2,R_CELLU,ACS_NO,MODIFY_DATE,MEMB_DT,CP_NO,CP_DT");
                sbquery.Append(" FROM Master_membmas WHERE CONVERT(varchar(20), MODIFY_DATE, 101) = CONVERT(varchar(20), GETDATE()-1, 101)");
                //sbquery.Append(" FROM Master_membmas WHERE PREMEMBNO='F' AND MEMBNO=10624");

                dtPCSdata = objDataAccessService.ExecuteInlineQuery(Convert.ToString(CommonService.enmConnectionName.PCS), sbquery.ToString());

                if (dtPCSdata != null && dtPCSdata.Rows != null && dtPCSdata.Rows.Count > 0)
                {
                    for (int i = 0; i < dtPCSdata.Rows.Count; i++)
                    {
                        cmdParameters = new Dictionary<string, SqlParameter>();
                        cmdParameters["PREMEMBNO"] = new SqlParameter("PREMEMBNO", dtPCSdata.Rows[i]["PREMEMBNO"]);
                        cmdParameters["MEMBNO"] = new SqlParameter("MEMBNO", dtPCSdata.Rows[i]["MEMBNO"]);
                        cmdParameters["FIRST_NAME"] = new SqlParameter("FIRST_NAME", dtPCSdata.Rows[i]["FIRST_NAME"]);
                        cmdParameters["LAST_NAME"] = new SqlParameter("LAST_NAME", dtPCSdata.Rows[i]["LAST_NAME"]);
                        cmdParameters["DOB"] = new SqlParameter("DOB", dtPCSdata.Rows[i]["DOB"]);
                        cmdParameters["P_ADD1"] = new SqlParameter("P_ADD1", dtPCSdata.Rows[i]["P_ADD1"]);
                        cmdParameters["P_ADD2"] = new SqlParameter("P_ADD2", dtPCSdata.Rows[i]["P_ADD2"]);
                        cmdParameters["P_ADD3"] = new SqlParameter("P_ADD3", dtPCSdata.Rows[i]["P_ADD3"]);
                        cmdParameters["P_CITY"] = new SqlParameter("P_CITY", dtPCSdata.Rows[i]["P_CITY"]);
                        cmdParameters["P_PIN"] = new SqlParameter("P_PIN", dtPCSdata.Rows[i]["P_PIN"]);
                        cmdParameters["P_TELNO1"] = new SqlParameter("P_TELNO1", dtPCSdata.Rows[i]["P_TELNO1"]);
                        cmdParameters["P_TELNO2"] = new SqlParameter("P_TELNO2", dtPCSdata.Rows[i]["P_TELNO2"]);
                        cmdParameters["P_EMAIL"] = new SqlParameter("P_EMAIL", dtPCSdata.Rows[i]["P_EMAIL"]);
                        cmdParameters["P_CELLU"] = new SqlParameter("P_CELLU", dtPCSdata.Rows[i]["P_CELLU"]);
                        cmdParameters["R_EMAIL"] = new SqlParameter("R_EMAIL", dtPCSdata.Rows[i]["R_EMAIL"]);
                        cmdParameters["R_TELNO1"] = new SqlParameter("R_TELNO1", dtPCSdata.Rows[i]["R_TELNO1"]);
                        cmdParameters["R_TELNO2"] = new SqlParameter("R_TELNO2", dtPCSdata.Rows[i]["R_TELNO2"]);
                        cmdParameters["R_CELLU"] = new SqlParameter("R_CELLU", dtPCSdata.Rows[i]["R_CELLU"]);
                        cmdParameters["ACS_NO"] = new SqlParameter("ACS_NO", dtPCSdata.Rows[i]["ACS_NO"]);
                        cmdParameters["MEMB_DT"] = new SqlParameter("MEMB_DT", dtPCSdata.Rows[i]["MEMB_DT"]);
                        cmdParameters["CP_NO"] = new SqlParameter("CP_NO", dtPCSdata.Rows[i]["CP_NO"]);
                        cmdParameters["CP_DT"] = new SqlParameter("CP_DT", dtPCSdata.Rows[i]["CP_DT"]);

                        int updateStatus = objDataAccessService.ExecuteCommand(Convert.ToString(CommonService.enmConnectionName.training),
                            Convert.ToString(CommonService.enmStoreProcedures.SP_UPDATE_PCS_DETAILS), cmdParameters);
                    }
                }
            }
            catch (Exception ex)
            {
                CommonService.WriteToErrorLogs(ex.ToString(), Convert.ToString(CommonService.enmErrorLogs.PCS));
            }
        }
    }
}
