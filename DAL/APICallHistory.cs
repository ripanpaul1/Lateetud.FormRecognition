using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class APICallHistory
    {
        public long ID { get; set; }
        public string Comment { get; set; }
        public string Status { get; set; }
        public Nullable<long> RunID { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public Nullable<System.DateTime> StartTime { get; set; }
    }
}
