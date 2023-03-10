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
    
    public partial class Reagent_State
    {
        public int State_ID { get; set; }
        public Nullable<int> Layer_ID { get; set; }
        public Nullable<int> Reagent_ID { get; set; }
        public Nullable<double> Source { get; set; }
        public Nullable<double> Dilution { get; set; }
        public Nullable<double> Pusher { get; set; }
        public Nullable<double> Inject { get; set; }
        public Nullable<double> Pressure { get; set; }
        public Nullable<bool> EPISON_Control { get; set; }
        public Nullable<double> Bulk_SetConc { get; set; }
    
        public virtual Layer Layer { get; set; }
        public virtual Reagent Reagent { get; set; }
    }
}
