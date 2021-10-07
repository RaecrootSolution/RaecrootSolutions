using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ICSI.PG.DB
{
    public class ICSIStimulate:Database
    {
        public ICSIStimulate() { }
        public override IDbConnection CreateConnection()
        {
            return new SqlConnection(connectionString);
        }

        public override IDbCommand CreateCommand()
        {
            return new SqlCommand();
        }

        public override IDbConnection CreateOpenConnection()
        {
            SqlConnection connection = (SqlConnection)CreateConnection();
            connection.Open();
            return connection;
        }

        public override IDbCommand CreateCommand(string commandText, IDbConnection connection)
        {
            SqlCommand command = (SqlCommand)CreateCommand();
            command.CommandText = commandText;
            command.Connection = (SqlConnection)connection;
            command.CommandType = CommandType.Text;
            return command;
        }

        public override IDbCommand CreateStoredProcCommand(string procName, IDbConnection connection)
        {
            SqlCommand command = (SqlCommand)CreateCommand();
            command.CommandText = procName;
            command.Connection = (SqlConnection)connection;
            command.CommandType = CommandType.StoredProcedure;
            return command;
        }

        public override IDataParameter CreateParameter(string parameterName, object parameterValue)
        {
            return new SqlParameter(parameterName, parameterValue);
        }

        public override void closeObjects(IDbConnection connection, IDbCommand command, IDataReader reader)
        {
            try
            {
                if (reader != null && !reader.IsClosed) { reader.Close(); reader.Dispose(); }
            }
            catch { }

            try
            {
                if (command != null) { command.Dispose(); }
            }
            catch { }

            try
            {
                if (connection != null && connection.State != ConnectionState.Closed) { connection.Close(); connection.Dispose(); }
            }
            catch { }
        }
    }
}
