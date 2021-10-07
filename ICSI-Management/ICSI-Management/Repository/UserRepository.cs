using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Text;
using ICSI_Management.DBContext;
using ICSI_Management.Models;

namespace ICSI_Management.Repository
{
    public class UserRepository : IUserRepository
    {
        private ICSI_DBModelEntities DBcontext;
        public UserRepository(ICSI_DBModelEntities objusercontext)
        {
            this.DBcontext = objusercontext;
        }
        public bool CheckLogin(SUPER_USER_T objuser) //This method check the user existence
        {
            bool flag = false;
            SUPER_USER_T loginresult = DBcontext.SUPER_USER_T.Where(s => s.LOGIN_ID == objuser.USER_ID && s.LOGIN_PWD_TX == objuser.LOGIN_PWD_TX).FirstOrDefault();
            if (loginresult == null)
            {
                return flag;
            }
            else
            {
                flag = true;
            }
            return flag;

        }
        public SUPER_USER_T GetUserByUserName(string LoginId)
        {
            return DBcontext.SUPER_USER_T.Where(x => x.LOGIN_ID == LoginId).FirstOrDefault();
        }

        #region Responsibility
        public int AddResponsibility(Responsibility responsibility)
        {
            int status = 0;
            try
            {

                RESPONSIBILITY_T resResp = DBcontext.RESPONSIBILITY_T.Where(x => x.ID == responsibility.ID).FirstOrDefault();

                RESPONSIBILITY_T obj;
                if (resResp == null)
                {
                    obj = new RESPONSIBILITY_T();
                    obj.CREATED_DT = DateTime.Now;
                    obj.CREATED_BY = Convert.ToInt32(HttpContext.Current.Session["USER_T_ID"]);
                }
                else
                    obj = resResp;

                obj.RESP_NAME_TX = responsibility.Resp_Name;
                obj.REF_ID = responsibility.RefId;
                obj.RESP_DESC_TX = responsibility.Resp_Desc;
                obj.RESP_TYPE_NM = responsibility.Resp_Type;
                obj.ACTIVE_YN = responsibility.Active;
                obj.READ_WRITE_YN = responsibility.Read_Write;
                obj.RESP_SYM_NAME_TX = responsibility.Resp_Sym_Name;
                obj.UPDATED_BY = Convert.ToInt32(HttpContext.Current.Session["USER_T_ID"]);
                obj.UPDATED_DT = DateTime.Now;

                if (resResp == null)
                    DBcontext.RESPONSIBILITY_T.Add(obj);
                status = DBcontext.SaveChanges();
            }
            catch (Exception ex) { }

            return status;
        }
        public Responsibility GetResponsibilityById(int respId)
        {
            return DBcontext.RESPONSIBILITY_T.Where(x => x.ID == respId).Select(x => new Responsibility
            {
                Resp_Name = x.RESP_NAME_TX,
                Resp_Desc = x.RESP_DESC_TX,
                Read_Write = x.READ_WRITE_YN,
                Active = x.ACTIVE_YN,
                Resp_Type = x.RESP_TYPE_NM,
                Resp_Sym_Name = x.RESP_SYM_NAME_TX,
                ID = x.ID
            }).FirstOrDefault();
        }
        public List<Responsibility> GetResponsibilities()
        {
            Dictionary<int, string> ResponseListList = new Dictionary<int, string>();
            ResponseListList.Add(1,"Web Application");
            ResponseListList.Add(2,"Application");
            ResponseListList.Add(3, "Module");
            ResponseListList.Add(4,"Screen");
            ResponseListList.Add(5,"Functionality");
            ResponseListList.Add(6,"Report");
            ResponseListList.Add(7,"Approve");
            ResponseListList.Add(8, "Reject");


            return (from x in DBcontext.RESPONSIBILITY_T.AsEnumerable()
                    select new Responsibility
                    {
                        Resp_Name = x.RESP_NAME_TX,
                        Resp_Desc = x.RESP_DESC_TX,
                        Read_Write = x.READ_WRITE_YN,
                        Active = x.ACTIVE_YN,
                        RespTypeName = ResponseListList[x.RESP_TYPE_NM],
                        Resp_Sym_Name = x.RESP_SYM_NAME_TX,
                        ID = x.ID,
                        ModifyDate = x.UPDATED_DT
                    }).ToList();
            //return DBcontext.RESPONSIBILITY_T.Select(x => new Responsibility
            //{
            //    Resp_Name = x.RESP_NAME_TX,
            //    Resp_Desc = x.RESP_DESC_TX,
            //    Read_Write = x.READ_WRITE_YN,
            //    Active = x.ACTIVE_YN,
            //    RespTypeName = ResponseListList[x.RESP_TYPE_NM],
            //    Resp_Sym_Name = x.RESP_SYM_NAME_TX,
            //    ID = x.ID,
            //    ModifyDate = x.UPDATED_DT
            //}).OrderBy(x => x.ModifyDate).ToList();
        }
        #endregion

        #region Role
        public bool InsertRole(Roles roles)
        {
            try
            {
                ROLE_T role = new ROLE_T();
                role.ROLE_NAME_TX = roles.RoleName;
                role.ROLE_SYM_NAME_TX = roles.RoleSysName;
                role.ROLE_DESC_TX = roles.RoleDescription;
                role.ORDER_NM = roles.RoleOrder;
                role.ACTIVE_YN = roles.Active;
                role.CREATED_BY = 1;
                role.CREATED_DT = DateTime.Now;
                role.UPDATED_BY = 1;
                role.UPDATED_DT = DateTime.Now;
                DBcontext.ROLE_T.Add(role);
                DBcontext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        public List<ROLE_T> GetAllRoles()
        {

            return DBcontext.ROLE_T.ToList();
        }
        public ROLE_T GetRoleById(int RoleId)
        {
            return DBcontext.ROLE_T.Where(x => x.ID == RoleId).FirstOrDefault();
        }
        public bool UpdateRole(Roles roles)
        {
            try
            {
                ROLE_T rolet = DBcontext.ROLE_T.Where(x => x.ID == roles.Id).FirstOrDefault();
                rolet.ROLE_NAME_TX = roles.RoleName;
                rolet.ROLE_SYM_NAME_TX = roles.RoleSysName;
                rolet.ROLE_DESC_TX = roles.RoleDescription;
                rolet.ACTIVE_YN = roles.Active;
                rolet.ORDER_NM = roles.RoleOrder;
                rolet.UPDATED_BY = 1;
                rolet.UPDATED_DT = DateTime.Now;
                rolet.CREATED_BY = 1;
                rolet.CREATED_DT = DateTime.Now;
                DBcontext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        #endregion

        #region Screen
        public int AddScreen(Screen screen)
        {
            int status = 0;
            try
            {
                SCREEN_T resScreen = DBcontext.SCREEN_T.Where(x => x.ID == screen.ID).FirstOrDefault();

                SCREEN_T obj;
                if (resScreen == null)
                {
                    obj = new SCREEN_T();
                    obj.CREATED_DT = DateTime.Now;
                    obj.CREATED_BY = Convert.ToInt32(HttpContext.Current.Session["USER_T_ID"]);
                }
                else
                    obj = resScreen;

                obj.SCREEN_NAME_TX = screen.Screen_Name;
                obj.SCHEMA_NAME_TX = screen.Schema_Name;
                obj.APP_MODULE_ID = screen.AppModuleId;
                obj.SCREEN_TITLE_TX = screen.Screen_Title;
                obj.SCREEN_FILE_NAME_TX = screen.Screen_File_Name;
                obj.SCREEN_STYLE_TX = screen.Screen_Style;
                obj.SCREEN_SCRIPT_TX = screen.Screen_Script;
                obj.SCREEN_CONTENT_TX = screen.Screen_Content;
                obj.SCREEN_CLASS_NAME_TX = screen.Screen_Class_Name;
                obj.SCREEN_REF_CLASS_NAME_TX = screen.Screen_Ref_Class_Name;
                obj.SCREEN_CLASS_STATIC_YN = screen.Screen_Class_Static;
                obj.SCREEN_METHOD_NAME_TX = screen.Screen_Method_Name;
                obj.SCREEN_METHOD_STATIC_YN = screen.Screen_Method_Static;
                obj.TABLE_NAME_TX = screen.Table_Name;
                obj.APP_MODULE_ID = screen.AppModuleId;
                obj.MANDATORY_SCR_ID = screen.MandScreenId;
                obj.ACTION_TX = screen.Action_TX;
                obj.ACTIVE_YN = screen.Active;
                obj.SCREEN_SYM_NAME_TX = screen.Screen_Sym_Name;
                obj.UPDATED_BY = Convert.ToInt32(HttpContext.Current.Session["USER_T_ID"]);
                obj.UPDATED_DT = DateTime.Now;

                if (resScreen == null)
                    DBcontext.SCREEN_T.Add(obj);
                status = DBcontext.SaveChanges();
            }
            catch (Exception ex) { }

            return status;
        }
        public Screen GetScreenById(int screenId)
        {
            List<SCREEN_T> lst = DBcontext.SCREEN_T.Where(x => x.ID == screenId).ToList();
            return lst.Select(x => new Screen
            {
                Screen_Name = x.SCREEN_NAME_TX,
                Screen_Title = x.SCREEN_TITLE_TX,
                Screen_File_Name = x.SCREEN_FILE_NAME_TX,
                Screen_Style = x.SCREEN_STYLE_TX,
                Screen_Script = x.SCREEN_SCRIPT_TX,
                Screen_Content = x.SCREEN_CONTENT_TX,
                Screen_Class_Name = x.SCREEN_CLASS_NAME_TX,
                Screen_Ref_Class_Name = x.SCREEN_REF_CLASS_NAME_TX,
                Screen_Class_Static = x.SCREEN_CLASS_STATIC_YN == false ? false : true,
                Screen_Method_Name = x.SCREEN_METHOD_NAME_TX,
                Screen_Method_Static = x.SCREEN_METHOD_STATIC_YN == false ? false : true,
                Table_Name = x.TABLE_NAME_TX,
                Action_TX = x.ACTION_TX,
                Active = x.ACTIVE_YN,
                ID = x.ID,
                Schema_Name=x.SCHEMA_NAME_TX,
                lstAppModule = DBcontext.APP_MODULE_T.Select(m => new AppModule
                {
                    Id = m.ID,
                    ModuleName = m.MODULE_NAME_TX
                }).ToList(),

                mandSList = DBcontext.SCREEN_T.Select(m => new Screen
                {
                    MandScreenId = m.ID,
                    MandScreenName = m.SCREEN_NAME_TX
                }).ToList(),
                AppModuleId =x.APP_MODULE_ID,
                MandScreenId=Convert.ToInt32( x.MANDATORY_SCR_ID)
            }).FirstOrDefault();
        }
        public List<Screen> GetScreenss()
        {
            List<APP_MODULE_T> lstAppModule = DBcontext.APP_MODULE_T.ToList();
            List<SCREEN_T> lstScreen = this.DBcontext.SCREEN_T.ToList();
            //return DBcontext.SCREEN_T.Where(x => x.ACTIVE_YN == true).Select(x => new Screen
            //{
            //    Screen_Name = x.SCREEN_NAME_TX,
            //    Screen_Title = x.SCREEN_TITLE_TX,
            //    Screen_File_Name = x.SCREEN_FILE_NAME_TX,
            //    Screen_Style = x.SCREEN_STYLE_TX,
            //    Screen_Script = x.SCREEN_SCRIPT_TX,
            //    Screen_Content = x.SCREEN_CONTENT_TX,
            //    Screen_Class_Name = x.SCREEN_CLASS_NAME_TX,
            //    Screen_Class_Static = x.SCREEN_CLASS_STATIC_YN == false ? false : true,
            //    Screen_Method_Name = x.SCREEN_METHOD_NAME_TX,
            //    Screen_Method_Static = x.SCREEN_METHOD_STATIC_YN == false ? false : true,
            //    Table_Name = x.TABLE_NAME_TX,
            //    AppModuleName= lstAppModule.Where(m=>m.ID==x.APP_MODULE_ID).Select(m=>m.MODULE_NAME_TX).FirstOrDefault(),
            //    Action_TX = x.ACTION_TX,
            //    Active = x.ACTIVE_YN,
            //    ID = x.ID,
            //    ModifyDate = x.UPDATED_DT
            //}).OrderBy(x => x.ModifyDate).ToList();
            List<SCREEN_T> lst = DBcontext.SCREEN_T.Where(x => x.ACTIVE_YN == true).ToList();

            return (from x in lst
                    select new Screen
                    {
                        Screen_Name = x.SCREEN_NAME_TX,
                        Screen_Title = x.SCREEN_TITLE_TX,
                        Screen_File_Name = x.SCREEN_FILE_NAME_TX,
                        Screen_Style = x.SCREEN_STYLE_TX,
                        Screen_Script = x.SCREEN_SCRIPT_TX,
                        Screen_Content = x.SCREEN_CONTENT_TX,
                        Screen_Class_Name = x.SCREEN_CLASS_NAME_TX,
                        Screen_Ref_Class_Name = x.SCREEN_REF_CLASS_NAME_TX,
                        Screen_Class_Static = x.SCREEN_CLASS_STATIC_YN == false ? false : true,
                        Screen_Method_Name = x.SCREEN_METHOD_NAME_TX,
                        Screen_Method_Static = x.SCREEN_METHOD_STATIC_YN == false ? false : true,
                        Table_Name = x.TABLE_NAME_TX,
                        Schema_Name=x.SCHEMA_NAME_TX,
                        lstAppModule = DBcontext.APP_MODULE_T.Select(m => new AppModule
                        {
                            Id = m.ID,
                            ModuleName = m.MODULE_NAME_TX
                        }).ToList(),

                        mandSList = DBcontext.SCREEN_T.Select(m => new Screen
                        {
                            MandScreenId = m.ID,
                            MandScreenName = m.SCREEN_NAME_TX
                        }).ToList(),
                        AppModuleName = lstAppModule.Where(m => m.ID == x.APP_MODULE_ID).Select(m => m.MODULE_NAME_TX).FirstOrDefault(),
                        MandScreenName = lstScreen.Where(m => m.ID == x.MANDATORY_SCR_ID).Select(m => m.SCREEN_NAME_TX).FirstOrDefault(),
                        Action_TX = x.ACTION_TX,
                 Active = x.ACTIVE_YN,
                 ID = x.ID,
                 ModifyDate = x.UPDATED_DT
             }).ToList();
        }
        #endregion

        #region User Type
        public int AddUserType(UserType usertype)
        {
            int status = 0;
            try
            {
                USER_TYPE_T resUsertype = DBcontext.USER_TYPE_T.Where(x => x.ID == usertype.ID).FirstOrDefault();

                USER_TYPE_T obj;
                if (resUsertype == null)
                {
                    obj = new USER_TYPE_T();
                    obj.CREATED_DT = DateTime.Now;
                    obj.CREATED_BY = Convert.ToInt32(HttpContext.Current.Session["USER_T_ID"]);
                }
                else
                    obj = resUsertype;

                obj.USER_TYPE_TX = usertype.User_Type;
                obj.USER_TYPE_DESC_TX = usertype.User_Desc;
                obj.ACTIVE_YN = usertype.Active;
                obj.UPDATED_BY = Convert.ToInt32(HttpContext.Current.Session["USER_T_ID"]);
                obj.UPDATED_DT = DateTime.Now;

                if (resUsertype == null)
                    DBcontext.USER_TYPE_T.Add(obj);
                status = DBcontext.SaveChanges();
            }
            catch (Exception ex) { }

            return status;
        }
        public UserType GetUserTypeById(int usertypeId)
        {
            return DBcontext.USER_TYPE_T.Where(x => x.ID == usertypeId).Select(x => new UserType
            {
                User_Type = x.USER_TYPE_TX,
                User_Desc = x.USER_TYPE_DESC_TX,
                Active = x.ACTIVE_YN,
                ID = x.ID
            }).FirstOrDefault();
        }
        public List<UserType> GetUserTypes()
        {
            return DBcontext.USER_TYPE_T.Select(x => new UserType
            {
                User_Type = x.USER_TYPE_TX,
                User_Desc = x.USER_TYPE_DESC_TX,
                Active = x.ACTIVE_YN,
                ID = x.ID,
                ModifyDate = x.UPDATED_DT
            }).OrderBy(x => x.ModifyDate).ToList();
        }
        #endregion

        #region Reports
        public bool InsertReport(Report report)
        {
            try
            {
                REPORT_T reportt = new REPORT_T();
                reportt.REPORT_NAME_TX = report.ReportName;
                reportt.SQL_TX = report.SQL;
                reportt.COLUMNS_TX = report.Column;
                reportt.TITLE_TX = report.Title;
                reportt.ACTIVE_YN = report.Active;
                reportt.CREATED_BY = 1;
                reportt.CREATED_DT = DateTime.Now;
                reportt.UPDATED_BY = 1;
                reportt.UPDATED_DT = DateTime.Now;
                DBcontext.REPORT_T.Add(reportt);
                DBcontext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        public List<REPORT_T> GetAllReports()
        {
            return DBcontext.REPORT_T.ToList();
        }
        public REPORT_T GetReportById(int RoleId)
        {
            return DBcontext.REPORT_T.Where(x => x.ID == RoleId).FirstOrDefault();
        }
        public bool UpdateReport(Report report)
        {
            try
            {
                REPORT_T reportt = DBcontext.REPORT_T.Where(x => x.ID == report.Id).FirstOrDefault();
                reportt.REPORT_NAME_TX = report.ReportName;
                reportt.SQL_TX = report.SQL;
                reportt.COLUMNS_TX = report.Column;
                reportt.TITLE_TX = report.Title;
                reportt.ACTIVE_YN = report.Active;
                DBcontext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        public bool InsertUserRole(UserRole userrole)
        {
            try
            {
                USER_ROLE_T userrolet = new USER_ROLE_T();
                userrolet.USER_ID = userrole.UserId;
                userrolet.ROLE_ID = userrole.RoleId;
                userrolet.ACTIVE_YN = userrole.Active;
                userrolet.CREATED_BY = 1;
                userrolet.CREATED_DT = DateTime.Now;
                userrolet.UPDATED_BY = 1;
                userrolet.UPDATED_DT = DateTime.Now;
                DBcontext.USER_ROLE_T.Add(userrolet);
                DBcontext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        public List<USER_ROLE_T> GetAllUserRole()
        {
            return DBcontext.USER_ROLE_T.ToList();
        }
        public USER_ROLE_T GetUserRoleById(int RoleId)
        {
            return DBcontext.USER_ROLE_T.Where(x => x.ID == RoleId).FirstOrDefault();
        }
        public bool UpdateUserRole(UserRole userrole)
        {
            try
            {
                USER_ROLE_T userrolet = DBcontext.USER_ROLE_T.Where(x => x.ID == userrole.Id).FirstOrDefault();
                userrolet.USER_ID = userrole.UserId;
                userrolet.ROLE_ID = userrole.RoleId;
                userrolet.ACTIVE_YN = userrole.Active;
                DBcontext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        public List<USER_T> GetALlUsers()
        {
            return
  DBcontext.USER_T.ToList();
        }
        public string UserName(int UserId)
        {
            return DBcontext.USER_T.Where(x => x.ID == UserId).Select(x => x.USER_NAME_TX).FirstOrDefault();
        }
        public string RoleName(int RoleId)
        {
            return DBcontext.ROLE_T.Where(x => x.ID == RoleId).Select(x => x.ROLE_NAME_TX).FirstOrDefault();
        }
        #endregion

        
        #region Screen Component
        public bool InsertScreenComponent(ScreenComponent screencomonent)
        {
            try
            {
                SCREEN_COMP_T screencomponentt = new SCREEN_COMP_T();
                screencomponentt.SCREEN_ID = screencomonent.ScreenId;
                screencomponentt.REF_ID = screencomonent.ManComId;
                screencomponentt.COMP_STYLE_TX = screencomonent.ComponentStyle;
                screencomponentt.COMP_SCRIPT_TX = screencomonent.ComponentScript;
                screencomponentt.SCREEN_REF_METHOD_NAME_TX = screencomonent.ScreenRefMethod;
                screencomponentt.SCHEMA_NAME_TX = screencomonent.SchemaName;
                screencomponentt.TABLE_NAME_TX = screencomonent.TableName;
                screencomponentt.MANDATORY_YN = screencomonent.IsMand;
                screencomponentt.WHERE_TX = screencomonent.Where;
                screencomponentt.ORDER_NM = screencomonent.OrderNumber;
                screencomponentt.COMP_TYPE_NM = screencomonent.ComponentType;
                screencomponentt.COMP_CONTENT_TX = screencomonent.ComponentContent;
                screencomponentt.COMP_NAME_TX = screencomonent.ComponentName;
                screencomponentt.COMP_VALUE_TX = screencomonent.ComponentValue;
                screencomponentt.COMP_TEXT_TX = screencomonent.ComponentText;
                screencomponentt.COMP_CLASS_NAME_TX = screencomonent.ComponentClassName;
                screencomponentt.COMP_CLASS_STATIC_YN = screencomonent.ComponentClassStatic;
                screencomponentt.COMP_METHOD_NAME_TX = screencomonent.ComponentMethodName;
                screencomponentt.COMP_METHOD_STATIC_YN = screencomonent.ComponentMethodStatic;
                screencomponentt.COLUMN_NAME_TX = screencomonent.ColumnName;
                screencomponentt.READ_WRITE_YN = screencomonent.ReadWrite;
                screencomponentt.SQL_TX = screencomonent.SQL;
                screencomponentt.REPORT_ID = screencomonent.ReportId;
                screencomponentt.ACTIVE_YN = screencomonent.Active;
                screencomponentt.CREATED_BY = 1;
                screencomponentt.CREATED_DT = DateTime.Now;
                screencomponentt.UPDATED_BY = 1;
                screencomponentt.UPDATED_DT = DateTime.Now;
                DBcontext.SCREEN_COMP_T.Add(screencomponentt);
                DBcontext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        public List<SCREEN_COMP_T> GetAllScreenComponent()
        {
            return DBcontext.SCREEN_COMP_T.ToList();
        }
        public SCREEN_COMP_T GetScreenComponentById(int ScreenId)
        {
            return DBcontext.SCREEN_COMP_T.Where(x => x.ID == ScreenId).FirstOrDefault();
        }
        public bool UpdateScreenComponent(ScreenComponent screencomonent)
        {
            try
            {
                SCREEN_COMP_T screencomponentt = DBcontext.SCREEN_COMP_T.Where(x => x.ID == screencomonent.Id).FirstOrDefault();
                screencomponentt.SCREEN_ID = screencomonent.ScreenId;
                screencomponentt.REF_ID = screencomonent.ManComId;
                screencomponentt.COMP_STYLE_TX = screencomonent.ComponentStyle;
                screencomponentt.COMP_SCRIPT_TX = screencomonent.ComponentScript;
                screencomponentt.SCREEN_REF_METHOD_NAME_TX = screencomonent.ScreenRefMethod;
                screencomponentt.SCHEMA_NAME_TX = screencomonent.SchemaName;
                screencomponentt.TABLE_NAME_TX = screencomonent.TableName;
                screencomponentt.MANDATORY_YN = screencomonent.IsMand;
                screencomponentt.WHERE_TX = screencomonent.Where;
                screencomponentt.ORDER_NM = screencomonent.OrderNumber;
                screencomponentt.COMP_TYPE_NM = screencomonent.ComponentType;
                screencomponentt.COMP_CONTENT_TX = screencomonent.ComponentContent;
                screencomponentt.COMP_NAME_TX = screencomonent.ComponentName;
                screencomponentt.COMP_VALUE_TX = screencomonent.ComponentValue;
                screencomponentt.COMP_TEXT_TX = screencomonent.ComponentText;
                screencomponentt.COMP_CLASS_NAME_TX = screencomonent.ComponentClassName;
                screencomponentt.COMP_CLASS_STATIC_YN = screencomonent.ComponentClassStatic;
                screencomponentt.COMP_METHOD_NAME_TX = screencomonent.ComponentMethodName;
                screencomponentt.COMP_METHOD_STATIC_YN = screencomonent.ComponentMethodStatic;
                screencomponentt.COLUMN_NAME_TX = screencomonent.ColumnName;
                screencomponentt.READ_WRITE_YN = screencomonent.ReadWrite;
                screencomponentt.SQL_TX = screencomonent.SQL;
                screencomponentt.REPORT_ID = screencomonent.ReportId;
                screencomponentt.ACTIVE_YN = screencomonent.Active;
                screencomponentt.CREATED_BY = 1;
                screencomponentt.CREATED_DT = DateTime.Now;
                screencomponentt.UPDATED_BY = 1;
                screencomponentt.UPDATED_DT = DateTime.Now;

                DBcontext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        #endregion

        #region User
        public bool InsertUser(User user)
        {
            try
            {
                USER_T usert = new USER_T();
                usert.USER_ID = user.UserId;
                usert.USER_TYPE_ID = user.UserTypeId;
                usert.USER_NAME_TX = user.UserName;
                usert.LOGIN_ID = user.LoginId;
                usert.LOGIN_PWD_TX = user.LoginPwd;
                usert.ACTIVE_YN = user.Active;
                usert.CREATED_BY = 1;
                usert.CREATED_DT = DateTime.Now;
                usert.UPDATED_BY = 1;
                usert.UPDATED_DT = DateTime.Now;
                DBcontext.USER_T.Add(usert);
                DBcontext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        public List<USER_T> GetAllUser()
        {
            return DBcontext.USER_T.ToList();
        }
        public USER_T GetUserById(int UserId)
        {
            return DBcontext.USER_T.Where(x => x.ID == UserId).FirstOrDefault();
        }
        public bool UpdateUser(User user)
        {
            try
            {
                USER_T usert = DBcontext.USER_T.Where(x => x.ID == user.Id).FirstOrDefault();
                usert.USER_ID = user.UserId;
                usert.USER_NAME_TX = user.UserName;
                usert.USER_TYPE_ID = user.UserTypeId;
                usert.LOGIN_ID = user.LoginId;
                usert.LOGIN_PWD_TX = user.LoginPwd;
                usert.ACTIVE_YN = user.Active;
                usert.UPDATED_BY = 1;
                usert.UPDATED_DT = DateTime.Now;
                DBcontext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        #endregion

        public string UserType(long UserTypeId)
        {
            return DBcontext.USER_TYPE_T.Where(x => x.ID == UserTypeId).Select(x => x.USER_TYPE_TX).FirstOrDefault();
        }

        public List<USER_TYPE_T> UserTypleList()
        {
            return DBcontext.USER_TYPE_T.ToList();
        }

        #region Role Responsibility Mapping
        public int AddRoleRespMapping(RoleResponsibilityMapping rolerespMapping)
        {
            int status = 0;
            try
            {
                UpdateRoleResp(rolerespMapping.Role_Id);
                int roleId = rolerespMapping.Role_Id;
                string[] arrResp = rolerespMapping.RespName.Split(',');

                for (int i = 0; i < arrResp.Length - 1; i++)
                {
                    int respId = Convert.ToInt32(arrResp[i]);

                    ROLE_RESP_T resRoleResp = DBcontext.ROLE_RESP_T.Where(x => x.ROLE_ID == roleId && x.RESP_ID == respId).FirstOrDefault();

                    ROLE_RESP_T obj;
                    if (resRoleResp == null)
                    {
                        obj = new ROLE_RESP_T();
                        obj.CREATED_DT = DateTime.Now;
                        obj.CREATED_BY = Convert.ToInt32(HttpContext.Current.Session["USER_T_ID"]);
                    }
                    else
                        obj = resRoleResp;

                    obj.ROLE_ID = roleId;
                    obj.RESP_ID = respId;
                    obj.ACTIVE_YN = true;
                    obj.UPDATED_BY = Convert.ToInt32(HttpContext.Current.Session["USER_T_ID"]);
                    obj.UPDATED_DT = DateTime.Now;

                    if (resRoleResp == null)
                        DBcontext.ROLE_RESP_T.Add(obj);
                    status = DBcontext.SaveChanges();
                }
            }
            catch (Exception ex) { }

            return status;
        }

        public int UpdateRoleResp(int roleId)
        {
            int status = 0;
            List<ROLE_RESP_T> resRoleResp = DBcontext.ROLE_RESP_T.Where(x => x.ROLE_ID == roleId && x.ACTIVE_YN == true).ToList();
            if (resRoleResp.Count > 0)
            {
                foreach (var item in resRoleResp)
                {
                    item.UPDATED_BY = Convert.ToInt32(HttpContext.Current.Session["USER_T_ID"]);
                    item.UPDATED_DT = DateTime.Now;
                    item.ACTIVE_YN = false;
                    DBcontext.SaveChanges();
                }
            }

            return status;
        }

        public RoleResponsibilityMapping GetRoleResponsibilityById(int respId)
        {
            RoleResponsibilityMapping resRoleResp = (from objROLE_RESP_T in DBcontext.ROLE_RESP_T
                                                     join objRole in DBcontext.ROLE_T
                                                     on objROLE_RESP_T.ROLE_ID equals objRole.ID
                                                     join objResp in DBcontext.RESPONSIBILITY_T
                                                     on objROLE_RESP_T.RESP_ID equals objResp.ID
                                                     where objROLE_RESP_T.ID == respId
                                                     select new RoleResponsibilityMapping
                                                     {
                                                         Role_Id = objROLE_RESP_T.ROLE_ID,
                                                         Resp_Id = objROLE_RESP_T.RESP_ID,
                                                         RoleName = objRole.ROLE_NAME_TX,
                                                         RespName = objResp.RESP_NAME_TX,
                                                         Active = objROLE_RESP_T.ACTIVE_YN,
                                                         ID = objROLE_RESP_T.ID
                                                     }).FirstOrDefault();

            return resRoleResp;
        }

        public List<RoleResponsibilityMapping> GetRoleResponsibilites()
        {
            List<RoleResponsibilityMapping> resRoleResp = (from objROLE_RESP_T in DBcontext.ROLE_RESP_T
                                                           join objRole in DBcontext.ROLE_T
                                                           on objROLE_RESP_T.ROLE_ID equals objRole.ID
                                                           join objResp in DBcontext.RESPONSIBILITY_T
                                                           on objROLE_RESP_T.RESP_ID equals objResp.ID
                                                           where objROLE_RESP_T.ACTIVE_YN == true
                                                           && objRole.ACTIVE_YN == true
                                                           && objResp.ACTIVE_YN == true
                                                           select new RoleResponsibilityMapping
                                                           {
                                                               Role_Id = objROLE_RESP_T.ROLE_ID,
                                                               Resp_Id = objROLE_RESP_T.RESP_ID,
                                                               RoleName = objRole.ROLE_NAME_TX,
                                                               RespName = objResp.RESP_NAME_TX,
                                                               ModifyDate = objROLE_RESP_T.UPDATED_DT,
                                                               Active = objROLE_RESP_T.ACTIVE_YN,
                                                               ID = objROLE_RESP_T.ID
                                                           }).OrderBy(x => x.Role_Id).ThenBy(x => x.ModifyDate).ToList();

            List<RoleResponsibilityMapping> resRoleMap = new List<RoleResponsibilityMapping>();
            int[] arr = resRoleResp.Select(x => x.Role_Id).Distinct().ToArray();

            for (int i = 0; i < arr.Length; i++)
            {
                var resRespMap = resRoleResp.Where(x => x.Role_Id == arr[i]).ToList();
                if (resRespMap.Count > 0)
                {
                    RoleResponsibilityMapping objRoleResponsibilityMapping = new RoleResponsibilityMapping();
                    objRoleResponsibilityMapping.Role_Id = arr[i];
                    objRoleResponsibilityMapping.RoleName = resRespMap.Select(x => x.RoleName).FirstOrDefault();
                    objRoleResponsibilityMapping.Active = resRespMap.Select(x => x.Active).FirstOrDefault();
                    foreach (var item in resRespMap)
                    {
                        objRoleResponsibilityMapping.RespName += item.RespName + ",";
                    }
                    resRoleMap.Add(objRoleResponsibilityMapping);
                }
            }

            return resRoleMap;
        }

        public List<Responsibility> lstResponsibilities()
        {
            return DBcontext.RESPONSIBILITY_T.Where(x => x.ACTIVE_YN == true).Select(x => new Responsibility
            {
                Resp_Name = x.RESP_NAME_TX,
                ID = x.ID
            }).ToList();
        }


        public List<Roles> lstRoles()
        {
            return DBcontext.ROLE_T.Where(x => x.ACTIVE_YN == true).Select(x => new Roles
            {
                RoleName = x.ROLE_NAME_TX,
                Id = x.ID
            }).ToList();
        }

        public List<Responsibility> GetResponsibilitiesByRole(int roleId)
        {
            List<Responsibility> resps = new List<Responsibility>();
            if (roleId != 0)
            {
                resps = (from objROLE_RESP_T in DBcontext.ROLE_RESP_T
                         join objRole in DBcontext.ROLE_T
                         on objROLE_RESP_T.ROLE_ID equals objRole.ID
                         join objResp in DBcontext.RESPONSIBILITY_T
                         on objROLE_RESP_T.RESP_ID equals objResp.ID
                         where objROLE_RESP_T.ROLE_ID == roleId
                         && objROLE_RESP_T.ACTIVE_YN == true
                         select new Responsibility
                         {
                             ID = objROLE_RESP_T.RESP_ID,
                             Resp_Name = objResp.RESP_NAME_TX,
                             //Resp_Desc= objResp.RESP_DESC_TX,
                             //Resp_Type= objResp.RESP_TYPE_NM,
                             //Read_Write=objResp.READ_WRITE_YN,
                             //Active=objResp.ACTIVE_YN
                         }).ToList();
            }



            var res = (from p in lstResponsibilities()
                       where !resps.Select(x => x.Resp_Name).Contains(p.Resp_Name)
                       select p).ToList();

            return res;
        }

        public List<RoleResponsibilityMapping> GetResponsibilityByRoleId(int roleId)
        {
            List<RoleResponsibilityMapping> resResps = new List<RoleResponsibilityMapping>();
            if (roleId != 0)
            {
                resResps = (from objROLE_RESP_T in DBcontext.ROLE_RESP_T
                            join objRole in DBcontext.ROLE_T
                            on objROLE_RESP_T.ROLE_ID equals objRole.ID
                            join objResp in DBcontext.RESPONSIBILITY_T
                            on objROLE_RESP_T.RESP_ID equals objResp.ID
                            where objROLE_RESP_T.ROLE_ID == roleId
                            && objROLE_RESP_T.ACTIVE_YN == true
                            select new RoleResponsibilityMapping
                            {
                                Resp_Id = objROLE_RESP_T.RESP_ID,
                                RespName = objResp.RESP_NAME_TX
                            }).ToList();
            }

            return resResps;
        }
        #endregion

        #region User Responsibility Mapping
        public int AddUserRespMapping(UserRespMapping userrespMapping)
        {
            int status = 0;
            try
            {
                UpdateUserResp(userrespMapping.User_Id);
                int UserId = userrespMapping.User_Id;
                if (userrespMapping.RespName != null)
                {
                    string[] arrResp = userrespMapping.RespName.Split(',');

                    for (int i = 0; i < arrResp.Length - 1; i++)
                    {
                        int respId = Convert.ToInt32(arrResp[i]);
                        USER_RESP_T resRoleResp = DBcontext.USER_RESP_T.Where(x => x.USER_ID == UserId && x.RESP_ID == respId).FirstOrDefault();

                        USER_RESP_T obj;
                        if (resRoleResp == null)
                        {
                            obj = new USER_RESP_T();
                            obj.CREATED_DT = DateTime.Now;
                            obj.CREATED_BY = Convert.ToInt32(HttpContext.Current.Session["USER_T_ID"]);
                        }
                        else
                            obj = resRoleResp;

                        obj.USER_ID = UserId;
                        obj.RESP_ID = respId;
                        obj.ACTIVE_YN = true;
                        obj.UPDATED_BY = Convert.ToInt32(HttpContext.Current.Session["USER_T_ID"]);
                        obj.UPDATED_DT = DateTime.Now;

                        if (resRoleResp == null)
                            DBcontext.USER_RESP_T.Add(obj);
                        status = DBcontext.SaveChanges();
                    }
                }
            }
            catch (Exception ex) { }

            return status;
        }
        public int AddUserRoleMapping(UserRoleMapping userroleMapping)
        {
            int status = 0;
            try
            {
                UpdateUserRole(userroleMapping.User_Id);
                int UserId = userroleMapping.User_Id;
                string[] arrRole = userroleMapping.RoleName.Split(',');

                for (int i = 0; i < arrRole.Length - 1; i++)
                {
                    int roleId = Convert.ToInt32(arrRole[i]);
                    USER_ROLE_T resRoleRole = DBcontext.USER_ROLE_T.Where(x => x.USER_ID == UserId && x.ROLE_ID == roleId).FirstOrDefault();

                    USER_ROLE_T obj;
                    if (resRoleRole == null)
                    {
                        obj = new USER_ROLE_T();
                        obj.CREATED_DT = DateTime.Now;
                        obj.CREATED_BY = Convert.ToInt32(HttpContext.Current.Session["USER_T_ID"]);
                    }
                    else
                        obj = resRoleRole;

                    obj.USER_ID = UserId;
                    obj.ROLE_ID = roleId;
                    obj.ACTIVE_YN = true;
                    obj.UPDATED_BY = Convert.ToInt32(HttpContext.Current.Session["USER_T_ID"]);
                    obj.UPDATED_DT = DateTime.Now;

                    if (resRoleRole == null)
                        DBcontext.USER_ROLE_T.Add(obj);
                    status = DBcontext.SaveChanges();
                }
            }
            catch (Exception ex) { }

            return status;
        }
        public int UpdateUserRole(int UserId)
        {
            int status = 0;
            List<USER_ROLE_T> resUserRole = DBcontext.USER_ROLE_T.Where(x => x.USER_ID == UserId && x.ACTIVE_YN == true).ToList();
            if (resUserRole.Count > 0)
            {
                foreach (var item in resUserRole)
                {
                    item.UPDATED_BY = Convert.ToInt32(HttpContext.Current.Session["USER_T_ID"]);
                    item.UPDATED_DT = DateTime.Now;
                    item.ACTIVE_YN = false;
                    DBcontext.SaveChanges();
                }
            }

            return status;
        }
        public int UpdateUserResp(int UserId)
        {
            int status = 0;
            List<USER_RESP_T> resUserResp = DBcontext.USER_RESP_T.Where(x => x.USER_ID == UserId && x.ACTIVE_YN == true).ToList();
            if (resUserResp.Count > 0)
            {
                foreach (var item in resUserResp)
                {
                    item.UPDATED_BY = Convert.ToInt32(HttpContext.Current.Session["USER_T_ID"]);
                    item.UPDATED_DT = DateTime.Now;
                    item.ACTIVE_YN = false;
                    DBcontext.SaveChanges();
                }
            }

            return status;
        }

        public List<User> lstUsers()
        {
            return DBcontext.USER_T.Where(x => x.ACTIVE_YN == true).Select(x => new User { UserName = x.USER_NAME_TX, Id = x.ID }).ToList();
        }

        public List<UserRespMapping> GetUserResponsibilites()
        {
            List<UserRespMapping> resUserResp = (from objUser_RESP_T in DBcontext.USER_RESP_T
                                                 join objUser in DBcontext.USER_T
                                                 on objUser_RESP_T.USER_ID equals objUser.ID
                                                 join objResp in DBcontext.RESPONSIBILITY_T
                                                 on objUser_RESP_T.RESP_ID equals objResp.ID
                                                 where objUser_RESP_T.ACTIVE_YN == true
                                                 && objUser.ACTIVE_YN == true
                                                 && objResp.ACTIVE_YN == true
                                                 select new UserRespMapping
                                                 {
                                                     User_Id = objUser_RESP_T.USER_ID,
                                                     Resp_Id = objUser_RESP_T.RESP_ID,
                                                     UserName = objUser.USER_NAME_TX,
                                                     RespName = objResp.RESP_NAME_TX,
                                                     ModifyDate = objUser_RESP_T.UPDATED_DT,
                                                     Active = objUser_RESP_T.ACTIVE_YN,
                                                     ID = objUser_RESP_T.ID
                                                 }).OrderBy(x => x.ModifyDate).ToList();

            List<UserRespMapping> resRespMap = new List<UserRespMapping>();
            int[] arr = resUserResp.Select(x => x.User_Id).Distinct().ToArray();

            for (int i = 0; i < arr.Length; i++)
            {
                var resUserRespMap = resUserResp.Where(x => x.User_Id == arr[i]).ToList();
                if (resUserRespMap.Count > 0)
                {
                    UserRespMapping objUserResponsibilityMapping = new UserRespMapping();
                    objUserResponsibilityMapping.User_Id = arr[i];
                    objUserResponsibilityMapping.UserName = resUserRespMap.Select(x => x.UserName).FirstOrDefault();
                    objUserResponsibilityMapping.Active = resUserRespMap.Select(x => x.Active).FirstOrDefault();
                    foreach (var item in resUserRespMap)
                    {
                        objUserResponsibilityMapping.RespName += item.RespName + ",";
                    }
                    resRespMap.Add(objUserResponsibilityMapping);
                }
            }

            return resRespMap;
        }

        public List<UserRoleMapping> GetUserRoles()
        {
            List<UserRoleMapping> resUserRole = (from objUSER_ROLE_T in DBcontext.USER_ROLE_T
                                                 join objUser in DBcontext.USER_T
                                                 on objUSER_ROLE_T.USER_ID equals objUser.ID
                                                 join objRole in DBcontext.ROLE_T
                                                 on objUSER_ROLE_T.ROLE_ID equals objRole.ID
                                                 where objUSER_ROLE_T.ACTIVE_YN == true
                                                 && objUser.ACTIVE_YN == true
                                                 && objRole.ACTIVE_YN == true
                                                 select new UserRoleMapping
                                                 {
                                                     User_Id = objUSER_ROLE_T.USER_ID,
                                                     RoleId = objUSER_ROLE_T.ROLE_ID,
                                                     UserName = objUser.USER_NAME_TX,
                                                     RoleName = objRole.ROLE_NAME_TX,
                                                     ModifyDate = objUSER_ROLE_T.UPDATED_DT,
                                                     Active = objUSER_ROLE_T.ACTIVE_YN,
                                                     ID = objUSER_ROLE_T.ID
                                                 }).OrderBy(x => x.ModifyDate).ToList();

            List<UserRoleMapping> resRoleMap = new List<UserRoleMapping>();
            int[] arr = resUserRole.Select(x => x.User_Id).Distinct().ToArray();

            for (int i = 0; i < arr.Length; i++)
            {
                var resUserRoleMap = resUserRole.Where(x => x.User_Id == arr[i]).ToList();
                if (resUserRoleMap.Count > 0)
                {
                    UserRoleMapping objUserRoleMapping = new UserRoleMapping();
                    objUserRoleMapping.User_Id = arr[i];
                    objUserRoleMapping.UserName = resUserRoleMap.Select(x => x.UserName).FirstOrDefault();
                    objUserRoleMapping.Active = resUserRoleMap.Select(x => x.Active).FirstOrDefault();
                    foreach (var item in resUserRoleMap)
                    {
                        objUserRoleMapping.RoleName += item.RoleName + ",";
                    }
                    resRoleMap.Add(objUserRoleMapping);
                }
            }

            return resRoleMap;
        }

        public List<UserRespMapping> GetResponsibilityByUserId(int userId)
        {
            List<UserRespMapping> resResps = new List<UserRespMapping>();
            if (userId != 0)
            {
                resResps = (from objUSER_RESP_T in DBcontext.USER_RESP_T
                            join objUser in DBcontext.USER_T
                            on objUSER_RESP_T.USER_ID equals objUser.ID
                            join objResp in DBcontext.RESPONSIBILITY_T
                            on objUSER_RESP_T.RESP_ID equals objResp.ID
                            where objUSER_RESP_T.USER_ID == userId
                            && objUSER_RESP_T.ACTIVE_YN == true
                            select new UserRespMapping
                            {
                                Resp_Id = objUSER_RESP_T.RESP_ID,
                                RespName = objResp.RESP_NAME_TX
                            }).ToList();
            }

            return resResps;
        }
        public List<UserRoleMapping> GetRoleByUserId(int userId)
        {
            List<UserRoleMapping> resRoles = new List<UserRoleMapping>();
            if (userId != 0)
            {
                resRoles = (from objUSER_ROLE_T in DBcontext.USER_ROLE_T
                            join objUser in DBcontext.USER_T
                            on objUSER_ROLE_T.USER_ID equals objUser.ID
                            join objRole in DBcontext.ROLE_T
                            on objUSER_ROLE_T.ROLE_ID equals objRole.ID
                            where objUSER_ROLE_T.USER_ID == userId
                            && objUSER_ROLE_T.ACTIVE_YN == true
                            select new UserRoleMapping
                            {
                                RoleId = objUSER_ROLE_T.ROLE_ID,
                                RoleName = objRole.ROLE_NAME_TX
                            }).ToList();
            }

            return resRoles;
        }
        public List<Responsibility> GetResponsibilitiesByUser(int userId)
        {
            List<Responsibility> resps = new List<Responsibility>();
            if (userId != 0)
            {
                resps = (from objUser_RESP_T in DBcontext.USER_RESP_T
                         join objUser in DBcontext.USER_T
                         on objUser_RESP_T.USER_ID equals objUser.ID
                         join objResp in DBcontext.RESPONSIBILITY_T
                         on objUser_RESP_T.RESP_ID equals objResp.ID
                         where objUser_RESP_T.USER_ID == userId
                         && objUser_RESP_T.ACTIVE_YN == true
                         select new Responsibility
                         {
                             ID = objUser_RESP_T.RESP_ID,
                             Resp_Name = objResp.RESP_NAME_TX,
                             //Resp_Desc= objResp.RESP_DESC_TX,
                             //Resp_Type= objResp.RESP_TYPE_NM,
                             //Read_Write=objResp.READ_WRITE_YN,
                             //Active=objResp.ACTIVE_YN
                         }).ToList();
            }

            var res = (from p in lstResponsibilities()
                       where !resps.Select(x => x.Resp_Name).Contains(p.Resp_Name)
                       select p).ToList();

            return res;
        }

        public List<Roles> GetRolesByUser(int userId)
        {
            List<Roles> resps = new List<Roles>();
            if (userId != 0)
            {
                resps = (from objUSER_ROLE_T in DBcontext.USER_ROLE_T
                         join objUser in DBcontext.USER_T
                         on objUSER_ROLE_T.USER_ID equals objUser.ID
                         join objRole in DBcontext.ROLE_T
                         on objUSER_ROLE_T.ROLE_ID equals objRole.ID
                         where objUSER_ROLE_T.USER_ID == userId
                         && objUSER_ROLE_T.ACTIVE_YN == true
                         select new Roles
                         {
                             Id = objUSER_ROLE_T.ROLE_ID,
                             RoleName = objRole.ROLE_NAME_TX,
                             //Resp_Desc= objResp.RESP_DESC_TX,
                             //Resp_Type= objResp.RESP_TYPE_NM,
                             //Read_Write=objResp.READ_WRITE_YN,
                             //Active=objResp.ACTIVE_YN
                         }).ToList();
            }

            var role = (from p in lstRoles()
                        where !resps.Select(x => x.RoleName).Contains(p.RoleName)
                        select p).ToList();

            return role;
        }

        #endregion

        public int GetRoleOrder()
        {
            try
            {
                return DBcontext.ROLE_T.Max(x => x.ORDER_NM);
            }
            catch (Exception ex)
            {
                return 0;
            }

        }

        public bool InsertApplication(Application application)
        {
            try
            {
                APPLICATION_T applicationt = new APPLICATION_T();
                applicationt.APPLICATION_NAME_TX = application.ApplicationName;
                applicationt.APPLICATIONDESC_TX = application.ApplicationDescription;
                applicationt.SCHEMA_NAME_TX = application.SchemaName;
                applicationt.MANDATORY_APP_ID = application.MandAppId;
                applicationt.ACTIVE_YN = application.Active;
                applicationt.WEB_APP_ID = application.WebAppId;
                applicationt.CREATED_BY = 1;
                applicationt.CREATED_DT = DateTime.Now;
                applicationt.UPDATED_BY = 1;
                applicationt.UPDATED_DT = DateTime.Now;
                DBcontext.APPLICATION_T.Add(applicationt);
                DBcontext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        public List<APPLICATION_T> GetAllApplications()
        {

            return DBcontext.APPLICATION_T.ToList();
        }
        public APPLICATION_T GetApplicationById(int ApplicationId)
        {
            return DBcontext.APPLICATION_T.Where(x => x.ID == ApplicationId).FirstOrDefault();
        }
        public bool UpdateApplication(Application application)
        {
            try
            {
                APPLICATION_T applicationt = DBcontext.APPLICATION_T.Where(x => x.ID == application.Id).FirstOrDefault();
                applicationt.APPLICATION_NAME_TX = application.ApplicationName;                
                applicationt.APPLICATIONDESC_TX = application.ApplicationDescription;
                applicationt.SCHEMA_NAME_TX = application.SchemaName;
                applicationt.MANDATORY_APP_ID = application.MandAppId;
                applicationt.WEB_APP_ID = application.WebAppId;
                applicationt.ACTIVE_YN = application.Active;                
                applicationt.UPDATED_BY = 1;
                applicationt.UPDATED_DT = DateTime.Now;
                applicationt.CREATED_BY = 1;
                applicationt.CREATED_DT = DateTime.Now;
                DBcontext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public bool InsertAppModule(AppModule appmodule)
        {
            try
            {
                APP_MODULE_T appmodulet = new APP_MODULE_T();
                appmodulet.MODULE_NAME_TX = appmodule.ModuleName;
                appmodulet.MODULE_DESC_TX = appmodule.ModuleDescription;
                appmodulet.SCHEMA_NAME_TX = appmodule.SchemaName;
                appmodulet.MANDATORY_MOD_ID = appmodule.MandModuleId;
                appmodulet.APP_ID = appmodule.AppId;
                appmodulet.ACTIVE_YN = appmodule.Active;
                appmodulet.CREATED_BY = 1;
                appmodulet.CREATED_DT = DateTime.Now;
                appmodulet.UPDATED_BY = 1;
                appmodulet.UPDATED_DT = DateTime.Now;
                DBcontext.APP_MODULE_T.Add(appmodulet);
                DBcontext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        public List<APP_MODULE_T> GetAllAppModules()
        {

            return DBcontext.APP_MODULE_T.ToList();
        }
        public APP_MODULE_T GetAppModuleById(int appmoduleid)
        {
            return DBcontext.APP_MODULE_T.Where(x => x.ID == appmoduleid).FirstOrDefault();
        }
        public bool UpdateAppModule(AppModule appmodule)
        {
            try
            {
                APP_MODULE_T appmodulet = DBcontext.APP_MODULE_T.Where(x => x.ID == appmodule.Id).FirstOrDefault();
                appmodulet.MODULE_NAME_TX = appmodule.ModuleName;
                appmodulet.SCHEMA_NAME_TX = appmodule.SchemaName;
                appmodulet.MANDATORY_MOD_ID = appmodule.MandModuleId;
                appmodulet.MODULE_DESC_TX = appmodule.ModuleDescription;
                appmodulet.APP_ID = appmodule.AppId;
                appmodulet.ACTIVE_YN = appmodule.Active;
                appmodulet.UPDATED_BY = 1;
                appmodulet.UPDATED_DT = DateTime.Now;
                appmodulet.CREATED_BY = 1;
                appmodulet.CREATED_DT = DateTime.Now;
                DBcontext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public bool InsertMenu(Menu menu)
        {
            
            try
            {
                MENU_T menut = new MENU_T();
                menut.MENU_NAME_TX = menu.MenuName;
                menut.MENU_LABEL_TX = menu.MenuLabelName;
                menut.ORDER_NM = menu.OrderNumber;
                menut.WEB_APP_ID = menu.WebAppId;
                menut.SCREEN_ID = menu.ScreenId;
                menut.REF_ID = menu.ParentMenuId;
                menut.ACTIVE_YN = menu.Active;
                menut.CREATED_BY = 1;
                menut.CREATED_DT = DateTime.Now;
                menut.UPDATED_BY = 1;
                menut.UPDATED_DT = DateTime.Now;
                DBcontext.MENU_T.Add(menut);
                DBcontext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        public List<MENU_T> GetAllMenus()
        {

            return DBcontext.MENU_T.ToList();
        }
        public MENU_T GetMenuById(int MenuId)
        {
            return DBcontext.MENU_T.Where(x => x.ID == MenuId).FirstOrDefault();
        }
        public bool UpdateMenu(Menu menu)
        {
            try
            {
                MENU_T menut = DBcontext.MENU_T.Where(x => x.ID == menu.Id).FirstOrDefault();
                menut.MENU_NAME_TX = menu.MenuName;
                menut.MENU_LABEL_TX = menu.MenuLabelName;
                menut.WEB_APP_ID = menu.WebAppId;
                menut.SCREEN_ID = menu.ScreenId;
                menut.REF_ID = menu.ParentMenuId;
                menut.ORDER_NM = menu.OrderNumber;
                menut.ACTIVE_YN = menu.Active;
                menut.UPDATED_BY = 1;
                menut.UPDATED_DT = DateTime.Now;
                menut.CREATED_BY = 1;
                menut.CREATED_DT = DateTime.Now;
                DBcontext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }


        public bool InsertWebApplication(WebApplication webapplication)
        {
            try
            {
                WEB_APPLICATION_T webapplicationt = new WEB_APPLICATION_T();
                webapplicationt.WEB_APP_NAME_TX = webapplication.WebAppName;
                webapplicationt.WEB_APP_DESC_TX = webapplication.WebAppDesc;
                webapplicationt.SCHEMA_NAME_TX = webapplication.SchemaName;
                webapplicationt.WEB_URL_TX = webapplication.WebURL;
                webapplicationt.ACTIVE_YN = webapplication.Active;
                webapplicationt.CREATED_BY = 1;
                webapplicationt.CREATED_DT = DateTime.Now;
                webapplicationt.UPDATED_BY = 1;
                webapplicationt.UPDATED_DT = DateTime.Now;
                DBcontext.WEB_APPLICATION_T.Add(webapplicationt);
                DBcontext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        public List<WEB_APPLICATION_T> GetAllWebApplications()
        {

            return DBcontext.WEB_APPLICATION_T.ToList();
        }
        public WEB_APPLICATION_T GetWebApplicationById(int ApplicationId)
        {
            return DBcontext.WEB_APPLICATION_T.Where(x => x.ID == ApplicationId).FirstOrDefault();
        }
        public bool UpdateWebApplication(WebApplication application)
        {
            try
            {
                WEB_APPLICATION_T applicationt = DBcontext.WEB_APPLICATION_T.Where(x => x.ID == application.Id).FirstOrDefault();
                applicationt.WEB_APP_NAME_TX = application.WebAppName;
                applicationt.WEB_APP_DESC_TX = application.WebAppDesc;
                applicationt.SCHEMA_NAME_TX = application.SchemaName;
                applicationt.WEB_URL_TX = application.WebURL;
                applicationt.ACTIVE_YN = application.Active;
                applicationt.UPDATED_BY = 1;
                applicationt.UPDATED_DT = DateTime.Now;
                applicationt.CREATED_BY = 1;
                applicationt.CREATED_DT = DateTime.Now;
                DBcontext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
    }
}