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
    using System.Web;

    public partial class Book
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Book()
        {
            this.BorrowedDetails = new HashSet<BorrowedDetail>();
        }
    
        public int ID { get; set; }
        public string Title { get; set; }
        public string Thumbnail { get; set; }
        public float Price { get; set; }
        public int AvailableBook { get; set; }
        public string Description { get; set; }
        public int AuthorID { get; set; }
        public int CategoryID { get; set; }
    
//         public int id { get; set; }
//         [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a valid full name")]
//         public string title { get; set; }

//         public string thumbnail { get; set; }

//         //[Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a valid price")]
//         //[RegularExpression(@"^\$?(\d{1,3}(\,\d{3})*|(\d+))(\.\d{2})?$", ErrorMessage = "Please enter digit")]
//         public float price { get; set; }
//         //[Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a valid available book")]
//         //[RegularExpression(@"^[0-9]*$", ErrorMessage = "Please enter digit")]
//         public int available_book { get; set; }
//         //[Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a valid description")]
//         public string description { get; set; }
//         [Required]

//         public int author_id { get; set; }
//         [Required]

//         public int category_id { get; set; }

//         //[Required(ErrorMessage = "Please select file.")]
//         //[RegularExpression(@"([a-zA-Z0-9\s_\\.\-:])+(.png|.jpg|.gif)$", ErrorMessage = "Only Image files allowed")]
//         public HttpPostedFileBase ImageFile { get; set; }

//         [Required]
        public virtual Author Author { get; set; }
        [Required]
        public virtual BookCategory BookCategory { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BorrowedDetail> BorrowedDetails { get; set; }
    }
}
