using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class ExceptionLog
    {
        public long ID { get; set; }
        public Nullable<System.DateTime> ErrorTime { get; set; }
        public string ErrorNumber { get; set; }
        public string ErrorMessage { get; set; }
        public string Comments { get; set; }
    }
}
