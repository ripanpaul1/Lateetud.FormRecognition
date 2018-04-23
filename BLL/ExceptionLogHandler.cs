using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class ExceptionLogHandler
    {
        ExceptionLogDBAccess exceptionLogDB = null;
        public ExceptionLogHandler()
        {
            exceptionLogDB = new ExceptionLogDBAccess();
        }
        public bool AddNew(ExceptionLog objExceptionLog)
        {
            return exceptionLogDB.AddNew(objExceptionLog);

        }
    }
}
