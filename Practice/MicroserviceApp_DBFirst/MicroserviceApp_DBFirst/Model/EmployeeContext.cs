using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroserviceApp_DBFirst.Model
{
    public class EmployeeContext : DbContext
    {
        public EmployeeContext(DbContextOptions<EmployeeContext> options) : base(options)
        {

        }
        public DbSet<tblSkill> tblSkills { get; set; }
        public DbSet<tblEmployee> tblEmployees { get; set; }
        public DbSet<Categories> Categories { get; set; }
    }
}
