using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace AplicatieMagazinOnline.Models

{
    public class Book
    {
        [Key]
        public int BookID { get; set; }
        [Required(ErrorMessage = "Titlul este obligatoriu")]
        [StringLength(100, ErrorMessage = "Titlul nu poate avea mai mult de 100 de caractere")]
        [MinLength(5, ErrorMessage = "Titlul trebuie sa aiba mai mult de 5 caractere")]
        public string Title { get; set; }
        public string? Image { get; set; }
        [Required(ErrorMessage = "Pretul este obligatoriu")]
        public int Price { get; set; }
        [Required(ErrorMessage = "Descrierea este obligatorie")]
        [StringLength(1000, ErrorMessage = "Descrierea nu poate avea mai mult de 1000 de caractere")]
        [MinLength(5, ErrorMessage = "Descrierea trebuie sa aiba mai mult de 5 caractere")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Numarul de pagini este obligatoriu")]
        public int NoPages { get; set; }
        [Required(ErrorMessage = "Data publicarii  este obligatorie")]
        public DateTime PublishedDate { get; set; }
        [Required(ErrorMessage = "Autorul este obligatoriu")]
        [StringLength(100, ErrorMessage = "Numele autorului nu poate avea mai mult de 100 de caractere")]
        [MinLength(5, ErrorMessage = "Numele autorului trebuie sa aiba mai mult de 5 caractere")]
        public string Author { get; set; }
        [Required(ErrorMessage = "Ratingul este obligatoriu")]
        [Range(1,5)]
        public int Rating { get; set; }
        [Required(ErrorMessage = "Categoria este obligatorie")]
        public int? CategoryID { get; set; }

        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public virtual Category? Category { get; set; }
        
        public virtual ICollection<Comment>? Comments { get; set; }
        public bool Approved { get; set; }
        [NotMapped]
        public IEnumerable<SelectListItem>? Categ { get; set; }

        //DE REVENIT AICI
        public virtual ICollection<Cart>? Carts { get; set; }



    }
}
