using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace ICSI.PG.DB
{
    public sealed class DatabaseFactory
    {
        private static int dbCount = 0;
        private static string defaultSchema = "";
        private static string defaultEnv = "";
        private static Dictionary<string, Database> databases = new Dictionary<string, Database>();
        public static void setConfiguration() {
            try
            {
                if (databases == null || databases.Count == 0) initialize();
            }catch(Exception ex) { }
        }
        public static Dictionary<string, Database> GetDatabases() { return databases; }

        private static void initialize()
        {
            dbCount = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["DBCount"]);
            defaultSchema= System.Configuration.ConfigurationManager.AppSettings["DEFAULT_SCHEMA"];
            for(var i = 1; i <= dbCount; i++)
            {
                string dbConnStr = System.Configuration.ConfigurationManager.ConnectionStrings["DB" + i].ConnectionString;
                string child = System.Configuration.ConfigurationManager.AppSettings["DB" + i];
                DB.Database database = createInistance(child, dbConnStr);
                databases.Add(child, database);
                if (defaultSchema.Equals(child)) databases.Add("DEFAULT", database);
            }
        }

        /*private static JToken SerializeDBInfo(IConfiguration config,bool isRootSet)
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
        }*/

        private static Database createInistance(string envschema, string ConnectionStr)
        {
            try
            {
                // Find the class
                Type database = Type.GetType("ICSI.PG.DB.ICSIStimulate");

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
            if (databases.ContainsKey(schema)) return databases[schema];
            else return null;
        }
    }
}
