
using System.ComponentModel.DataAnnotations;

namespace AplicatieMagazinOnline.Models
{
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }
        [Required(ErrorMessage = "Numele categoriei este obligatoriu")]
        public string CategoryName { get; set; }
        [Required(ErrorMessage = "Descrirea categoriei este obligatorie")]
        public string CategoryDescription { get; set; }
        public virtual ICollection<Book>? Books { get; set; }
    }
}
