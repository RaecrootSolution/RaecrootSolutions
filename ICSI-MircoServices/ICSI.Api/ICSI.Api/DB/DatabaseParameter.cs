using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;

namespace ICSI.Api.DB
{
    public class DatabaseParameter
    {
        private String _parameterName;
        private Object _parameterValue;
        private DbType _parameterDataType;
        private ParameterDirection _parameterDirection;
        private IDbDataParameter _dataParameter;

        public DatabaseParameter(String parameterName, ParameterDirection parameterDirection, Object parameterValue, IDbDataParameter dataParameter)
        {
            this._dataParameter = dataParameter;
            this._parameterName = parameterName;
            this._parameterDirection = parameterDirection;
            this._parameterValue = parameterValue;
        }

        public DatabaseParameter(String parameterName, Object parameterValue, DbType parameterDataType, ParameterDirection parameterDirection)
        {
            this._parameterName = parameterName;
            this._parameterValue = parameterValue;
            this._parameterDataType = parameterDataType;
            this._parameterDirection = parameterDirection;
        }
        public DatabaseParameter(String parameterName, Object parameterValue, DbType parameterDataType)
            : this(parameterName, parameterValue, parameterDataType, ParameterDirection.Input)
        {

        }

        public DatabaseParameter(String parameterName, Object parameterValue, ParameterDirection direction)
            : this(parameterName, parameterValue, DbType.String, direction)
        {

        }

        public DatabaseParameter(String parameterName, String parameterValue)
            : this(parameterName, parameterValue, DbType.String, ParameterDirection.Input)
        {

        }
        public String GetParameterName() { return this._parameterName; }
        public Object GetParameterValue() { return this._parameterValue; }
        public DbType GetParameterDataType() { return this._parameterDataType; }
        public ParameterDirection GetParameterDirection() { return this._parameterDirection; }
        public IDbDataParameter GeteDataParameter() { return this._dataParameter; }
    }
}
