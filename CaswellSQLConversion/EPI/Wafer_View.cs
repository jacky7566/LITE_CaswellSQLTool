using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaswellSQLConversion.EPI
{
    public class Wafer_View
    {
        public int Run_ID { get; set; }
        public int Pocket_ID { get; set; }
        //public string Reactor_Name { get; set; }
        //public int Descriptor { get; set; }
        public int? Pocket_Pos { get; set; }
        public string PROMIS_Lot_Name { get; set; }
        public string PROMIS_Component { get; set; }
        //public DateTime? Run_Date { get; set; }
        //public DateTime? Start_Time { get; set; }
        //public DateTime? End_Time { get; set; }
        //public string Purpose { get; set; }
        //public string Notes { get; set; }
        //public string Load_Lock { get; set; }
        public string Wafer_ID { get; set; }
        public string Wafer_Prep { get; set; }
        public double? Surfscan_Density { get; set; }
        public double? Surfscan_Count { get; set; }
        public string Release_Batch { get; set; }
        public string Substrate_Part_ID { get; set; }
        public string Part_Number { get; set; }
        public double? QWQB { get; set; }
        public double? ZOM { get; set; }
        public double? Center_Val { get; set; }
        public double? Average_Val { get; set; }
        public double? SD { get; set; }
        public double? Edge_Exclusion { get; set; }
        public string Spec_Band { get; set; }
        public double? Area_in_Spec { get; set; }
        public string Type { get; set; }
    }
}
