using AplicatieMagazinOnline.Models;
using System.ComponentModel.DataAnnotations;

namespace AplicatieMagazinOnline.Models
{
    public class Comment
    {

        [Key]
        public int CommentID { get; set; }
        [Required(ErrorMessage = "Continutul este obligatoriu")]
        public string Content { get; set; }
        public DateTime Date { get; set; }
        public int? BookID { get; set; }
        public string? UserId { get; set; }
       
        public virtual ApplicationUser? User { get; set; }
        public virtual Book? Book { get; set; }


    }
}
