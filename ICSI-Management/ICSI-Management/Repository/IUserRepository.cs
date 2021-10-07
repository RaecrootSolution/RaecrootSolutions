using ICSI_Management.DBContext;
using ICSI_Management.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace ICSI_Management.Repository
{
    public interface IUserRepository
    {
        SUPER_USER_T GetUserByUserName(string LoginId);
        bool CheckLogin(SUPER_USER_T objuser);

        int AddResponsibility(Responsibility responsibility);
        Responsibility GetResponsibilityById(int respId);
        List<Responsibility> GetResponsibilities();

        bool InsertRole(Roles roles);
        List<ROLE_T> GetAllRoles();
        ROLE_T GetRoleById(int RoleId);
        bool UpdateRole(Roles roles);

        bool InsertReport(Report report);
        List<REPORT_T> GetAllReports();
        REPORT_T GetReportById(int RoleId);
        bool UpdateReport(Report report);
        bool InsertUserRole(UserRole userrole);
        List<USER_ROLE_T> GetAllUserRole();
        USER_ROLE_T GetUserRoleById(int RoleId);
        bool UpdateUserRole(UserRole userrole);
        List<USER_T> GetALlUsers();
        string UserName(int UserId);
        string RoleName(int RoleId);

        int AddScreen(Screen screen);
        Screen GetScreenById(int screenId);
        List<Screen> GetScreenss();

        int AddUserType(UserType usertype);
        UserType GetUserTypeById(int usertypeId);
        List<UserType> GetUserTypes();

        int AddRoleRespMapping(RoleResponsibilityMapping rolerespMapping);
        RoleResponsibilityMapping GetRoleResponsibilityById(int respId);
        List<RoleResponsibilityMapping> GetRoleResponsibilites();
        List<Responsibility> lstResponsibilities();
        List<Roles> lstRoles();
        List<RoleResponsibilityMapping> GetResponsibilityByRoleId(int roleId);
        List<Responsibility> GetResponsibilitiesByRole(int roleId);

        int AddUserRespMapping(UserRespMapping userrespMapping);
        int UpdateUserResp(int UserId);
        List<User> lstUsers();
        List<UserRespMapping> GetUserResponsibilites();
        List<UserRespMapping> GetResponsibilityByUserId(int userId);
        List<Responsibility> GetResponsibilitiesByUser(int userId);

        bool InsertScreenComponent(ScreenComponent screencomonent);
        List<SCREEN_COMP_T> GetAllScreenComponent();
        SCREEN_COMP_T GetScreenComponentById(int RoleId);
        bool UpdateScreenComponent(ScreenComponent screencomonent);
        bool InsertUser(User user);
        List<USER_T> GetAllUser();
        USER_T GetUserById(int UserId);
        bool UpdateUser(User user);
        string UserType(long UserTypeId);
        List<USER_TYPE_T> UserTypleList();
        List<UserRoleMapping> GetRoleByUserId(int userId);
        List<Roles> GetRolesByUser(int userId);
        List<UserRoleMapping> GetUserRoles();
        int AddUserRoleMapping(UserRoleMapping userroleMapping);
        int GetRoleOrder();

        bool InsertApplication(Application application);
        List<APPLICATION_T> GetAllApplications();
        APPLICATION_T GetApplicationById(int ApplicationId);
        bool UpdateApplication(Application application);

        bool InsertAppModule(AppModule appmodule);
        List<APP_MODULE_T> GetAllAppModules();
        APP_MODULE_T GetAppModuleById(int AppModuleId);
        bool UpdateAppModule(AppModule appmodule);

        bool InsertMenu(Menu menu);
        List<MENU_T> GetAllMenus();
        MENU_T GetMenuById(int MenuId);
        bool UpdateMenu(Menu menu);

        bool InsertWebApplication(WebApplication webapplication);
        List<WEB_APPLICATION_T> GetAllWebApplications();
        WEB_APPLICATION_T GetWebApplicationById(int WebApplicationId);
        bool UpdateWebApplication(WebApplication webapplication);
    }
}