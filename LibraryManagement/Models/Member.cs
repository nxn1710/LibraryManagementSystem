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
    using System.ComponentModel.DataAnnotations;

    public partial class Member
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Member()
        {
            this.Borroweds = new HashSet<Borrowed>();
        }
    
        public int ID { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a valid full name")]
        [StringLength(64, MinimumLength = 3, ErrorMessage = "Please length of name must be from 3 to 64 characters")]
        public string FullName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a valid phone")]
        [RegularExpression(@"(84|0[1|2|3|4|5|6|7|8|9])+([0-9]{8})", ErrorMessage = "Please length of phone no must be 10 numbers")]
        public string PhoneNumber { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a valid address")]
        [StringLength(128, ErrorMessage = "Only enter 128 characters")]
        public string Address { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Borrowed> Borroweds { get; set; }
    }
}
