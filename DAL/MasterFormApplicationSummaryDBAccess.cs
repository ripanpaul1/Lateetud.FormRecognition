using DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class MasterFormApplicationSummaryDBAccess
    {
        public bool AddNew(MasterFormApplicationSummary _applicationSummary)
        {

            SqlParameter[] parameters = new SqlParameter[]
            {

           new SqlParameter("@RunID",_applicationSummary.RunID),
           new SqlParameter("@FileName",_applicationSummary.FileName),
           new SqlParameter("@FieldKey",_applicationSummary.FieldKey),
           new SqlParameter("@FieldValue",_applicationSummary.FieldValue),

           new SqlParameter("@EntryDate",_applicationSummary.EntryDate),


            };
            return SqlDBHelper.ExecuteNonQuery("proc_MasterFormApplicationSummary_AddNew", CommandType.StoredProcedure, parameters);
        }

        public DataTable GetResultByRunID(long RunID)
        {
            SqlParameter[] parameters = new SqlParameter[]
           {
            new SqlParameter("@RunID", RunID)
           };


            DataTable table = SqlDBHelper.ExecuteParamerizedSelectCommand("proc_ApplicationSummary_GetResultByRunID", CommandType.StoredProcedure, parameters);


            return table;

        }

    }
}
