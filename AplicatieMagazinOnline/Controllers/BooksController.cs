using AplicatieMagazinOnline.Data;
using AplicatieMagazinOnline.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;

namespace AplicatieMagazinOnline.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;
        private ApplicationDbContext _context;

        private IWebHostEnvironment _env;

        public BooksController(ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager, 
            RoleManager<IdentityRole> roleManager,
            IWebHostEnvironment env
            )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _env = env;

        }
        // Se afiseaza lista tuturor cartilor din baza de date impreuna cu categoria din care fac parte
        //HttpGet implict
        //[Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Index()
        {
            var books = db.Books.Include("Category").Include("User")
                                .OrderBy(a => a.PublishedDate);

            ViewBag.Books = books;

            // MOTOR DE CAUTARE
            var search = "";

            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                // eliminam spatiile libere 
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();

                // Cautare in articol (Title si Description)
                List<int> bookIds = db.Books.Where(
                                at => at.Title.Contains(search)
                                      || at.Description.Contains(search)
                                ).Select(a => a.BookID).ToList();

                // Cautare in comentarii
                List<int> bookIdsOfCommentsWithSearchString = db.Comments.Where(
                                            c => c.Content.Contains(search)
                                            ).Select(c => (int)c.BookID).ToList();

                // Se formeaza o singura lista formata din toate id-urile 
               //selectate anterior
               List<int> mergeIds = bookIds.Union(bookIdsOfCommentsWithSearchString).ToList();

                // Lista cartilorlor care contin cuvantul cautat
                // fie in carte -> Title si Content
                // fie in comentarii -> Content
                books = db.Books.Where(book => 
                         mergeIds.Contains(book.BookID))
                                    .Include("Category")
                                    .Include("User")
                                    .OrderBy(a => a.PublishedDate)
                                    .OrderBy(a => a.Price)
                                    .OrderBy(a => a.Rating); ;
            }

            ViewBag.SearchString = search;

            // AFISARE PAGINATA

            // Vrem sa avem cate 3 carti pe pg
            int _perPage = 3;
            
            if(TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            // Fiind un numar variabil de carti, verificam de fiecare data utilizand
            // metoda Count()

            int totalItems = books.Count();

            // Se preia pagina curenta din View-ul asociat
            // Numarul paginii este valoarea parametrului page din ruta
            // /Articles/Index?page=valoare

            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            // Pentru prima pagina offsetul o sa fie zero
            // Pentru pagina 2 o sa fie 3 
            // Asadar offsetul este egal cu numarul de articole 
            // care au fost deja afisate pe paginile anterioare
            var offset = 0;

            // Se calculeaza offsetul in functie de numarul 
            // paginii la care suntem
            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            // Se preiau cartile corespunzatoare pentru fiecare pagina la care ne aflam
            // in functie de offset
            var paginatedBooks = books.Skip(offset).Take(_perPage);

            // Preluam numarul ultimei pagini

            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);

            // Trimitem cartile cu ajutorul unui ViewBag catre View-ul corespunzator
            ViewBag.Books = paginatedBooks;


            // CONTINUARE MOTOR CAUTARE

            if (search != "")
            {
                ViewBag.PaginationBaseUrl = "/Books/Index/?search=" + search + "&page";
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Articles/Index/?page";
            }

            return View();

            //if (TempData.ContainsKey("message"))
            //{
            //    ViewBag.Message = TempData["message"];
            //}
            return View();
        }

            // Se afiseaza o singura carte in functie de id-ul sau 
            // impreuna cu categoria din care face parte
            // In plus sunt preluate si toate comentariile asociate unei carti
            // HttpGet implicit
            //[Authorize(Roles = "User,Editor,Admin")]
            public ActionResult Show(int id)
        {
            Book book = db.Books.Include("Category")
                                .Include("User")
                                .Include("Comments")
                                .Include("Comments.User")
                                .Where(bok => bok.BookID == id)
                                .First();

            SetAccessRights();

          /*  ViewBag.Book = book;
            ViewBag.category = book.Category;*/
            // ViewBag.Category(ViewBag.UnNume) = article.Category (proprietatea Category);
            return View(book);
        }

        // butoanele editare/stergere sunt vizibile doar adminului si editorului
        // care le-a adaugat
        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            if (User.IsInRole("Editor"))
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.UserCurent = _userManager.GetUserId(User);

            ViewBag.EsteAdmin = User.IsInRole("Admin");

            ViewBag.EsteEditor = User.IsInRole("Editor");

            ViewBag.EsteUser = User.IsInRole("User");

        }


        // Adaugarea unui comentariu asociat unei carti in baza de date
        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Show([FromForm] Comment comment)
        {
            comment.Date = DateTime.Now;
            comment.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();
                return Redirect("/Books/Show/" + comment.BookID);
            }

            else
            {
                Book bok = db.Books.Include("Category")
                                   .Include("User")
                                   .Include("Comments")
                                   .Include("Comments.User")
                                   .Where(bok => bok.BookID == comment.BookID)
                                   .First();

                SetAccessRights();

                //return Redirect("/Articles/Show/" + comm.ArticleId);

                return View(bok);
            }
        }


        // Se afiseaza formularul in care se vor completa datele unei carti
        // impreuna cu selectarea categoriei din care face parte
        // HttpGet implicit
        [Authorize(Roles ="Editor,Admin")]
        public IActionResult New()
        {
            Book book = new Book();

            book.Categ = GetAllCategories();

            // editorul trimite cereri adminului pt adaugare
            book.UserId = _userManager.GetUserId(User);
            book.Approved = User.IsInRole("Admin");

            return View(book);

        }

        // Se adauga cartea  in baza de date
        [HttpPost]
        [Authorize(Roles = "Editor,Admin")]
        public IActionResult New(Book book)
        {
           // book.Date = DateTime.Now;
            book.Categ = GetAllCategories();
            book.UserId = _userManager.GetUserId(User);
            book.Approved = User.IsInRole("Admin");

            if (ModelState.IsValid)
            {
                db.Books.Add(book);
                db.SaveChanges();
                if(User.IsInRole("Admin"))
                {
                    TempData["message"] = "Cartea a fost adaugata";
                }
                else
                {
                    TempData["message"] = "Cartea asteapt aprobarea adminului";
                }
                
                return RedirectToAction("Index");
            }
            else
            {
                return View(book);
            }

        }

        // Se afiseaza cartile care asteapta aprobarea adminului
        // Adminul aproba produsele editorului
        [Authorize(Roles ="Admin")]
        public IActionResult Approve()
        {
            var books = db.Books.Include("Category")
                                .Include("User");
            ViewBag.Books = books;

            if(TempData.ContainsKey("message"))
                ViewBag.Message = TempData["message"];

            return View();
        }

        // Se gaseste cartea in baza de date si se seteaza true la approved
        [HttpPost]
        [Authorize(Roles ="Admin")]
        public IActionResult Approve(int id)
        {
            Book book = db.Books.Find(id);
            book.Approved = true;

            if (ModelState.IsValid)
            {
                //db.Books.Add(book);
                db.SaveChanges();
                TempData["message"] = "Cartea a fost adaugata";
                return RedirectToAction("Index");
            }

            return View();
        }

        // Se editeaza o carte existenta in baza de date impreuna cu categoria din care face parte
        // Categoria se selecteaza dintr-un dropdown
        // HttpGet implicit
        // Se afiseaza formularul impreuna cu datele aferente articolului din baza de date
        [Authorize(Roles = "Editor,Admin")]
        public IActionResult Edit(int id)
        {

            Book book = db.Books.Include("Category")
                                        .Where(bok => bok.BookID == id)
                                        .First();

            book.Categ = GetAllCategories();

            // verificam daca userul care vrea sa editeze e editorul care a postat
            // sau e admin
            if(book.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(book);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                return RedirectToAction("Index");
            }

        }
        // Se adauga cartea modificata in baza de date
        [Authorize(Roles = "Editor,Admin")]
        [HttpPost]
        public IActionResult Edit(int id, Book requestBook)
        {
            Book book = db.Books.Find(id);
            requestBook.Categ = GetAllCategories();

            if (ModelState.IsValid)
            {
                if (book.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                {
                    book.Title = requestBook.Title;
                    book.Image = requestBook.Image;
                    book.Price = requestBook.Price;
                    book.Description = requestBook.Description;
                    book.NoPages = requestBook.NoPages;
                    book.PublishedDate = requestBook.PublishedDate;
                    book.Author = requestBook.Author;
                    book.Rating = requestBook.Rating;
                    TempData["message"] = "Cartea a fost modificata";
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                return View(requestBook);
            }
        }

        // Se sterge o carte din baza de date
        [Authorize(Roles = "Editor,Admin")]
        [HttpPost]
        public ActionResult Delete(int id)
        {
            Book book = db.Books.Include("Comments")
                                .Where(bok => bok.BookID == id)
                                .First();

            if (book.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Books.Remove(book);
                db.SaveChanges();
                TempData["message"] = "Cartea  a fost stearsa";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti o carte care nu va apartine";
                return RedirectToAction("Index");
            }
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            // generam o lista de tipul SelectListItem fara elemente
            var selectList = new List<SelectListItem>();

            // extragem toate categoriile din baza de date
            var categories = from cat in db.Categories
                             select cat;

            // iteram prin categorii
            foreach (var category in categories)
            {
                // adaugam in lista elementele necesare pentru dropdown
                // id-ul categoriei si denumirea acesteia
                selectList.Add(new SelectListItem
                {
                    Value = category.CategoryID.ToString(),
                    Text = category.CategoryName.ToString()
     
                });
            }
            /* Sau se poate implementa astfel: 
             * 
            foreach (var category in categories)
            {
                var listItem = new SelectListItem();
                listItem.Value = category.Id.ToString();
                listItem.Text = category.CategoryName.ToString();

                selectList.Add(listItem);
             }*/


            // returnam lista de categorii
            return selectList;
        }

        public IActionResult IndexNou()
        {
            return View();
        }
        //Imagini
        public IActionResult UploadImage()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(int id, IFormFile BookImage)
        {
            var databaseFileName = "";

            if (BookImage.Length > 0)
            {
                // Generam calea de stocare a fisierului
                var storagePath = Path.Combine(
                _env.WebRootPath, // Luam calea folderului wwwroot
                "images", // Adaugam calea folderului images
                BookImage.FileName // Numele fisierului
                );

                databaseFileName = "/images/" + BookImage.FileName;
                // Uploadam fisierul la calea de storage
                using (var fileStream = new FileStream(storagePath,
               FileMode.Create))
                {
                    await BookImage.CopyToAsync(fileStream);
                }
            }

            //Salvam storagePath-ul in baza de date
            Book book = db.Books.Find(id);
            book.Image = databaseFileName;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

    }
}
