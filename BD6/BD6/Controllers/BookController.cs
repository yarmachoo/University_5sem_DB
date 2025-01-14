using BD6.Entities;
using BD6.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BD6.Controllers
{
    public class BookController : Controller
    {
        private readonly DbService _dbService;
        public BookController(DbService dbService)
        {
            _dbService = dbService;
        }
        public async Task<IActionResult> Index()
        
        {
            var books = await _dbService.GetAllBooks();
            return View(books);
        }
        // Метод для отображения формы создания книги
        public async Task<IActionResult> Create()
        {
            ViewData["Publishings"] = await _dbService.GetAllPublishingsAsync();
            ViewData["Authors"] = await _dbService.GetAllAuthorsAsync();
            ViewData["Categories"] = await _dbService.GetAllCategoriesAsync();

            return View(new Book());
        }



        [HttpPost]
        public async Task<IActionResult> Create(Book book, List<int> selectedAuthors, List<int> selectedCategories)
        {
                var result = await _dbService.AddBookWithDetails(book, selectedAuthors, selectedCategories);
                if (result)
                {
                    return RedirectToAction(nameof(Index));
                }

            ViewData["Publishings"] = await _dbService.GetAllPublishingsAsync();
            ViewData["Authors"] = await _dbService.GetAllAuthorsAsync();
            ViewData["Categories"] = await _dbService.GetAllCategoriesAsync();

            return View(book);
        }

        // Метод для редактирования книги (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var book = await _dbService.GetBookById(id);
            if (book == null)
            {
                return NotFound();
            }

            var publishings = await _dbService.GetAllPublishingsAsync();
            ViewData["Publishings"] = publishings;
            return View(book);
        }

        // Метод для редактирования книги (POST)
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Book book)
        {
            if (id != book.BookId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                var publishings = await _dbService.GetAllPublishingsAsync();
                ViewData["Publishings"] = publishings;
                return View(book);
            }

            var success = await _dbService.UpdateBook(book);
            if (success)
            {
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Error = "An error occurred while updating the book.";
            var publishingsRetry = await _dbService.GetAllPublishingsAsync();
            ViewData["Publishings"] = publishingsRetry;
            return View(book);
        }

        // Метод для удаления книги (GET)
        public async Task<IActionResult> Delete(int bookId)
        {
            var book = await _dbService.GetBookById(bookId);
            if (book != null)
            {
                await _dbService.DeleteBook(bookId);
                return RedirectToAction("Index");
            }
            return NotFound();
        }


        // Метод для удаления книги (POST)
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _dbService.DeleteBook(id);
            if (success)
            {
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Error = "An error occurred while deleting the book.";
            return View();
        }

        public async Task<IActionResult> Details(int id)
        {
            var book = await _dbService.GetBookById(id);

            if (book == null)
            {
                return NotFound();
            }

            var reviews = await _dbService.GetFeedbacksByBookIdAsync(id);
            ViewData["Reviews"] = reviews;

            return View(book);
        }

    }
}
