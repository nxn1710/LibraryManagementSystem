//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LibraryManagement.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Book
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Book()
        {
            this.BorrowedDetails = new HashSet<BorrowedDetail>();
        }
    
        public int id { get; set; }
        public string title { get; set; }
        public int author_id { get; set; }
        public float price { get; set; }
        public string description { get; set; }
        public int category_id { get; set; }
        public string thumbnail { get; set; }
        public int available_book { get; set; }
    
        public virtual Author Author { get; set; }
        public virtual BookCategory BookCategory { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BorrowedDetail> BorrowedDetails { get; set; }
    }
}
