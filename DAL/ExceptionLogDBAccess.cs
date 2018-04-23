using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class ExceptionLogDBAccess
    {
        public bool AddNew(ExceptionLog _exceptionLog)
        {

            SqlParameter[] parameters = new SqlParameter[]
            {

           new SqlParameter("@ErrorTime",_exceptionLog.ErrorTime),
           new SqlParameter("@ErrorNumber",_exceptionLog.ErrorNumber),
           new SqlParameter("@ErrorMessage",_exceptionLog.ErrorMessage),
           new SqlParameter("@Comments",_exceptionLog.Comments),


            };
            return SqlDBHelper.ExecuteNonQuery("proc_ExceptionLog_AddNew", CommandType.StoredProcedure, parameters);
        }
    }
}
