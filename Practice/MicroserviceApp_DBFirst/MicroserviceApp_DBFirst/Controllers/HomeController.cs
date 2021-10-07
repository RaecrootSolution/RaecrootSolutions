using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MicroserviceApp_DBFirst.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MicroserviceApp_DBFirst.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly EmployeeContext _dbContext;
        public HomeController(EmployeeContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            //var _emplst = _dbContext.tblEmployees.
            //                Join(_dbContext.tblSkills, e => e.SkillID, s => s.SkillID,
            //                (e, s) => new EmployeeViewModel
            //                {
            //                    EmployeeID = e.EmployeeID,
            //                    EmployeeName = e.EmployeeName,
            //                    PhoneNumber = e.PhoneNumber,
            //                    Skill = s.Title,
            //                    YearsExperience = e.YearsExperience
            //                }).ToList();

            var Categories = _dbContext.Categories.Select(x => new { x.Id, x.Name, x.Description }).ToList();
                

            //IList<EmployeeViewModel> emplst = _emplst;
            //return emplst.AsEnumerable();
            return new string[] { "value1", "value2" };
        }
    }
}