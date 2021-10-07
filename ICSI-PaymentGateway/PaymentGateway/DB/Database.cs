using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;


namespace ICSI.PG.DB
{
    public abstract class Database
    {
        public string connectionString;

        #region Abstract Functions

        public abstract IDbConnection CreateConnection();
        public abstract IDbCommand CreateCommand();
        public abstract IDbConnection CreateOpenConnection();
        public abstract IDbCommand CreateCommand(string commandText, IDbConnection connection);
        public abstract IDbCommand CreateStoredProcCommand(string procName, IDbConnection connection);
        public abstract IDataParameter CreateParameter(string parameterName, object parameterValue);
        public abstract void closeObjects(IDbConnection connection, IDbCommand command, IDataReader reader);
        #endregion
    }
}
