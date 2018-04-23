using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class MasterFormApplicationSummary
    {
        public long ID { get; set; }
        public Nullable<long> RunID { get; set; }
        public string FileName { get; set; }
        public string FieldKey { get; set; }
        public string FieldValue { get; set; }
        public Nullable<System.DateTime> EntryDate { get; set; }
    }
}
