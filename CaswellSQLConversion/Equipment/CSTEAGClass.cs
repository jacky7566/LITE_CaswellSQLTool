using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaswellSQLConversion.Equipment
{
    class CSTEAGClass
    {
        public string SerialNumber { get; set; }
        public string Recipe { get; set; }
        public DateTime StartDateTime { get; set; }
        public string Operator { get; set; }
        public Dictionary<string, List<double>> CSVDicList { get; set; }
    }
}
