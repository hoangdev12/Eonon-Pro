//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebBTL.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class tblTinTuc
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tblTinTuc()
        {
            this.ContentBlocks = new HashSet<ContentBlock>();
        }
    
        public int PostID { get; set; }
        public string Title { get; set; }
        public string SContents { get; set; }
        public string Contents { get; set; }
        public string Thumb { get; set; }
        public bool Published { get; set; }
        public string Alias { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public string Author { get; set; }
        public Nullable<int> AccountID { get; set; }
        public string Tags { get; set; }
        public Nullable<int> CatID { get; set; }
        public Nullable<bool> isHot { get; set; }
        public Nullable<bool> isNewFeed { get; set; }
        public string MetaKey { get; set; }
        public string MetaDesc { get; set; }
        public Nullable<int> Views { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ContentBlock> ContentBlocks { get; set; }
    }
}
