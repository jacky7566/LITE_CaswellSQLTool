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
    
    public partial class Layer
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Layer()
        {
            this.Layer_Result = new HashSet<Layer_Result>();
            this.Reagent_State = new HashSet<Reagent_State>();
        }
    
        public int Layer_ID { get; set; }
        public Nullable<int> Run_ID { get; set; }
        public Nullable<int> nIndex { get; set; }
        public string Material { get; set; }
        public Nullable<double> Growth_Time { get; set; }
        public string Res_1 { get; set; }
        public string Res_2 { get; set; }
        public Nullable<double> Growth_Temperature { get; set; }
        public byte[] upsize_ts { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Layer_Result> Layer_Result { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Reagent_State> Reagent_State { get; set; }
    }
}
