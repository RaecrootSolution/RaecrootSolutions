using System;
using System.Data;
using System.Data.Sql;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using PwC.ICSI.BusinessLayer;
using System.Text.RegularExpressions;


public partial class StudentMemberPages_RequestAddressChangeMemberNewAddr : System.Web.UI.Page
{
    MemberBL Member = new MemberBL();
    string RegNo = "";
    string PreMembNo = "";
    string CorrCode = "";
    string request = "";
    System.Data.SqlClient.SqlConnection con1, con2, con3, con4, con5, con6, con7;
    System.Data.SqlClient.SqlDataReader dread;
    System.Data.SqlClient.SqlDataAdapter DAP;
    private string strMsg;
    DataSet dsFormLoad;
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if(Session["UserName"]!=null)
            {
                string PreRegNo = Session["UserName"].ToString();
                RegNo = PreRegNo.Remove(0,1); 
                string PreMembNo_Fisrt = PreRegNo.Substring(0,1);
                PreMembNo = PreMembNo_Fisrt;

                 string name = Session["WelcomeFullName"].ToString();
                 int nameLength = name.Length;
                 

                if (nameLength > 15)
                {
                    string NameDisplay = name.Substring(0, 15);
                    labelWelcome.Text = "Welcome" + NameDisplay.ToString();
                }
                else
                {
                    labelWelcome.Text = "Welcome" + Session["WelcomeFullName"].ToString();
                }
            }

            else
            {
                Response.Redirect("/Home/Index");
                //Response.Redirect("Home");
            }
           
               
                if (Request.QueryString["Request"] != null)
                    request = Request.QueryString["Request"].ToString();


            // string connectionString1 = ConfigurationManager.ConnectionStrings["icsinetcon1"].ConnectionString;
            Page.ClientScript.RegisterStartupScript(Type.GetType("System.String"), "addScript", "txtVisible()", true);

            //var appSettings = ConfigurationManager.AppSettings;
            //if (appSettings.Count == 0)
            //{
            //    Console.WriteLine("AppSettings is empty.");
            //}
            //else
            //{
            //    foreach (var key in appSettings.AllKeys)
            //    {
            //        Console.WriteLine("Key: {0} Value: {1}", key, appSettings[key]);
            //    }
            //}
            //var connectionString = ConfigurationManager.ConnectionStrings["icsinetcon11"].ConnectionString;


                con1 = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["icsinetcon11"].ConnectionString);
                con2 = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["icsinetcon11"].ConnectionString);
                con3 = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["icsinetcon11"].ConnectionString);
                con4 = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["icsinetcon11"].ConnectionString);
                con5 = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["icsinetcon11"].ConnectionString);
                con6 = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["icsinetcon11"].ConnectionString);
                con7 = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["icsinetcon11"].ConnectionString);
                
                if (!IsPostBack)
                {
                    
                    MemberBL _membdetails = new MemberBL();
                    dsFormLoad = _membdetails.GetMemberOnlineAddDetails(RegNo, PreMembNo);

                    if (dsFormLoad != null)
                    {
                        ddlState.DataSource = dsFormLoad.Tables[0];
                        ddlState.DataTextField = "state_name";
                        ddlState.DataValueField = "state";
                        ddlState.DataBind();
                        ddlState.Items.Insert(0, "");
                        ddlState.SelectedIndex = 0;


                        ddl_Chapter.DataSource = dsFormLoad.Tables[1];
                        ddl_Chapter.DataTextField = "chapter_name";
                        ddl_Chapter.DataValueField = "chap_code";
                        ddl_Chapter.DataBind();

                        ddl_Desig.DataSource = dsFormLoad.Tables[2];
                        ddl_Desig.DataTextField = "Desig_desc";
                        ddl_Desig.DataValueField = "desig_cd";
                        ddl_Desig.DataBind();
                        
                        ddl_Desig.Items.Insert(0, new ListItem("select Not Available", "0"));
                        
                    }
                    con1.Close();
                    con1.Dispose();
                }
            

            if (!IsPostBack)
            {

                

                if (Session["RegNo"] != null)
                {
                    
                }
            }
        }
        catch (Exception ex)
        { }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnGo_Click(object sender, EventArgs e)
    {
        lblStatus.Text = "Please wait while we fetch your data from Database";
        if (RadioButton1.Checked == true)
        {
            RadioButton2.Checked = false;
            RadioButton1_CheckedChanged(null, null);
            lblemailmessage.Text = "";
            lblph1message.Text = "";
            lblph2message.Text = "";
            lblcellmessage.Text = "";
            lblpinmessage.Text = "";
        }
        else if (RadioButton2.Checked == true)
        {
            RadioButton1.Checked = false;
            RadioButton2_CheckedChanged(null, null);
            lblemailmessage.Text = "";
            lblph1message.Text = "";
            lblph2message.Text = "";
            lblcellmessage.Text = "";
            lblpinmessage.Text = "";
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void RadioButton1_CheckedChanged(object sender, EventArgs e)
    {
        
        try
        {
            lblStatus.Text = "";
            
            enablecontrols();
            string strselect = "select a.region_nm, mc.chapter_name, b.desig_desc, a.p_orgname, a.p_add1, a.p_add2, a.p_add3, a.p_city, c.state_name, a.p_pin, a.p_std, a.p_telno1, a.p_telno2, a.p_faxno, a.p_email, a.website, a.p_cellu "
                + " from master_membmas a Left Join master_desigmas b ON a.desig_cd=b.desig_cd  Left join master_state c ON a.p_state=c.state inner Join  master_chapter mc On a.chapter_cd = mc.chap_code where  PreMembNo='" + PreMembNo.ToString() + "' and membno='" + RegNo.ToString() + "'";
           
            if (con2.State == ConnectionState.Broken || con2.State == ConnectionState.Closed)
                con2.Open();
            System.Data.SqlClient.SqlCommand sqlcmd = new System.Data.SqlClient.SqlCommand(strselect, con2);
            cleartext();
            dread = sqlcmd.ExecuteReader();
            if (dread.Read())
            {
                populatedata();
                
            }
            dread.Close();
            sqlcmd.Dispose();
            con2.Close();
            con2.Dispose();

            
        }
        catch (Exception ex)
        {
            ex = ex;
           
        }
        //}
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void RadioButton2_CheckedChanged(object sender, EventArgs e)
    {
        
        try
        {
            lblStatus.Text = "";
            
            disablecontrols();

            string strselect = "select a.region_nm, mc.chapter_name, b.desig_desc, a.p_orgname, a.r_add1, a.r_add2, a.r_add3, a.r_city, c.state_name, a.r_pin, a.p_std, a.r_telno1, a.r_telno2, a.r_faxno, a.r_email, a.website, a.r_cellu "
                + "from master_membmas a Left Join master_desigmas b ON a.desig_cd=b.desig_cd  Left join master_state c ON a.p_state=c.state inner Join  master_chapter mc On a.chapter_cd = mc.chap_code Where a.PreMembNo='" + PreMembNo.ToString() + "' and a.membno='" + RegNo.ToString() + "'";
          
            if (con3.State == ConnectionState.Broken || con3.State == ConnectionState.Closed)
                con3.Open();

            System.Data.SqlClient.SqlCommand sqlcmd = new System.Data.SqlClient.SqlCommand(strselect, con3);
            cleartext();
            dread = sqlcmd.ExecuteReader();
            if (dread.Read())
            {
                populatedata();
                
            }
            dread.Close();
            sqlcmd.Dispose();
            con3.Close();
            con3.Dispose();
        }
        catch (Exception ex)
        {
            
            ex = ex;
            con3.Close();
            con3.Dispose();
        }
        //}
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnSubmit_Click(object sender, EventArgs e)
    {

        if (chkDeclaration.Checked == true)
        {
            
        }    
        
        if (chkDeclaration.Checked != true)
        {
            Response.Write("<script language=javascript> alert('Kindly Check the Confirmation Box!')</script>");
            return;
        }


        MemberBL _membdetails = new MemberBL();

        DataSet dsFormLoad1 = _membdetails.CheckOnlineMembAddRegion(ddl_Chapter.SelectedValue.Trim(), ddl_Region.SelectedValue.Trim());
        if (dsFormLoad1.Tables[0].Rows.Count == 0)
        {
            lblStatus.Visible = true;
            lblStatus.Text = "Please select correct Region for selected Chapter";
            return;
        }

        if (RadioButton1.Checked == true)
        {
            string strinsert;
            string strupdate;
            
            string strdesig = "";
            string strDescID = "";
            string str = "";

            if ((Convert.ToInt32(Session["CPNo"])) > 0) // May-2012
            {
                if (txtAdd1.Text == "")
                {
                    Response.Write("<script language=javascript> alert('CP holder can not leave the address blank!')</script>");

                    return;
                }
                if (txtAdd2.Text == "")
                {
                    Response.Write("<script language=javascript> alert('Kindly fill address properly!')</script>");

                    return;
                }
                if (txtCity.Text == "")
                {
                    Response.Write("<script language=javascript> alert('Kindly fill name of City!')</script>");

                    return;
                }
            }
            strinsert = "insert into bck_prof_address select premembno,membno,GETDATE(),desig_cd,hierpos,p_orgname,p_add1,p_add2,p_add3,p_city,p_state,p_pin,p_std,p_telno1,p_telno2,p_faxno,p_email,p_pager,p_cellu,'',9999,c_addr_cd,getdate(),''FROM MASTER_MEMBMAS  where premembno='" + PreMembNo.ToString() + "' and membno=" + RegNo.ToString();
            System.Data.SqlClient.SqlCommand cmdinsert = new System.Data.SqlClient.SqlCommand(strinsert, con2);

            if (ddl_Desig.SelectedIndex == 0 && txtBoxNew_desig.Text.Trim() != "")
            {
                strdesig = "INSERT INTO master_desigmas (DESIG_CD, DESIG_DESC, USER_ID, CREATE_DATE)  SELECT MAX(desig_cd)+1, '" + txtBoxNew_desig.Text.Trim().ToUpper() + "','9999', GETDATE()  FROM master_desigmas"; //May-2012
                System.Data.SqlClient.SqlCommand cmddesig = new System.Data.SqlClient.SqlCommand(strdesig, con7);
                str = "select desig_cd as desig_cd from master_desigmas  where DESIG_DESC='" + txtBoxNew_desig.Text.Trim().ToUpper() + "'"; //May-2012
                if (con7.State == ConnectionState.Broken || con7.State == ConnectionState.Closed)
                    con7.Open();
                cmddesig.ExecuteNonQuery();
                System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(str, con7);
                System.Data.SqlClient.SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    strDescID = dr["desig_cd"].ToString().Trim();
                }
                dr.Close();
                cmd.Dispose();
                con7.Close();
                con7.Dispose();
                
                strupdate = "update master_membmas set desig_cd='" + strDescID + "',p_orgname='" + Regex.Replace(txtOrgname.Text.Trim().ToUpper(), "'", "''") + "'," +
                                           "p_add1='" + Regex.Replace(txtAdd1.Text.Trim().ToUpper(), "'", "''") + "',p_add2='" + Regex.Replace(txtAdd2.Text.Trim().ToUpper(), "'", "''") + "',p_add3='" + Regex.Replace(txtAdd3.Text.Trim().ToUpper(), "'", "''") + "',p_city='" + Regex.Replace(txtCity.Text.Trim().ToUpper(), "'", "''") + "',p_state='" + ddlState.SelectedValue.Trim() + "'," +
                                           "p_pin='" + Regex.Replace(txtPin.Text.Trim(), "'", "''") + "',p_std='" + Regex.Replace(txtStd.Text.Trim(), "'", "''") + "',p_telno1='" + Regex.Replace(txtPh1.Text.Trim(), "'", "''") + "',p_telno2='" + Regex.Replace(txtPh2.Text.Trim(), "'", "''") + "',p_faxno='" + Regex.Replace(txtFax.Text.Trim(), "'", "''") + "'," +
                                           "p_email='" + Regex.Replace(txtEmail.Text.Trim().ToUpper(), "'", "''") + "',website='" + txtWebsite.Text.Trim().ToUpper() + "',p_cellu='" + txtCellno.Text.Trim() + "',chapter_cd='" + ddl_Chapter.SelectedValue.Trim() + "',region_nm='" + ddl_Region.SelectedValue.Trim() + "' where premembno='" + PreMembNo.ToString() + "' and membno=" + RegNo.ToString(); //May-2012 .ToUpper()

            }
            else
            {
                
                strupdate = "update master_membmas set desig_cd='" + Convert.ToInt32(Regex.Replace(ddl_Desig.SelectedValue.Trim(), "'", "0")) + "',p_orgname='" + Regex.Replace(txtOrgname.Text.Trim().ToUpper(), "'", "''") + "'," +
                                                            "p_add1='" + Regex.Replace(txtAdd1.Text.Trim().ToUpper(), "'", "''") + "',p_add2='" + Regex.Replace(txtAdd2.Text.Trim().ToUpper(), "'", "''") + "',p_add3='" + Regex.Replace(txtAdd3.Text.Trim().ToUpper(), "'", "''") + "',p_city='" + Regex.Replace(txtCity.Text.Trim().ToUpper(), "'", "''") + "',p_state='" + ddlState.SelectedValue.Trim() + "'," +
                                                            "p_pin='" + Regex.Replace(txtPin.Text.Trim(), "'", "''") + "',p_std='" + Regex.Replace(txtStd.Text.Trim(), "'", "''") + "',p_telno1='" + Regex.Replace(txtPh1.Text.Trim(), "'", "''") + "',p_telno2='" + Regex.Replace(txtPh2.Text.Trim(), "'", "''") + "',p_faxno='" + Regex.Replace(txtFax.Text.Trim(), "'", "''") + "'," +
                                                            "p_email='" + Regex.Replace(txtEmail.Text.Trim().ToUpper(), "'", "''") + "',website='" + txtWebsite.Text.Trim().ToUpper() + "',p_cellu='" + txtCellno.Text.Trim() + "',chapter_cd='" + ddl_Chapter.SelectedValue.Trim() + "',region_nm='" + ddl_Region.SelectedValue.Trim() + "' where premembno='" + PreMembNo.ToString() + "' and membno=" + RegNo.ToString();    //May-2012 .ToUpper()

            }

            System.Data.SqlClient.SqlCommand cmdupdate = new System.Data.SqlClient.SqlCommand(strupdate, con2);
            try
            {
                if (con2.State == ConnectionState.Broken || con2.State == ConnectionState.Closed)
                    con2.Open();

                cmdinsert.ExecuteNonQuery();
                cmdupdate.ExecuteNonQuery();

                con2.Close();
                con2.Dispose();
            }
            catch (Exception ex)
            {
                lblStatus.Text = ex.Message;
                return;
            }
            Panel1.Visible = false;
            
            lblStatus.Text = "Your address has been changed succesfully.";
        }
        else if (RadioButton2.Checked == true)
        {
            string strinsert;
            string strupdate;
           
            strinsert = "insert into bck_resi_address select premembno,membno,GETDATE(),r_add1,r_add2,r_add3,r_city,r_state,r_pin,r_telno1,r_telno2,r_faxno,r_pager,r_cellu,'',user_id,c_addr_cd,R_EMAIL,'' from MASTER_MEMBMAS where premembno='" + PreMembNo.ToString() + "' and membno=" + RegNo.ToString();
            System.Data.SqlClient.SqlCommand cmdinsert = new System.Data.SqlClient.SqlCommand(strinsert, con2);
            strupdate = "update master_membmas set " +
                                            "r_add1='" + Regex.Replace(txtAdd1.Text.Trim().ToUpper(), "'", "''") + "',r_add2='" + Regex.Replace(txtAdd2.Text.Trim().ToUpper(), "'", "''") + "',r_add3='" + Regex.Replace(txtAdd3.Text.Trim().ToUpper(), "'", "''") + "',r_city='" + Regex.Replace(txtCity.Text.Trim().ToUpper(), "'", "''") + "',r_state='" + Regex.Replace(ddlState.SelectedValue.Trim(), "'", "''") + "'," +
                                            "r_pin='" + Regex.Replace(txtPin.Text.Trim(), "'", "''") + "',r_telno1='" + Regex.Replace(txtPh1.Text.Trim(), "'", "''") + "',r_telno2='" + Regex.Replace(txtPh2.Text.Trim(), "'", "''") + "',r_faxno='" + Regex.Replace(txtFax.Text.Trim(), "'", "''") + "',R_EMAIL='" + Regex.Replace(txtEmail.Text.Trim().ToUpper(), "'", "''") + "'," +
                                            "r_cellu='" + Regex.Replace(txtCellno.Text.Trim(), "'", "''") + "',chapter_cd='" + ddl_Chapter.SelectedValue.Trim() + "',region_nm='" + ddl_Region.SelectedValue.Trim() + "' where premembno='" + PreMembNo.ToString() + "' and membno=" + RegNo.ToString();	//May-2012 .ToUpper()
            System.Data.SqlClient.SqlCommand cmdupdate = new System.Data.SqlClient.SqlCommand(strupdate, con2);

            try
            {
                if (con2.State == ConnectionState.Broken || con2.State == ConnectionState.Closed)
                    con2.Open();

                cmdinsert.ExecuteNonQuery();
                cmdupdate.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                lblStatus.Text = ex.Message;
                return;
            }
            finally
            {
                con2.Close();
                con2.Dispose();
            }
            Panel1.Visible = false;
            
            lblStatus.Text = "Your address has been  changed succesfully.";
        }


        ///////

        //string RequestType = ConfigurationManager.AppSettings["Address"].ToString();


        
        DataSet ds = Member.QueryRequest(RegNo, PreMembNo, request, Session["UserName"].ToString());
        string Status = ds.Tables[0].Rows[0]["Status"].ToString();

        if (Status == "Success")
        {
            string querystring = ds.Tables[0].Rows[0]["ID"].ToString() + "|NA|" + request;
            
            Session["Receipt"] = querystring;
            //string acknowledgment = "Receipt.aspx";
            //Response.Redirect(acknowledgment);
            Response.Redirect("~/Home/acknowledgment");
        }

        ///////



    }
    /// <summary>
    /// 
    /// </summary>
    private void enablecontrols()
    {
        lblDesig.Visible = true;
        ddl_Desig.Visible = true;
        lblOrgname.Visible = true;
        txtOrgname.Visible = true;
        lblStd.Visible = true;
        txtStd.Visible = true;
        lblEmail.Visible = true;
        txtEmail.Visible = true;
        lblWebsite.Visible = true;
        txtWebsite.Visible = true;
        Label2.Visible = true;
        Label3.Visible = true;
        Label4.Visible = true;
        Label5.Visible = true;
        Label6.Visible = true;
        panel_Designation.Visible = true;
        panel_Address.Visible = true;

    }
    /// <summary>
    /// 
    /// </summary>
    private void disablecontrols()
    {
       // Label2.Visible = false;
        lblDesig.Visible = false;
        lblOrgname.Visible = false;
        ddl_Desig.Visible = false;
        Label3.Visible = false;
        txtOrgname.Visible = false;
        Label4.Visible = false;
        txtStd.Visible = false;
        lblStd.Visible = false;
        Label5.Visible = false;
        txtWebsite.Visible = false;
        lblWebsite.Visible = false;
        txtBoxNew_desig.Visible = false;
        panel_Designation.Visible = false;
        panel_Address.Visible = false;

    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tState"></param>
    /// <returns></returns>
    private string PopState(string tState)
    {
        string str;
        str = "select state from master_state where state_name = '" + tState.Trim() + "'";

        if (con4.State == ConnectionState.Broken || con4.State == ConnectionState.Closed)
            con4.Open();

        System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(str, con4);
        System.Data.SqlClient.SqlDataReader dr = cmd.ExecuteReader();
        if (dr.Read())
        {
            str = dr["State"].ToString().Trim();
        }
        dr.Close();
        cmd.Dispose();
        con4.Close();
        con4.Dispose();
        return str;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tChapter"></param>
    /// <returns></returns>
    private string PopChapter(string tChapter)
    {
        string str;
        str = "select chap_code from master_chapter where chapter_name = '" + tChapter.Trim() + "'";
        if (con5.State == ConnectionState.Broken || con5.State == ConnectionState.Closed)
            con5.Open();
        System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(str, con5);
        System.Data.SqlClient.SqlDataReader dr = cmd.ExecuteReader();
        if (dr.Read())
        {
            str = dr["chap_code"].ToString().Trim();
        }
        dr.Close();
        cmd.Dispose();
        con5.Close();
        con5.Dispose();
        return str;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tDesig"></param>
    /// <returns></returns>
    private string PopDesig(string tDesig)
    {
        string str;
        str = "select desig_cd from master_desigmas where desig_desc = '" + tDesig.Trim() + "'";
        if (con6.State == ConnectionState.Broken || con6.State == ConnectionState.Closed)
            con6.Open();
        System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(str, con6);
        System.Data.SqlClient.SqlDataReader dr = cmd.ExecuteReader();
        if (dr.Read())
        {
            str = dr["desig_cd"].ToString().Trim();
        }
        dr.Close();
        cmd.Dispose();
        con6.Close();
        con6.Dispose();
        return str;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnReset_Click(object sender, System.EventArgs e)
    {
        cleartext();
    }
    /// <summary>
    /// 
    /// </summary>
    private void populatedata()
    {
        Panel1.Visible = true;

       
        lblRegion.Text = dread.GetValue(0).ToString().Trim().ToUpper();
        ddl_Region.SelectedValue = dread.GetValue(0).ToString().Trim().ToUpper();
        lblChapter.Text = dread.GetValue(1).ToString().Trim().ToUpper();

        string str = dread.GetValue(0).ToString().Trim().ToUpper().ToString();
        if (str != null && str != "")
        {
            GetRegionWiseChp(str);
        }
        if (!dread.IsDBNull(1))
        {
            try
            {
                ddl_Chapter.SelectedValue = PopChapter(dread.GetValue(1).ToString().Trim().ToUpper());
            }
            catch (Exception ex)
            {
                Response.Write(ex.Message);
                ex = ex;
            }
        }


        lblDesig.Text = dread.GetValue(2).ToString().Trim().ToUpper();

        if (!dread.IsDBNull(2))
        {
            try
            {
                if ((Convert.ToInt32(Session["CPNo"])) > 0) // May-2012
                {
                    ddl_Desig.SelectedValue = PopDesig(dread.GetValue(2).ToString().Trim().ToUpper());
                    ddl_Desig.Enabled = false;
                }
                else
                {
                    ddl_Desig.Enabled = true;
                    ddl_Desig.SelectedValue = PopDesig(dread.GetValue(2).ToString().Trim().ToUpper());
                }
            }
            catch (Exception ex)
            {
                Response.Write(ex.Message);
                // ex = ex;
            }
        }

        if ((Convert.ToInt32(Session["CPNo"])) > 0) // May-2012
        {
            lblOrgname.Text = txtOrgname.Text = dread.GetValue(3).ToString().Trim().ToUpper();
            txtOrgname.ReadOnly = true;
        }
        else
        {
            txtOrgname.ReadOnly = false;
            lblOrgname.Text = txtOrgname.Text = dread.GetValue(3).ToString().Trim().ToUpper();
        }
        lblAdd1.Text = txtAdd1.Text = dread.GetValue(4).ToString().Trim().ToUpper();
        lblAdd2.Text = txtAdd2.Text = dread.GetValue(5).ToString().Trim().ToUpper();
        lblAdd3.Text = txtAdd3.Text = dread.GetValue(6).ToString().Trim().ToUpper();
        lblCity.Text = txtCity.Text = dread.GetValue(7).ToString().Trim().ToUpper();
        lblState.Text = dread.GetValue(8).ToString().Trim().ToUpper();
        if (!dread.IsDBNull(8))
        {
            try
            {
                ddlState.SelectedValue = PopState(dread.GetValue(8).ToString().Trim().ToUpper());
            }
            catch (Exception ex)
            {
                Response.Write(ex.Message);
            }
        }

        lblPin.Text = txtPin.Text = dread.GetValue(9).ToString().Trim().ToUpper();
        lblStd.Text = txtStd.Text = dread.GetValue(10).ToString().Trim().ToUpper();
        lblPh1.Text = txtPh1.Text = dread.GetValue(11).ToString().Trim().ToUpper();
        lblPh2.Text = txtPh2.Text = dread.GetValue(12).ToString().Trim().ToUpper();
        lblFax.Text = txtFax.Text = dread.GetValue(13).ToString().Trim().ToUpper();
        lblEmail.Text = txtEmail.Text = dread.GetValue(14).ToString().Trim().ToUpper();
        lblWebsite.Text = txtWebsite.Text = dread.GetValue(15).ToString().Trim().ToUpper();
        lblCellno.Text = txtCellno.Text = dread.GetValue(16).ToString().Trim().ToUpper();
        
    }
    /// <summary>
    /// 
    /// </summary>
    private void cleartext()
    {
        ddl_Region.SelectedIndex = 0;
        ddl_Chapter.SelectedIndex = 0;
        ddl_Desig.SelectedIndex = 0;
        txtOrgname.Text = "";
        txtAdd1.Text = "";
        txtAdd2.Text = "";
        txtAdd3.Text = "";
        txtCity.Text = "";
        ddlState.SelectedIndex = 0;
        txtPin.Text = "";
        txtStd.Text = "";
        txtPh1.Text = "";
        txtPh2.Text = "";
        txtFax.Text = "";
        txtCellno.Text = "";
        txtEmail.Text = "";
        txtWebsite.Text = "";
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void txtCity_TextChanged(object sender, EventArgs e)
    {

        if (RadioButton1.Checked)
        {
            Response.Write("<script>alert('Kindly change your chapter code and region code accordingly!')</script>");
            SetFocus(ddl_Region);

        }
        else if (RadioButton2.Checked)
        {
            string str;
           
            str = "select r_city from master_membmas where (p_city='' or p_city is null) and (r_city !='' or r_city is not null) and premembno='" + PreMembNo.ToString() + "' and membno=" + RegNo.ToString() + " and  r_city= '" + txtCity.Text.Trim().ToUpper() + "'";
            if (con6.State == ConnectionState.Broken || con6.State == ConnectionState.Closed)
                con6.Open();
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(str, con6);
            System.Data.SqlClient.SqlDataReader dr = cmd.ExecuteReader();
            if (dr.HasRows)
            {
                
            }
            else
            {
                Response.Write("<script>alert('Kindly change your chapter code and region code accordingly!')</script>");
                SetFocus(ddl_Region);
                
            }
            dr.Close();
            cmd.Dispose();
            con6.Close();
            con6.Dispose();
        }
    }

    protected void ddl_Region_SelectedIndexChanged(object sender, EventArgs e)
    {
        string region = ddl_Region.SelectedItem.ToString();
        GetRegionWiseChp(region);

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="strRegion"></param>
    private void GetRegionWiseChp(string strRegion)
    {
        if (con1.State == ConnectionState.Broken || con1.State == ConnectionState.Closed)
        {
            con1.Open();
            string strMsg = "select a.chap_code, a.chapter_name,a.chp_per_name from master_chapter a where a.chp_per_name='" + strRegion.ToString() + "'";
            DAP = new System.Data.SqlClient.SqlDataAdapter(strMsg, con1);
            DataTable dt = new DataTable();
            DAP.Fill(dt);
            con1.Close();
            con1.Dispose();
            if (dt.Rows.Count > 0)
            {
                ddl_Chapter.DataSource = dt;
                ddl_Chapter.DataTextField = "chapter_name";
                ddl_Chapter.DataValueField = "chap_code";
                ddl_Chapter.DataBind();
            }
            else
            {
                MemberBL _details = new MemberBL();
                dsFormLoad = _details.GetMemberOnlineAddDetails(RegNo, PreMembNo);
                ddl_Chapter.DataSource = dsFormLoad.Tables[1];
                ddl_Chapter.DataTextField = "chapter_name";
                ddl_Chapter.DataValueField = "chap_code";
                ddl_Chapter.DataBind();
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    //protected void ddl_Region_SelectedIndexChanged(object sender, EventArgs e)
    //{
    //    string region = ddl_Region.SelectedItem.ToString();
    //    GetRegionWiseChp(region);
    //}
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void txtCellno_TextChanged(object sender, EventArgs e)
    {
        if (txtCellno.Text == "")
        {            
            lblcellmessage.Text = "Enter Mobile No!";
            lblcellmessage.ForeColor = System.Drawing.Color.Red;
            return;
        }
        if (txtCellno.Text != "")
        {
            string txtmob = txtCellno.Text;
            Regex regex = new Regex(@"^([7-9]{1})([0-9]{9})$");
            Match match = regex.Match(txtmob);
            if (match.Success)
            { lblcellmessage.Text = ""; }
            else
            {               
                lblcellmessage.Text = "Enter valid Mobile No!";
                lblcellmessage.ForeColor = System.Drawing.Color.Red;
                return;
            }
        }  
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void txtEmail_TextChanged(object sender, EventArgs e)
    {
        if (txtEmail.Text == "")
        {           
            lblemailmessage.Text = "Enter Email ID!";
            lblemailmessage.Focus();
            lblemailmessage.ForeColor = System.Drawing.Color.Red;
            return;
        }
        if (txtEmail.Text != "")
        {
            string email = txtEmail.Text;
            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Match match = regex.Match(email);
            if (match.Success)
            { lblemailmessage.Text = ""; }
            else
            {
                lblemailmessage.Text = "Enter  Valid Email ID!";
                lblemailmessage.Focus();
                lblemailmessage.ForeColor = System.Drawing.Color.Red;
                return;
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void txtPh2_TextChanged(object sender, EventArgs e)
    {
        if (txtPh2.Text != "")
        {
            string phone1 = txtPh2.Text;
            Regex regex = new Regex(@"^[01]?[- .]?(\([2-9]\d{2}\)|[2-9]\d{2})[- .]?\d{3}[- .]?\d{4}$");
            Match match = regex.Match(phone1);
            if (match.Success)
            { lblph2message.Text = ""; }
            else
            {
               // Response.Write("<script language=javascript> alert('Enter valid Phone No!')</script>");
                lblph2message.Text = "Enter valid Phone No!";
                lblph2message.Focus();
                lblph2message.ForeColor = System.Drawing.Color.Red;
                return;
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void txtPh1_TextChanged(object sender, EventArgs e)
    {
        txtPh1.Focus();
        if (txtPh1.Text == "")
        {
            //Response.Write("<script language=javascript> alert('Enter Phone No!')</script>");
            lblph1message.Text = "Enter Phone No!";
            lblph1message.Focus();
            lblph1message.ForeColor = System.Drawing.Color.Red;
            return;
        }
        if (txtPh1.Text != "")
        {
            string phone = txtPh1.Text;
            Regex regex = new Regex(@"^[01]?[- .]?(\([2-9]\d{2}\)|[2-9]\d{2})[- .]?\d{3}[- .]?\d{4}$");
            Match match = regex.Match(phone);
            if (match.Success)
            { lblph1message.Text = ""; }
            else
            {
               // Response.Write("<script language=javascript> alert('Enter valid Phone No!')</script>");
                lblph1message.Text = "Enter valid Phone No!";
                lblph1message.Focus();
                lblph1message.ForeColor = System.Drawing.Color.Red;
                return;
            }
        }     
             
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void txtPin_TextChanged(object sender, EventArgs e)
    {
        if (txtPin.Text == "")
        {
           // Response.Write("<script language=javascript> alert('Enter Pin Code!')</script>");
            lblpinmessage.Text = "Enter Pin Code!";            
            lblpinmessage.Focus();
            lblpinmessage.ForeColor = System.Drawing.Color.Red;
            return;
        }
        if (txtPin.Text != "")
        {
            string pincode = txtPin.Text;
            Regex regex = new Regex(@"^\d{6}$");
            Match match = regex.Match(pincode);
            if (match.Success)
            { lblpinmessage.Text = ""; }
            else
            {
                lblpinmessage.Text = "Enter valid Pincode!";
                lblpinmessage.Focus();
                lblpinmessage.ForeColor = System.Drawing.Color.Red;
               // Response.Write("<script language=javascript> alert('Enter valid Pincode!')</script>");
                return;
            }
        }
    }
}
