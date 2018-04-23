using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class APICallHistoryHandler
    {
        APICallHistoryDBAccess apiHistoryDB = null;
        public APICallHistoryHandler()
        {
            apiHistoryDB = new APICallHistoryDBAccess();
        }
        public APICallHistory GetRunID()
        {
            return apiHistoryDB.GetRunID();
        }
        public bool AddNew(APICallHistory objCallHistory)
        {
            return apiHistoryDB.AddNew(objCallHistory);
        }
        public bool Update(APICallHistory objCallHistory)
        {
            return apiHistoryDB.Update(objCallHistory);
        }
    }
}
