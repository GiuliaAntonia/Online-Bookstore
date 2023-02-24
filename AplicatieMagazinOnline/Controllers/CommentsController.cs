using AplicatieMagazinOnline.Data;
using AplicatieMagazinOnline.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AplicatieMagazinOnline.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public CommentsController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;

        }

        // Stergerea unui comentariu asociat unei carti din baza de date
        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Delete(int id)
        {
            Comment comm = db.Comments.Find(id);

            // verif daca userul care vrea sa stearga un comm este cel care l-a postat
            // sau admin
            if(comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Comments.Remove(comm);
                db.SaveChanges();
                return Redirect("/Books/Show/" + comm.BookID);
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti comentariul";
                return RedirectToAction ("Index", "Books");
            }
            
        }

        // In acest moment vom implementa editarea intr-o pagina View separata
        // Se editeaza un comentariu existent
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Edit(int id)
        {
            Comment comm = db.Comments.Find(id);

            // verif daca userul care vrea sa editeze un comm e cel care l-a postat
            // sau admin
            if (comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(comm);
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa editati comentariul";
                return RedirectToAction("Index", "Books");
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Edit(int id, Comment requestComment)
        {
            Comment comm = db.Comments.Find(id);

            if (ModelState.IsValid)
            {
                if (comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                {
                    comm.Content = requestComment.Content;

                    db.SaveChanges();

                    return Redirect("/Books/Show/" + comm.BookID);
                }

                else
                {
                    TempData["message"] = "Nu aveti dreptul sa editati comentariul";
                    return RedirectToAction("Index", "Books");
                }

            }
            else
            {
                return View(requestComment);
            }

        }
    }
}
