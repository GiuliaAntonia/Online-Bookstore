using System.ComponentModel.DataAnnotations.Schema;

namespace AplicatieMagazinOnline.Models
{
    public class Cart
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CartID { get; set; }
        public int? BookID { get; set; }
        public string? UserId { get; set; }
        public int Quantity { get; set; }
        public virtual Book? Book { get; set; }
        public virtual ApplicationUser? User { get; set; }

        //public virtual Profile? User { get; set; }
    }
}
