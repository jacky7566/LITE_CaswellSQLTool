using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaswellSQLConversion.EPI
{
    class LayerResult_View
    {
        public int Run_ID { get; set; }
        public int Layer_ID { get; set; }
        public int nIndex { get; set; }
        public string Material { get; set; }
        public double? Value { get; set; }
        public string Units { get; set; }
        public string Type { get; set; }
        public string Technique { get; set; }
        public bool? Exclude { get; set; }
        public string Comment { get; set; }
    }
}
