//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CaswellSQLConversion
{
    using System;
    using System.Collections.Generic;
    
    public partial class Run
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
        public Nullable<int> Batch_Size { get; set; }
        public Nullable<int> Run_in_Batch { get; set; }
        public string Load_Lock { get; set; }
        public Nullable<int> Platten_Pos { get; set; }
        public Nullable<double> Target_Depl_Depth { get; set; }
        public Nullable<double> Arsine_Cons { get; set; }
        public Nullable<double> Phosphine_Cons { get; set; }
        public string BatchID { get; set; }
        public byte[] upsize_ts { get; set; }
        public Nullable<bool> TDS_Upload { get; set; }
        public Nullable<System.DateTime> TDS_UploadDate { get; set; }
    
        public virtual Reactor Reactor { get; set; }
    }
}
