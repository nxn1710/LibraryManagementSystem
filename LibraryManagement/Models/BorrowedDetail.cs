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
    
    public partial class BorrowedDetail
    {
        public int id { get; set; }
        public int book_id { get; set; }
        public int borrow_id { get; set; }
    
        public virtual Book Book { get; set; }
        public virtual Borrowed Borrowed { get; set; }
    }
}