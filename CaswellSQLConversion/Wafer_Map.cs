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
    
    public partial class Wafer_Map
    {
        public int Map_ID { get; set; }
        public Nullable<int> Pocket_ID { get; set; }
        public Nullable<double> Center_Val { get; set; }
        public Nullable<double> Average_Val { get; set; }
        public Nullable<double> SD { get; set; }
        public Nullable<double> Edge_Exclusion { get; set; }
        public string Spec_Band { get; set; }
        public Nullable<double> Area_in_Spec { get; set; }
        public string Comment { get; set; }
        public string Type { get; set; }
        public byte[] upsize_ts { get; set; }
    
        public virtual Pocket Pocket { get; set; }
    }
}