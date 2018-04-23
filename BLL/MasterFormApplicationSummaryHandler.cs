using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL;
using System.Data;

namespace BLL
{
    public class MasterFormApplicationSummaryHandler 
    {
        
        MasterFormApplicationSummaryDBAccess summaryDB = null;
        public MasterFormApplicationSummaryHandler()
        {
            summaryDB = new MasterFormApplicationSummaryDBAccess();
        }
        public bool AddNew(MasterFormApplicationSummary objApplicationSummary)
        {
            return summaryDB.AddNew(objApplicationSummary);
        }
        public DataTable GetResultByRunID(long RunID)
        {
            return summaryDB.GetResultByRunID(RunID);
        }
    }
}
