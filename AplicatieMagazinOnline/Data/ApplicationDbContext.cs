
using Microsoft.EntityFrameworkCore;
using AplicatieMagazinOnline.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace AplicatieMagazinOnline.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        public DbSet<Book> Books { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Cart> Carts { get; set; }

        // public DbSet<User> Users { get; set; }
        //public DbSet<Order> Orders { get; set; }
        public DbSet<Comment> Comments { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            // definire primary key compus
            modelBuilder.Entity<Cart>()
                .HasKey(ab => new { ab.CartID, ab.BookID, ab.UserId });


            // definire relatii cu modelele Bookmark si Article (FK)
            modelBuilder.Entity<Cart>()
                .HasOne(ab => ab.Book)
                .WithMany(ab => ab.Carts)
                .HasForeignKey(ab => ab.BookID);

            modelBuilder.Entity<Cart>()
                .HasOne(ab => ab.User)
                .WithMany(ab => ab.Carts)
                .HasForeignKey(ab => ab.UserId);
        }


    }
}
