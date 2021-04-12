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
    
    public partial class PROMIS_Part
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PROMIS_Part()
        {
            this.Pockets = new HashSet<Pocket>();
        }
    
        public int Part_ID { get; set; }
        public string Part_Number { get; set; }
        public string Part_Name { get; set; }
        public string Part_PROMIS_Batch_Prefix { get; set; }
        public Nullable<bool> Part_Prod { get; set; }
        public Nullable<bool> Part_Current { get; set; }
        public string Template_Path { get; set; }
        public string Material { get; set; }
        public byte[] upsize_ts { get; set; }
        public string Material_Options { get; set; }
        public string Unidentified_Part_Number { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Pocket> Pockets { get; set; }
    }
}
