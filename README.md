# 📚 Online-Bookstore
Web application using ASP.NET Core MVC that features:
* role-based authentication system with four types of user: admin, collaborator, registered user, unregistered user
* the collaborator can add books in the store by sending additional requests to the admin who can aprrove or reject them. After the aproval the books can be viewed in the store
* books are part of categories (dynamically created). The admin is the only one who can create, read, update, delete on categories
* a book has a title, picture, price, rating and reviews from users
* the collaborator can only edit and delete the books he added
* the unregistered user will be directed to create an account the moment he tries to add a book in the shopping cart. He can only view the books and its comments
* a registered user can place orders, review the books and also can edit or delete the reviews later on
* books can be searched by title using a searching engine and can also be sorted ascending and descending by price and rating score 
* the admin can delete and edit both the books and the comments. He can also activate or revoke user's roles
