using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaswellSQLConversion.EPI
{
    public class Run_View
    {
        public int Run_ID { get; set; }
        public Nullable<int> Reactor_ID { get; set; }
        public Nullable<int> Descriptor { get; set; }
        public Nullable<System.DateTime> Run_Date { get; set; }
        public Nullable<System.DateTime> Start_Time { get; set; }
        public Nullable<System.DateTime> End_Time { get; set; }
        public Nullable<double> Growth_Temp { get; set; }
        public Nullable<double> Growth_Press { get; set; }
        public string Purpose { get; set; }
        public string Res_1 { get; set; }
        public string Res_2 { get; set; }
        public Nullable<double> Res_3 { get; set; }
        public string Notes { get; set; }
        public string Category { get; set; }
        public string Wafer_Size { get; set; }
        public string Collector_Purge { get; set; }
        public string AsH3_source { get; set; }
        public string Load_Lock { get; set; }
        public Nullable<int> Platten_Pos { get; set; }
        public Nullable<double> Arsine_Cons { get; set; }
        public Nullable<double> Phosphine_Cons { get; set; }
        public string BatchID { get; set; }
        public virtual Reactor Reactor { get; set; }
        public Nullable<System.DateTime> TDS_UploadDate { get; set; }
    }
}
