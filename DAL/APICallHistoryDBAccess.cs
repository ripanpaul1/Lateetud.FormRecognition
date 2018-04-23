using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class APICallHistoryDBAccess
    {
        public APICallHistory GetRunID()
        {
            APICallHistory _apiCallHistory = null;

            using (DataTable table = SqlDBHelper.ExecuteSelectCommand("proc_APICallHistory_GetRunID", CommandType.StoredProcedure))
            {
                //check if any record exist or not
                if (table.Rows.Count == 1)
                {
                    DataRow row = table.Rows[0];

                    _apiCallHistory = new APICallHistory();
                    _apiCallHistory.RunID = Convert.ToInt32(row["RunID"]);

                }
            }

            return _apiCallHistory;
        }
        public bool AddNew(APICallHistory _apiHistory)
        {

            SqlParameter[] parameters = new SqlParameter[]
            {

           new SqlParameter("@RunID",_apiHistory.RunID),
           new SqlParameter("@Comment",_apiHistory.Comment),
           new SqlParameter("@Status",_apiHistory.Status),
           new SqlParameter("@EndTime",_apiHistory.EndTime),

           new SqlParameter("@StartTime",_apiHistory.StartTime),


            };
            return SqlDBHelper.ExecuteNonQuery("proc_APICallHistory_AddNew", CommandType.StoredProcedure, parameters);
        }

        public bool Update(APICallHistory _apiHistory)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
            new SqlParameter("@RunID", _apiHistory.RunID),
            new SqlParameter("@Status",_apiHistory.Status),

            new SqlParameter("@EndTime",_apiHistory.EndTime),
           
            };

            return SqlDBHelper.ExecuteNonQuery("proc_APICallHistory_Update", CommandType.StoredProcedure, parameters);
        }
    }
}
