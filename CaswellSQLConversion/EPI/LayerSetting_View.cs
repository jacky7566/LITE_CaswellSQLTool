using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaswellSQLConversion.EPI
{
    public class LayerSetting_View
    {
        public int Run_ID { get; set; }
        public int Layer_ID { get; set; }
        public int? nIndex { get; set; }
        public double? Growth_Time { get; set; }
        public double? Growth_Temperature { get; set; }
        public string Material { get; set; }
        public string Reagent_Name { get; set; }
        public double? Source { get; set; }
        public double? Dilution { get; set; }
        public double? Pusher { get; set; }
        public double? Inject { get; set; }
        public double? Pressure { get; set; }
        public bool? EPISON_Control { get; set; }
    }
}
