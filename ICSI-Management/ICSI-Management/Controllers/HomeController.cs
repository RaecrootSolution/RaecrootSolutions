using ICSI_Management.DBContext;
using ICSI_Management.Models;
using ICSI_Management.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace ICSI_Management.Controllers
{
    public class HomeController : Controller
    {
        private IUserRepository _userRepository;
        public HomeController()
        {
            this._userRepository = new UserRepository(new ICSI_DBModelEntities());
        }
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(Login obj)
        {
            bool check = false;
            bool checkUdn = false;

            SUPER_USER_T objtbluser = new SUPER_USER_T();
            string message = string.Empty;
           
                //int userid = 0;
                SUPER_USER_T tblUser = _userRepository.GetUserByUserName(obj.UserName);
                if (tblUser == null)
                {
                    ViewBag.Message = "You are not registered. Please go to Member registration link.";
                    return View(obj);
                }
                else
                {
                    //userid = tblUser.USER_ID;
                }
                objtbluser.USER_NAME_TX = obj.UserName;
                objtbluser.USER_ID = tblUser.LOGIN_ID;
                Session["LogedUserID"] = objtbluser.USER_ID;
                Session["UserID"] = tblUser.USER_ID;
                Session["USER_T_ID"] = tblUser.ID;
                objtbluser.LOGIN_PWD_TX = obj.Password;
                //for count user
                Session["UserName"] = objtbluser.USER_NAME_TX;
                check = _userRepository.CheckLogin(objtbluser);
                if (check == false)
                {
                    message = "Password is incorrect";
                    ViewBag.Message = message;
                }
                else
                {

                    return RedirectToAction("Dashboard");

                }
            
           
            return View(obj);
        }

        public ActionResult Dashboard()
        {
            return View();
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpGet]
        public ActionResult Responsibility(int value = 0)
        {
            int id = value;
            Responsibility objresponsibility = new Responsibility();
            if (Session["USER_T_ID"] != null)
            {
                if (id == 0)
                {
                    objresponsibility.Read_Write = true;
                    objresponsibility.Active = true;
                }
                else
                {
                    objresponsibility = _userRepository.GetResponsibilityById(id);
                }
                objresponsibility.lstResponsibility = _userRepository.GetResponsibilities();
                objresponsibility.lstDropDown = new List<DropdownValue> { new DropdownValue { Text = "Please Text", Value = 0 } };
                if (TempData["disabled"] != null)
                {
                    ViewBag.disabled = TempData["disabled"];
                }
                if (TempData["Message"] != null)
                {
                    ViewBag.Message = TempData["Message"];
                }
            }
            else
                return RedirectToAction("Index");

            return View(objresponsibility);
        }

        [HttpPost]
        public ActionResult Responsibility(Responsibility responsibility)
        {
            if (Session["USER_T_ID"] != null)
            {

                
                int status = _userRepository.AddResponsibility(responsibility);
                    if (status > 0)
                    {                        
                       
                        if(responsibility.ID!=0)
                        {
                            TempData["disabled"] = "disabled";
                            TempData["Message"] = "Record Updated Successfully";
                        }
                        else
                        {
                            TempData["disabled"] = "";
                            TempData["Message"] = "Record inserted Successfully";
                        }
                        
                        return RedirectToAction("Responsibility");
                    }
                        
                
            }
            else
                return RedirectToAction("Index");
            return View(responsibility);
        }

        [HttpGet]
        public ActionResult RoleMaster(int? Id)
        {
            if (Session["USER_T_ID"] != null)
            {
                List<ROLE_T> allRoles = _userRepository.GetAllRoles();
                Roles role = new Roles();
                if (Id != null)
                {
                    ROLE_T rolet = _userRepository.GetRoleById(Convert.ToInt32(Id));
                    if (rolet != null)
                    {
                        role.Id = rolet.ID;
                        role.RoleName = rolet.ROLE_NAME_TX;
                        role.RoleSysName = rolet.ROLE_SYM_NAME_TX;
                        role.RoleDescription = rolet.ROLE_DESC_TX;
                        role.Active = Convert.ToBoolean(rolet.ACTIVE_YN);
                        role.RoleOrder = rolet.ORDER_NM;

                    }
                }
                else
                {
                    role.Active = true;
                    role.RoleOrder = _userRepository.GetRoleOrder() + 1;
                }

                if (allRoles != null && allRoles.Count > 0)
                {
                    role.AllRoles = (from roles in allRoles
                                     select new Roles
                                     {
                                         Id = roles.ID,
                                         RoleName = roles.ROLE_NAME_TX,
                                         RoleSysName = roles.ROLE_SYM_NAME_TX,
                                         RoleDescription = roles.ROLE_DESC_TX,
                                         Active = Convert.ToBoolean(roles.ACTIVE_YN),
                                         RoleOrder = roles.ORDER_NM
                                     }).ToList<Roles>();

                }
                if (TempData["Message"] != null)
                {
                    ViewBag.Message = TempData["Message"];
                }
                if (TempData["disabled"] != null)
                {
                    ViewBag.disabled = TempData["disabled"];
                }
               
                return View(role);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public ActionResult RoleMaster(Roles roles)
        {
            if (Session["USER_T_ID"] != null)
            {
               
                    if (roles.Id == 0)
                    {
                        bool result = _userRepository.InsertRole(roles);
                        if (result)
                        {
                            TempData["Message"] = "Role inserted Successfully";                            
                            TempData["disabled"] = "disabled";
                            return RedirectToAction("RoleMaster");

                        }
                        else
                        {
                            ViewBag.Message = "Error..";
                            TempData["disabled"] = "";
                        }
                    }
                    else
                    {
                        bool result = _userRepository.UpdateRole(roles);
                        if (result)
                        {

                            TempData["Message"] = "Role updated Successfully";
                            TempData["disabled"] = "disabled";
                            return RedirectToAction("RoleMaster", 0);
                        }
                        else
                        {
                            TempData["disabled"] = "";
                            ViewBag.Message = "Error..";
                        }



                    }
                

                return View(roles);
            }
            else
            {
                return RedirectToAction("Index");
            }

        }

        [HttpGet]
        public ActionResult Screen(int id = 0)
        {
            Screen objscreen = new Screen();
           
            if (Session["USER_T_ID"] != null)
            {
                if (id == 0)
                {
                    objscreen.lstAppModule = _userRepository.GetAllAppModules().Select(x => new AppModule
                    {
                        Id = x.ID,
                        ModuleName = x.MODULE_NAME_TX
                    }).ToList();

                    objscreen.mandSList = _userRepository.GetScreenss().Where(x=>x.ID!=id)
                        .Select(x => new Screen
                    {
                        MandScreenId = x.ID,
                        MandScreenName = x.Schema_Name
                    }).ToList();
                    objscreen.Active = true;
                }
                    
                else
                {                    
                    objscreen = _userRepository.GetScreenById(id);
                }
                    

                objscreen.lstScreen = _userRepository.GetScreenss();
                if (TempData["Message"] != null)
                {
                    ViewBag.Message = TempData["Message"];
                }
                if (TempData["disabled"] != null)
                {
                    ViewBag.disabled = TempData["disabled"];
                }
            }
            else
                return RedirectToAction("Index");
            return View(objscreen);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Screen(Screen screen)
        {
            if (Session["USER_T_ID"] != null)
            {
              
                    int status = _userRepository.AddScreen(screen);
                    if (status > 0)
                    {
                      
                        if (screen.ID != 0)
                        {
                            TempData["disabled"] = "disabled";
                            TempData["Message"] = "Record Updated Successfully";
                        }
                        else
                        {
                            TempData["disabled"] = "";
                            TempData["Message"] = "Record inserted Successfully";
                        }

                        return RedirectToAction("Screen");
                    }
                
            }
            else
                return RedirectToAction("Index");
            return View(screen);
        }

        [HttpGet]
        public ActionResult UserType(int id = 0)
        {
            UserType objusertype = new UserType();
            if (Session["USER_T_ID"] != null)
            {
                if (id == 0)
                    objusertype.Active = true;
                else
                    objusertype = _userRepository.GetUserTypeById(id);

                objusertype.lstUserType = _userRepository.GetUserTypes();
                if (TempData["Message"] != null)
                {
                    ViewBag.Message = TempData["Message"];
                }
                if (TempData["disabled"] != null)
                {
                    ViewBag.disabled = TempData["disabled"];
                }
            }
            else
                return RedirectToAction("Index");
            return View(objusertype);
        }
        
        [HttpPost]
        public ActionResult UserType(UserType usertype)
        {

            if (Session["USER_T_ID"] != null)
            {
               
                    int status = _userRepository.AddUserType(usertype);
                    if (status > 0)
                    {
                        TempData["Message"] = "Record inserted Successfully";
                        if (usertype.ID != 0)
                        {
                            TempData["disabled"] = "disabled";
                        }
                        else
                        {
                            TempData["disabled"] = "";
                        }

                        return RedirectToAction("UserType");
                    }
                
            }
            else
                return RedirectToAction("Index");
            return View(usertype);
        }

        [HttpGet]
        public ActionResult ReportMaster(int? Id)
        {
            if (Session["USER_T_ID"] != null)
            {
                List<REPORT_T> allReports = _userRepository.GetAllReports();
                Report report = new Report();
                if (Id != null)
                {
                    REPORT_T reportt = _userRepository.GetReportById(Convert.ToInt32(Id));
                    if (reportt != null)
                    {
                        report.Id = reportt.ID;
                        report.ReportName = reportt.REPORT_NAME_TX;
                        report.SQL = reportt.SQL_TX;
                        report.Column = reportt.COLUMNS_TX;
                        report.Title = reportt.TITLE_TX;
                        report.Active = reportt.ACTIVE_YN;

                    }
                }
                else
                {
                    report.Active = true;
                }

                if (allReports != null && allReports.Count > 0)
                {
                    report.AllReport = (from reports in allReports
                                        select new Report
                                        {
                                            Id = reports.ID,
                                            ReportName = reports.REPORT_NAME_TX,
                                            SQL = reports.SQL_TX,
                                            Column = reports.COLUMNS_TX,
                                            Title = reports.TITLE_TX,
                                            Active = reports.ACTIVE_YN
                                        }).ToList<Report>();

                }
                if (TempData["Message"] != null)
                {
                    ViewBag.Message = TempData["Message"];
                }
                if (TempData["disabled"] != null)
                {
                    ViewBag.disabled = TempData["disabled"];
                }
                return View(report);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public ActionResult ReportMaster(Report report)
        {
            if (Session["USER_T_ID"] != null)
            {
               
                    if (report.Id == 0)
                    {
                        bool result = _userRepository.InsertReport(report);
                        if (result)
                        {
                            TempData["Message"] = "Report inserted Successfully";
                            return RedirectToAction("ReportMaster");

                        }
                        else
                        {
                            ViewBag.Message = "Error..";
                        }
                    }
                    else
                    {
                        bool result = _userRepository.UpdateReport(report);
                        if (result)
                        {

                            TempData["Message"] = "Report updated Successfully";
                            TempData["disabled"] = "disabled";
                            return RedirectToAction("ReportMaster", 0);
                        }
                        else
                        {
                            ViewBag.Message = "Error..";
                        }



                    }
                

                return View(report);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        [HttpGet]
        public ActionResult UserRoleMaster(int? Id)
        {
            List<USER_ROLE_T> alluserrole = _userRepository.GetAllUserRole();
            UserRole userrole = new UserRole();
            if (Id != null)
            {
                USER_ROLE_T reportt = _userRepository.GetUserRoleById(Convert.ToInt32(Id));
                if (reportt != null)
                {
                    userrole.Id = reportt.ID;
                    userrole.UserId = reportt.USER_ID;
                    userrole.RoleId = reportt.ROLE_ID;
                    userrole.Active = reportt.ACTIVE_YN;


                }
            }

            if (alluserrole != null && alluserrole.Count > 0)
            {
                userrole.userrole = (from ur in alluserrole
                                     select new UserRole
                                     {
                                         Id = ur.ID,
                                         UserId = ur.USER_ID,
                                         RoleId = ur.ROLE_ID,
                                         Active = ur.ACTIVE_YN,
                                         RoleName = _userRepository.RoleName(ur.ROLE_ID),
                                         UserName = _userRepository.UserName(ur.USER_ID)
                                     }).ToList<UserRole>();

            }
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }
            return View(userrole);
        }
        [HttpPost]
        public ActionResult UserRoleMaster(UserRole userrole)
        {
           
                if (userrole.Id == 0)
                {
                    bool result = _userRepository.InsertUserRole(userrole);
                    if (result)
                    {
                        TempData["Message"] = "Report inserted Successfully";
                        return RedirectToAction("UserRoleMaster");

                    }
                    else
                    {
                        ViewBag.Message = "Error..";
                    }
                }
                else
                {
                    bool result = _userRepository.UpdateUserRole(userrole);
                    if (result)
                    {

                        TempData["Message"] = "updated Successfully";
                        return RedirectToAction("UserRoleMaster", 0);
                    }
                    else
                    {
                        ViewBag.Message = "Error..";
                    }



                }
            

            return View(userrole);
        }

        [HttpGet]
        public ActionResult RoleMapping(int id = 0)
        {
            RoleResponsibilityMapping objrolerespMap = new RoleResponsibilityMapping();
            if (Session["USER_T_ID"] != null)
            {
                if (id == 0)
                {
                    objrolerespMap.Active = true;
                    objrolerespMap.lstResponsibilityByRoleId = _userRepository.GetResponsibilityByRoleId(0);
                    objrolerespMap.lstResponsibilities = _userRepository.lstResponsibilities();
                    objrolerespMap.lstRoleRespMapping = _userRepository.GetRoleResponsibilites();
                }
                else
                {
                    //objrolerespMap = _userRepository.GetRoleResponsibilityById(id);
                    objrolerespMap.lstResponsibilityByRoleId = _userRepository.GetResponsibilityByRoleId(id);
                    //objrolerespMap.lstExistingResponsibilities = _userRepository.GetResponsibilitiesByRole(id);
                    objrolerespMap.lstResponsibilities = _userRepository.GetResponsibilitiesByRole(id);
                    objrolerespMap.Role_Id = id;
                    objrolerespMap.ID = id;

                    //objrolerespMap.lstResponsibilities = _userRepository.lstResponsibilities();
                    objrolerespMap.lstRoleRespMapping = _userRepository.GetRoleResponsibilites();
                }
                objrolerespMap.lstRolls = _userRepository.lstRoles();
                /*
                 objrolerespMap.lstResponsibilities = _userRepository.lstResponsibilities();
                 objrolerespMap.lstRoleRespMapping = _userRepository.GetRoleResponsibilites();
                 */
                if (TempData["Message"] != null)
                {
                    ViewBag.Message = TempData["Message"];
                }
                if (TempData["disabled"] != null)
                {
                    ViewBag.disabled = TempData["disabled"];
                }
            }
            else
                return RedirectToAction("Index");
            return View(objrolerespMap);
        }

        [HttpPost]
        public ActionResult RoleMapping(RoleResponsibilityMapping roleResponsibilityMapping, string lstRolls)
        {
            if (Session["USER_T_ID"] != null)
            {
                if (roleResponsibilityMapping.ID != 0)
                    roleResponsibilityMapping.Role_Id = roleResponsibilityMapping.ID;
                if (roleResponsibilityMapping.SelectedRes != null)
                {
                    for (int i = 0; i < roleResponsibilityMapping.SelectedRes.Length; i++)
                    {
                        roleResponsibilityMapping.RespName += roleResponsibilityMapping.SelectedRes[i] + ",";
                    }
                }
                int status = _userRepository.AddRoleRespMapping(roleResponsibilityMapping);
                TempData["Message"] = "Record inserted Successfully";
                if (roleResponsibilityMapping.ID != 0)
                {
                    TempData["disabled"] = "disabled";
                }
                else
                {
                    TempData["disabled"] = "";
                }
                return RedirectToAction("RoleMapping");
            }
            else
                return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult UserRespMapping(int id = 0)
        {
            UserRespMapping objuserrespMap = new UserRespMapping();
            if (Session["USER_T_ID"] != null)
            {
                if (id == 0)
                {
                    objuserrespMap.Active = true;
                    objuserrespMap.lstResponsibilityByUserId = _userRepository.GetResponsibilityByUserId(0);
                    objuserrespMap.lstResponsibilities = _userRepository.lstResponsibilities();

                }
                else
                {
                    objuserrespMap.lstResponsibilityByUserId = _userRepository.GetResponsibilityByUserId(id);
                    objuserrespMap.lstResponsibilities = _userRepository.GetResponsibilitiesByUser(id);
                    objuserrespMap.User_Id = id;
                    objuserrespMap.ID = id;
                }
                objuserrespMap.lstUserRespMapping = _userRepository.GetUserResponsibilites();
                objuserrespMap.lstUsers = _userRepository.lstUsers();
                if (TempData["Message"] != null)
                {
                    ViewBag.Message = TempData["Message"];
                }
                if (TempData["disabled"] != null)
                {
                    ViewBag.disabled = TempData["disabled"];
                }
            }
            else
                return RedirectToAction("Index");
            return View(objuserrespMap);
        }

        [HttpPost]
        public ActionResult UserRespMapping(UserRespMapping userResponsibilityMapping)
        {
            if (Session["USER_T_ID"] != null)
            {
                if (userResponsibilityMapping.SelectedRes != null)
                {
                    for (int i = 0; i < userResponsibilityMapping.SelectedRes.Length; i++)
                    {
                        userResponsibilityMapping.RespName += userResponsibilityMapping.SelectedRes[i] + ",";
                    }
                }
                int status = _userRepository.AddUserRespMapping(userResponsibilityMapping);
                TempData["Message"] = "Record inserted Successfully";
                if (userResponsibilityMapping.ID != 0)
                {
                    TempData["disabled"] = "disabled";
                }
                else
                {
                    TempData["disabled"] = "";
                }
                return RedirectToAction("UserRespMapping");
            }
            else
                return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult ScreenComponentMaster(int? Id)
        {
            if (Session["USER_T_ID"] != null)
            {
                Dictionary<int, string> CompType = new Dictionary<int, string>();
                CompType.Add(1, "Hidden");
                CompType.Add(2, "Text");
                CompType.Add(3, "Password");
                CompType.Add(4, "Radio Group");
                CompType.Add(5, "Radio Button");
                CompType.Add(6, "Checkbox");
                CompType.Add(7, "Textarea");
                CompType.Add(8, "Dropdown");
                CompType.Add(9, "Submit Button");
                CompType.Add(10, "Button");
                CompType.Add(11, "List");
                CompType.Add(12, "Image");
                CompType.Add(13, "Div");
                CompType.Add(14, "Span");
                CompType.Add(15, "Anchor");
                CompType.Add(16, "Form");
                CompType.Add(17, "Table With Query");
                CompType.Add(18, "Table");
                CompType.Add(19, "Tr");
                CompType.Add(20, "td");
                CompType.Add(21, "th");
                CompType.Add(22, "Report");
                CompType.Add(23, "Custom");
                CompType.Add(24, "Search");
                CompType.Add(25, "Date");
                CompType.Add(26, "DateTime");
                
                List<SCREEN_COMP_T> allscreencoponent = _userRepository.GetAllScreenComponent();
                ScreenComponent screencomponet = new ScreenComponent();

                screencomponet.lstScreen = _userRepository.GetScreenss().Select(x => new Screen
                {
                    ID = x.ID,
                    Screen_Name = x.Screen_Name
                }).ToList();

                screencomponet.lstMndCom = _userRepository.GetAllScreenComponent().Where(x=>x.ID!= screencomponet.Id)
                    
                    .Select(x => new ScreenComponent
                {
                    ManComId = x.ID,
                    ManComName = x.COMP_NAME_TX
                }).ToList();

                screencomponet.lstReport = _userRepository.GetAllReports().Select(x => new Report
                {
                    Id = x.ID,
                    ReportName = x.REPORT_NAME_TX
                }).ToList();
                if (Id != null)
                {
                    SCREEN_COMP_T screencomponentt = _userRepository.GetScreenComponentById(Convert.ToInt32(Id));
                    if (screencomponentt != null)
                    {
                        screencomponet.Id = screencomponentt.ID;
                        screencomponet.ScreenId = screencomponentt.SCREEN_ID;
                        screencomponet.ScreenName = screencomponet.lstScreen.Where(x => x.ID == screencomponentt.SCREEN_ID).Select(x => x.Screen_Name).FirstOrDefault();
                        screencomponet.ReportName = screencomponet.lstReport.Where(x => x.Id == screencomponentt.REPORT_ID).Select(x => x.ReportName).FirstOrDefault();
                        screencomponet.ManComId = Convert.ToInt32(screencomponentt.REF_ID);
                        screencomponet.OrderNumber = screencomponentt.ORDER_NM;
                        screencomponet.ComponentType = screencomponentt.COMP_TYPE_NM;
                        screencomponet.ComponentContent = screencomponentt.COMP_CONTENT_TX;
                        screencomponet.ComponentName = screencomponentt.COMP_NAME_TX;
                        screencomponet.ComponentValue = screencomponentt.COMP_VALUE_TX;
                        screencomponet.ComponentText = screencomponentt.COMP_TEXT_TX;
                        screencomponet.ComponentStyle = screencomponentt.COMP_STYLE_TX;
                        screencomponet.ComponentScript= screencomponentt.COMP_SCRIPT_TX;
                        screencomponet.ScreenRefMethod = screencomponentt.SCREEN_REF_METHOD_NAME_TX;
                        screencomponet.ComponentClassName = screencomponentt.COMP_CLASS_NAME_TX;
                        screencomponet.ComponentClassStatic = Convert.ToBoolean(screencomponentt.COMP_CLASS_STATIC_YN);
                        screencomponet.ComponentMethodName = screencomponentt.COMP_METHOD_NAME_TX;
                        screencomponet.ComponentMethodStatic = Convert.ToBoolean(screencomponentt.COMP_METHOD_STATIC_YN);
                        screencomponet.ColumnName = screencomponentt.COLUMN_NAME_TX;
                        screencomponet.ReadWrite = screencomponentt.READ_WRITE_YN;
                        screencomponet.SQL = screencomponentt.SQL_TX;
                        screencomponet.Where = screencomponentt.WHERE_TX;
                        screencomponet.SchemaName= screencomponentt.SCHEMA_NAME_TX;
                        screencomponet.TableName=screencomponentt.TABLE_NAME_TX;
                        screencomponet.IsMand = screencomponentt.MANDATORY_YN;
                        screencomponet.ReportId = Convert.ToInt32(screencomponentt.REPORT_ID);
                        screencomponet.Active = screencomponentt.ACTIVE_YN;


                    }
                }
                else
                {
                    screencomponet.Active = true;
                }

                if (allscreencoponent != null && allscreencoponent.Count > 0)
                {
                    screencomponet.lst = (from ur in allscreencoponent
                                          select new ScreenComponent
                                          {
                                              Id = ur.ID,
                                              ScreenId = ur.SCREEN_ID,
                                              RefId = Convert.ToInt32(ur.REF_ID),
                                              OrderNumber = ur.ORDER_NM,
                                              ManComId=Convert.ToInt32( ur.REF_ID),
                                              ComponentTypeName = CompType[ur.COMP_TYPE_NM],
                                              
                                              ComponentContent = ur.COMP_CONTENT_TX,
                                              ComponentName = ur.COMP_NAME_TX,
                                              ComponentValue = ur.COMP_VALUE_TX,
                                              ComponentText = ur.COMP_TEXT_TX,
                                              ComponentClassName = ur.COMP_CLASS_NAME_TX,
                                              ComponentClassStatic = Convert.ToBoolean(ur.COMP_CLASS_STATIC_YN),
                                              ComponentMethodName = ur.COMP_METHOD_NAME_TX,
                                              ComponentMethodStatic = Convert.ToBoolean(ur.COMP_METHOD_STATIC_YN),
                                              ColumnName = ur.COLUMN_NAME_TX,
                                              ReadWrite = ur.READ_WRITE_YN,
                                              SQL = ur.SQL_TX,
                                              ReportId = Convert.ToInt32(ur.REPORT_ID),
                                              Active = ur.ACTIVE_YN,
                                              ComponentScript=ur.COMP_SCRIPT_TX,
                                              ComponentStyle=ur.COMP_STYLE_TX,
                                              ScreenRefMethod=ur.SCREEN_REF_METHOD_NAME_TX,
                                              Where=ur.WHERE_TX,
                                              SchemaName=ur.SCHEMA_NAME_TX,
                                              TableName=ur.TABLE_NAME_TX,
                                              IsMand=ur.MANDATORY_YN,
                                              ScreenName = screencomponet.lstScreen.Where(x => x.ID == ur.SCREEN_ID).Select(x => x.Screen_Name).FirstOrDefault(),
                                              ManComName = screencomponet.lstMndCom.Where(x => x.ManComId == ur.REF_ID).Select(x => x.ComponentName).FirstOrDefault(),
                                              ReportName = screencomponet.lstReport.Where(x => x.Id == ur.REPORT_ID).Select(x => x.ReportName).FirstOrDefault()
                                          }).ToList<ScreenComponent>();

                }
                if (TempData["Message"] != null)
                {
                    ViewBag.Message = TempData["Message"];
                }
                if (TempData["disabled"] != null)
                {
                    ViewBag.disabled = TempData["disabled"];
                }
                return View(screencomponet);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public ActionResult ScreenComponentMaster(ScreenComponent screencoponent, string ComponentType)
        {

            if (Session["USER_T_ID"] != null)
            {
                
                    if (screencoponent.Id == 0)
                    {
                        bool result = _userRepository.InsertScreenComponent(screencoponent);
                        if (result)
                        {
                            TempData["Message"] = "Screen Component inserted Successfully";
                            return RedirectToAction("ScreenComponentMaster");

                        }
                        else
                        {
                            ViewBag.Message = "Error..";
                        }
                    }
                    else
                    {
                        bool result = _userRepository.UpdateScreenComponent(screencoponent);
                        if (result)
                        {

                            TempData["Message"] = "updated Successfully";
                            TempData["disabled"] = "disabled";
                            return RedirectToAction("ScreenComponentMaster", 0);
                        }
                        else
                        {
                            ViewBag.Message = "Error..";
                        }



                    }
                

                return View(screencoponent);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public ActionResult User(int? Id)
        {
            if (Session["USER_T_ID"] != null)
            {
                List<USER_T> alluser = _userRepository.GetAllUser();
                List<USER_TYPE_T> lstUserTpe = _userRepository.UserTypleList();
                User user = new User();

                if (lstUserTpe != null && lstUserTpe.Count > 0)
                {
                    user.usertypelst = (from data in lstUserTpe
                                        select new SelectListItem
                                        {
                                            Value = data.ID.ToString(),
                                            Text = data.USER_TYPE_TX
                                        }).ToList<SelectListItem>();
                }
                if (Id != null)
                {
                    USER_T usert = _userRepository.GetUserById(Convert.ToInt32(Id));
                    if (usert != null)
                    {
                        user.Id = usert.ID;
                        user.UserId = usert.USER_ID;
                        user.UserName = usert.USER_NAME_TX;
                        user.LoginId = usert.LOGIN_ID;
                        user.LoginPwd = usert.LOGIN_PWD_TX;
                        user.UserTypeId = usert.USER_TYPE_ID;
                        user.Active = usert.ACTIVE_YN;
                    }
                }
                else
                {
                    user.Active = true;
                }

                if (alluser != null && alluser.Count > 0)
                {
                    user.lst = (from ur in alluser
                                select new User
                                {
                                    Id = ur.ID,
                                    UserId = ur.USER_ID,
                                    UserTypeId = ur.USER_TYPE_ID,
                                    UserType = lstUserTpe.Where(x => x.ID == ur.USER_TYPE_ID).Select(x => x.USER_TYPE_TX).FirstOrDefault(),
                                    Active = ur.ACTIVE_YN,
                                    LoginId = ur.LOGIN_ID,
                                    LoginPwd = ur.LOGIN_PWD_TX,
                                    UserName = ur.USER_NAME_TX
                                }).ToList<User>();

                }
                if (TempData["Message"] != null)
                {
                    ViewBag.Message = TempData["Message"];
                }
                if (TempData["disabled"] != null)
                {
                    ViewBag.disabled = TempData["disabled"];
                }
                return View(user);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public ActionResult User(User user)
        {

            if (Session["USER_T_ID"] != null)
            {
               
                    if (user.Id == 0)
                    {
                        bool result = _userRepository.InsertUser(user);
                        if (result)
                        {
                            TempData["Message"] = "Record inserted Successfully";
                            return RedirectToAction("User");

                        }
                        else
                        {
                            ViewBag.Message = "Error..";
                        }
                    }
                    else
                    {
                        bool result = _userRepository.UpdateUser(user);
                        if (result)
                        {

                            TempData["Message"] = "updated Successfully";
                            TempData["disabled"] = "disabled";
                            return RedirectToAction("User", 0);
                        }
                        else
                        {
                            ViewBag.Message = "Error..";
                        }



                    }
                

                return View(user);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public ActionResult UserRoleMapping(int id = 0)
        {
            UserRoleMapping objuserroleMap = new UserRoleMapping();
            if (Session["USER_T_ID"] != null)
            {
                if (id == 0)
                {
                    objuserroleMap.Active = true;
                    objuserroleMap.lstRoleByUserId = _userRepository.GetRoleByUserId(0);
                    objuserroleMap.lstRoles = _userRepository.lstRoles();

                }
                else
                {
                    objuserroleMap.lstRoleByUserId = _userRepository.GetRoleByUserId(id);
                    objuserroleMap.lstRoles = _userRepository.GetRolesByUser(id);
                    objuserroleMap.User_Id = id;
                    objuserroleMap.ID = id;
                }
                objuserroleMap.lstUserRoleMapping = _userRepository.GetUserRoles();
                objuserroleMap.lstUsers = _userRepository.lstUsers();
                if (TempData["Message"] != null)
                {
                    ViewBag.Message = TempData["Message"];
                }
                if (TempData["disabled"] != null)
                {
                    ViewBag.disabled = TempData["disabled"];
                }
            }
            else
                return RedirectToAction("Index");
            return View(objuserroleMap);
        }

        [HttpPost]
        public ActionResult UserRoleMapping(UserRoleMapping userRoleMapping, string[] lstRoleByUserId)
        {
            if (Session["USER_T_ID"] != null)
            {
                if (lstRoleByUserId != null)
                {
                    for (int i = 0; i < lstRoleByUserId.Length; i++)
                    {
                        userRoleMapping.RoleName += lstRoleByUserId[i] + ",";
                    }

                    int status = _userRepository.AddUserRoleMapping(userRoleMapping);
                    if (status > 0)
                        return RedirectToAction("UserRoleMapping");
                }
                else
                {
                    TempData["Message"] = "Please select all Allocated value before Add/Update the user mapping.";
                    return RedirectToAction("UserRoleMapping");
                }
            }
            else
                return RedirectToAction("Index");
            return View(userRoleMapping);
        }

        [HttpGet]
        public ActionResult Application(int? Id)
        {
            if (Session["USER_T_ID"] != null)
            {
                List<APPLICATION_T> allApplications = _userRepository.GetAllApplications();
                string ProjectName = System.Web.HttpContext.Current.ApplicationInstance.GetType().BaseType.Assembly.GetName().Name;
                Application application = new Application();
                application.mandApps = allApplications.Where(x=>x.APPLICATION_NAME_TX!=ProjectName).Select(x => new Models.Application
                {
                    Id = x.ID,
                    ApplicationName = x.APPLICATION_NAME_TX
                }).ToList();
                application.WebApps = _userRepository.GetAllWebApplications().Select(x => new WebApplication
                {
                    Id = x.ID,
                    WebAppName = x.WEB_APP_NAME_TX
                }).ToList();
                if (Id != null)
                {
                    APPLICATION_T applicationt = _userRepository.GetApplicationById(Convert.ToInt32(Id));
                    if (applicationt != null)
                    {
                        application.ApplicationName = applicationt.APPLICATION_NAME_TX;
                        application.Id = applicationt.ID;
                        application.ApplicationDescription = applicationt.APPLICATIONDESC_TX;
                        application.SchemaName = applicationt.SCHEMA_NAME_TX;
                        application.MandAppId = Convert.ToInt32( applicationt.MANDATORY_APP_ID);
                        application.WebAppId = Convert.ToInt32( applicationt.WEB_APP_ID);
                        application.Active = Convert.ToBoolean(applicationt.ACTIVE_YN);


                    }
                }
                else
                {
                    application.Active = true;
                }

                if (allApplications != null && allApplications.Count > 0)
                {
                    application.AllApplications = (from app in allApplications
                                                   select new Application
                                                   {
                                                       Id = app.ID,
                                                       ApplicationName = app.APPLICATION_NAME_TX,
                                                       ApplicationDescription = app.APPLICATIONDESC_TX,
                                                       SchemaName = app.SCHEMA_NAME_TX,
                                                       MandAppName= allApplications.Where(x=>x.MANDATORY_APP_ID==app.MANDATORY_APP_ID).Select(x=>x.APPLICATION_NAME_TX).FirstOrDefault(),
                                                       WebAppName =application.WebApps.Where(x=>x.Id==app.WEB_APP_ID).Select(x=>x.WebAppName).FirstOrDefault(),
                                                       Active = Convert.ToBoolean(app.ACTIVE_YN)
                                                   }).ToList<Application>();

                }
                if (TempData["Message"] != null)
                {
                    ViewBag.Message = TempData["Message"];
                }
                if (TempData["disabled"] != null)
                {
                    ViewBag.disabled = TempData["disabled"];
                }
                return View(application);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        [HttpPost]        
        public ActionResult Application(Application application)
        {
            if (Session["USER_T_ID"] != null)
            {
                
                    if (application.Id == 0)
                    {
                        bool result = _userRepository.InsertApplication(application);
                        if (result)
                        {
                            TempData["Message"] = "Application inserted Successfully";
                            return RedirectToAction("Application");

                        }
                        else
                        {
                            ViewBag.Message = "Error..";
                        }
                    }
                    else
                    {
                        bool result = _userRepository.UpdateApplication(application);
                        if (result)
                        {

                            TempData["Message"] = "Application updated Successfully";
                            TempData["disabled"] = "disabled";
                            return RedirectToAction("Application", 0);
                        }
                        else
                        {
                            ViewBag.Message = "Error..";
                            ViewBag.disabled = "";
                        }



                    }
                

                return View(application);
            }
            else
            {
                return RedirectToAction("Index");
            }

        }

        [HttpGet]
        public ActionResult App_Module(int? Id)
        {
            if (Session["USER_T_ID"] != null)
            {
                List<APP_MODULE_T> allAppModules = _userRepository.GetAllAppModules();
                AppModule appmodule = new AppModule();
                appmodule.lstMandMoudule = _userRepository.GetAllAppModules().Select(x => new AppModule
                {
                    Id = x.ID,
                    ModuleName = x.MODULE_NAME_TX
                }).ToList();
                appmodule.apps = _userRepository.GetAllApplications().Select(x => new Application
                {
                    ApplicationName = x.APPLICATION_NAME_TX,
                    Id = x.ID
                }).ToList();
                if (Id != null)
                {
                    APP_MODULE_T appmodulet = _userRepository.GetAppModuleById(Convert.ToInt32(Id));
                    if (appmodulet != null)
                    {
                        appmodule.ModuleName = appmodulet.MODULE_NAME_TX;
                        appmodule.MandModuleId = Convert.ToInt32( appmodulet.MANDATORY_MOD_ID);
                        appmodule.SchemaName = appmodulet.SCHEMA_NAME_TX;
                        appmodule.Id = appmodulet.ID;
                        appmodule.ModuleDescription = appmodulet.MODULE_DESC_TX;
                        appmodule.AppId = appmodulet.APP_ID;
                        appmodule.Active = Convert.ToBoolean(appmodulet.ACTIVE_YN);


                    }
                }
                else
                {
                    appmodule.Active = true;
                }

                if (allAppModules != null && allAppModules.Count > 0)
                {
                    appmodule.AllAppModules = (from app in allAppModules
                                               select new AppModule
                                               {
                                                   Id = app.ID,
                                                   ModuleDescription = app.MODULE_DESC_TX,
                                                   ModuleName = app.MODULE_NAME_TX,
                                                   Active = Convert.ToBoolean(app.ACTIVE_YN),
                                                   SchemaName = app.SCHEMA_NAME_TX,
                                                   MandModuleName =appmodule.lstMandMoudule.Where(x=>x.Id==app.MANDATORY_MOD_ID).Select(x=>x.ModuleName).FirstOrDefault(),
                                                   AppName = appmodule.apps.Where(x => x.Id == app.APP_ID).Select(x => x.ApplicationName).FirstOrDefault()
                                               }).ToList<AppModule>();

                }
                if (TempData["Message"] != null)
                {
                    ViewBag.Message = TempData["Message"];
                }
                if (TempData["disabled"] != null)
                {
                    ViewBag.disabled = TempData["disabled"];
                }
                return View(appmodule);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public ActionResult App_Module(AppModule appModule)
        {
            if (Session["USER_T_ID"] != null)
            {
                
                    if (appModule.Id == 0)
                    {
                        bool result = _userRepository.InsertAppModule(appModule);
                        if (result)
                        {
                            TempData["Message"] = "Module inserted Successfully";
                            return RedirectToAction("App_Module");

                        }
                        else
                        {
                            ViewBag.Message = "Error..";
                        }
                    }
                    else
                    {
                        bool result = _userRepository.UpdateAppModule(appModule);
                        if (result)
                        {

                            TempData["Message"] = "Module updated Successfully";
                            TempData["disabled"] = "disabled";
                            return RedirectToAction("App_Module", 0);
                        }
                        else
                        {
                            ViewBag.Message = "Error..";
                        }



                    }
                

                return View(appModule);
            }
            else
            {
                return RedirectToAction("Index");
            }

        }

        [HttpGet]
        public ActionResult Menu(int? Id)
        {
            if (Session["USER_T_ID"] != null)
            {
                List<MENU_T> allMenus = _userRepository.GetAllMenus();
                Menu menu = new Models.Menu();
                menu.ParentMenues = allMenus.Select(x => new Menu { Id = x.ID, MenuName = x.MENU_NAME_TX }).ToList();
                menu.screens = _userRepository.GetScreenss().Select(x => new Screen
                {
                    Screen_Name = x.Screen_Name,
                    ID = x.ID
                }).ToList();
                menu.WebApps = _userRepository.GetAllWebApplications().Select(x => new WebApplication
                {
                    WebAppName = x.WEB_APP_NAME_TX,
                    Id = x.ID
                }).ToList();
                if (Id != null)
                {
                    MENU_T menut = _userRepository.GetMenuById(Convert.ToInt32(Id));
                    if (menut != null)
                    {
                        menu.MenuName = menut.MENU_NAME_TX;
                        menu.Id = menut.ID;
                        menu.MenuLabelName = menut.MENU_LABEL_TX;
                        menu.ScreenId = menut.SCREEN_ID;
                        menu.WebAppId = menut.WEB_APP_ID;
                        menu.OrderNumber = menut.ORDER_NM;
                        menu.ParentMenuId = Convert.ToInt32(menut.REF_ID);
                        menu.Active = Convert.ToBoolean(menut.ACTIVE_YN);


                    }
                }
                else
                {
                    menu.Active = true;
                }

                if (allMenus != null && allMenus.Count > 0)
                {
                    menu.AllMenues = (from app in allMenus
                                      select new Menu
                                      {
                                          Id = app.ID,
                                          MenuName = app.MENU_NAME_TX,
                                          MenuLabelName = app.MENU_LABEL_TX,
                                          OrderNumber = app.ORDER_NM,
                                          Active = Convert.ToBoolean(app.ACTIVE_YN),
                                          ParentMenuName=menu.ParentMenues.Where(x=>x.ParentMenuId==app.REF_ID).Select(x=>x.MenuName).FirstOrDefault(),
                                          WebAppName = menu.WebApps.Where(x => x.Id == app.WEB_APP_ID).Select(x => x.WebAppName).FirstOrDefault(),
                                          ScreenName = menu.screens.Where(x => x.ID == app.SCREEN_ID).Select(x => x.Screen_Name).FirstOrDefault()
                                      }).ToList<Menu>();

                }
                if (TempData["Message"] != null)
                {
                    ViewBag.Message = TempData["Message"];
                }
                if (TempData["disabled"] != null)
                {
                    ViewBag.disabled = TempData["disabled"];
                }
                return View(menu);

            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public ActionResult Menu(Menu menu)
        {
            if (Session["USER_T_ID"] != null)
            {
                
                    if (menu.Id == 0)
                    {
                        bool result = _userRepository.InsertMenu(menu);
                        if (result)
                        {
                            TempData["Message"] = "Menu inserted Successfully";
                            return RedirectToAction("Menu");

                        }
                        else
                        {
                            ViewBag.Message = "Error..";
                        }
                    }
                    else
                    {
                        bool result = _userRepository.UpdateMenu(menu);
                        if (result)
                        {

                            TempData["Message"] = "Menu updated Successfully";
                            TempData["disabled"] = "disabled";
                            return RedirectToAction("Menu", 0);
                        }
                        else
                        {
                            ViewBag.Message = "Error..";
                        }



                    }
                

                return View(menu);
            }
            else
            {
                return RedirectToAction("Index");
            }

        }

        public ActionResult Logout()
        {
            Session["UserName"] = null;
            Session["USER_T_ID"] = null;
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult WebApplication(int? Id)
        {
            if (Session["USER_T_ID"] != null)
            {
                List<WEB_APPLICATION_T> allApplications = _userRepository.GetAllWebApplications();
                WebApplication application = new WebApplication();
                if (Id != null)
                {
                    WEB_APPLICATION_T applicationt = _userRepository.GetWebApplicationById(Convert.ToInt32(Id));
                    if (applicationt != null)
                    {
                        
                        application.WebAppName = applicationt.WEB_APP_NAME_TX;
                        application.WebAppDesc = applicationt.WEB_APP_DESC_TX;
                        application.SchemaName = applicationt.SCHEMA_NAME_TX;
                        application.Id = applicationt.ID;
                        application.WebURL = applicationt.WEB_URL_TX;
                        application.Active = Convert.ToBoolean(applicationt.ACTIVE_YN);


                    }
                }
                else
                {
                    application.Active = true;
                }

                if (allApplications != null && allApplications.Count > 0)
                {
                    application.allapplications = (from app in allApplications
                                                   select new WebApplication
                                                   {
                                                       Id = app.ID,
                                                       WebAppName = app.WEB_APP_NAME_TX,
                                                       WebAppDesc = app.WEB_APP_DESC_TX,
                                                       WebURL=app.WEB_URL_TX,
                                                       SchemaName=app.SCHEMA_NAME_TX,
                                                       Active = Convert.ToBoolean(app.ACTIVE_YN)
                                                   }).ToList<WebApplication>();

                }
                if (TempData["Message"] != null)
                {
                    ViewBag.Message = TempData["Message"];
                }
                if (TempData["disabled"] != null)
                {
                    ViewBag.disabled = TempData["disabled"];
                }
                return View(application);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public ActionResult WebApplication(WebApplication application)
        {
            if (Session["USER_T_ID"] != null)
            {
               
                    if (application.Id == 0)
                    {
                        bool result = _userRepository.InsertWebApplication(application);
                        if (result)
                        {
                            TempData["Message"] = "WebApplication inserted Successfully";
                            return RedirectToAction("WebApplication");

                        }
                        else
                        {
                            ViewBag.Message = "Error..";
                        }
                    }
                    else
                    {
                        bool result = _userRepository.UpdateWebApplication(application);
                        if (result)
                        {

                            TempData["Message"] = "WebApplication updated Successfully";
                            TempData["disabled"] = "disabled";
                            return RedirectToAction("WebApplication", 0);
                        }
                        else
                        {
                            ViewBag.Message = "Error..";
                            ViewBag.disabled = "";
                        }



                    }
                

                return View(application);
            }
            else
            {
                return RedirectToAction("Index");
            }

        }

        public ActionResult GetDropdownList(string Id)
        {
            List<DropdownValue> lst = new List<DropdownValue>();
            if(Id=="1")
            {
                lst = _userRepository.GetAllWebApplications().Select(x => new DropdownValue
                {
                    Value=x.ID,
                    Text=x.WEB_APP_NAME_TX
                }).ToList();
            }
            else if (Id == "2")
            {
                lst = _userRepository.GetAllApplications().Select(x => new DropdownValue
                {
                    Value = x.ID,
                    Text = x.APPLICATION_NAME_TX
                }).ToList();
            }
           else if (Id == "3")
            {
                lst = _userRepository.GetAllAppModules().Select(x => new DropdownValue
                {
                    Value = x.ID,
                    Text = x.MODULE_NAME_TX
                }).ToList();
            }
            else if (Id == "4")
            {
                lst = _userRepository.GetScreenss().Select(x => new DropdownValue
                {
                    Value = x.ID,
                    Text = x.Screen_Name
                }).ToList();
            }
            else
            {
                lst.Add(new DropdownValue { Value = 1, Text = "----Please Select---" });
            }            
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result = javaScriptSerializer.Serialize(lst);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

    }
    public class DropdownValue
    {
        public int Value { get; set; }
        public string Text { get; set; }
    }
}