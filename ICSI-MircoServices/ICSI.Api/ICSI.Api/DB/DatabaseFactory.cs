using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace ICSI.Api.DB
{
    public sealed class DatabaseFactory
    {
        private static int dbCount = 0;
        private static string defaultSchema = "";
        private static string defaultEnv = "";
        private static Dictionary<string, Database> databases = new Dictionary<string, Database>();
        private static IConfiguration _configuration;
        public static void setConfiguration(IConfiguration config) {
            _configuration = config;            
            initialize();
        }
        public static Dictionary<string, Database> GetDatabases() { return databases; }

        private static void initialize()
        {
            if (_configuration == null)
            {
                throw new Exception("Database Configuration failed from appsettings.json.");
            }            
            JObject o = (JObject)SerializeDBInfo(_configuration,false);
            if (o==null || !o.ContainsKey("DBInfo")) {
                throw new Exception("DBInfo not defined in Configuration section of appsettings.json.");
            }
            JObject r = (JObject)o.GetValue("DBInfo");
            string passwordType = _configuration.GetValue<string>("DBInfo:PasswordType");
            if(passwordType==null || passwordType.Length == 0)
            {
                ICSI.Api.Controllers.ICSIStimulateController.PASSWORD_TYPE = 1;
            }
            else
            {
                try
                {
                    ICSI.Api.Controllers.ICSIStimulateController.PASSWORD_TYPE = Convert.ToInt32(passwordType);
                }
                catch(Exception ex)
                {
                    ICSI.Api.Controllers.ICSIStimulateController.PASSWORD_TYPE = 1;
                }
            }
            string dbCountStr = _configuration.GetValue<string>("DBInfo:DBCount");
            // Verify a DatabaseFactoryConfiguration line exists in the web.config.
            if (dbCountStr==null || dbCountStr.Length == 0)
            {
                throw new Exception("Database count not defined in Configuration section of appsettings.json.");
            }
            dbCount = Convert.ToInt32(dbCountStr);
            defaultEnv = _configuration.GetValue<string>("DBInfo:DBEnv");
            if (defaultEnv==null || defaultEnv.Length == 0)
            {
                throw new Exception("Database Default Environment variable not defined in Configuration section of appsettings.json.");
            }
            defaultSchema = _configuration.GetValue<string>("DBInfo:DefaultSchema");
            if (defaultSchema==null || defaultSchema.Length == 0)
            {
                throw new Exception("Database Default Schema not defined in Configuration section of appsettings.json.");
            }
            /*string str = _configuration.GetValue<string>("DBInfo:"+ defaultEnv);
            if (str.Length == 0)
            {
                throw new Exception("Database Default Environment configuration not defined in Configuration section of appsettings.json.");
            }

            str = _configuration.GetValue<string>("DBInfo:" + defaultEnv+":"+ defaultSchema);
            if (str.Length == 0)
            {
                throw new Exception("Database Default Schema configuration not defined in Configuration section of appsettings.json.");
            }*/
            //databases.Add("DEFAULT", createInistance("DBInfo:" + defaultEnv + ":" + defaultSchema));
            var r1 = r.GetValue(defaultEnv).Value<JObject>();
            List<string> l1 = r1.Properties().Select(p => p.Name).ToList();
            //Dictionary<string, Dictionary<string, string>> d2 = new Dictionary<string, Dictionary<string, string>>();
            
            foreach (string child in l1)
            {
                JObject r2 = (JObject)r1.GetValue(child);
                //Dictionary<string, string> d3 = new Dictionary<string, string>();
                List<JProperty> l2 = r2.Properties().ToList();
                foreach(JProperty k in l2)
                {
                    if (k.Name.Equals("ConnectionString"))
                    {
                        DB.Database database = createInistance(child, k.Value.ToString());
                        databases.Add(child, database);
                        if (child.Equals(defaultSchema)) databases.Add("DEFAULT", database);
                    }
                }
            }
            /*Dictionary<string, Dictionary<string, string>> d = _configuration.GetSection("DBInfo:"+defaultEnv).Get<Dictionary<string, Dictionary<string, string>>>();
            if(d==null || d.Count == 0)
            {
                throw new Exception("Database Schema configuration not defined in Configuration section of appsettings.json.");
            }
            foreach(KeyValuePair<string,Dictionary<string, string>> d1 in d2)
            {
                Dictionary<string, string> c = d1.Value;
                if(c.ContainsKey("ConnectionString") && c["ConnectionString"]!=null && !c["ConnectionString"].Trim().Equals(""))
                {
                    Database database = createInistance("DBInfo:" + defaultEnv + ":" + d1.Key);
                    if (d1.Key.Equals(defaultSchema)) databases.Add("DEFAULT", database);
                    databases.Add(d1.Key, database);
                }
            }*/
        }

        private static JToken SerializeDBInfo(IConfiguration config,bool isRootSet)
        {
            JObject obj = new JObject();
            foreach (var child in config.GetChildren())
            {
                if (!isRootSet && child.Key.StartsWith("DBInfo")) isRootSet = true;
                if(isRootSet)obj.Add(child.Key, SerializeDBInfo(child, isRootSet));
            }
            if (isRootSet && !obj.HasValues && config is IConfigurationSection section)
            {
                //obj.Add(section.Key, section.Value.ToString());
                //var s = new JValue(section.Value);     
                return new JValue(section.Value);
            }
            return obj;
        }

        private static Database createInistance(string envschema, string ConnectionStr)
        {
            try
            {
                // Find the class
                Type database = Type.GetType("ICSI.Api.DB.ICSIStimulate");

                // Get it's constructor
                ConstructorInfo constructor = database.GetConstructor(new Type[] { });

                // Invoke it's constructor, which returns an instance.
                Database createdObject = (Database)constructor.Invoke(null);
                /*string str = _configuration.GetValue<string>(envschema+ ":ConnectionString");
                if (str.Length == 0)
                {
                    throw new Exception("Database ("+ envschema + ") configuration not defined in Configuration section of appsettings.json.");
                }*/
                // Initialize the connection string property for the database.
                createdObject.connectionString = ConnectionStr;
                // Pass back the instance as a Database
                return createdObject;
            }
            catch (Exception excep)
            {
                throw new Exception("Error instantiating database " + envschema + ". " + excep.Message);
            }
        }
        private DatabaseFactory()
        {
            
        }

        public static Database GetDatabase()
        {
            return GetDatabase("DEFAULT");
        }

        public static Database GetDatabase(string schema)
        {
            if (databases.ContainsKey("DEFAULT")) return databases["DEFAULT"];
            else return null;
        }
    }
}
