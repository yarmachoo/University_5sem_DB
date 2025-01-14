using BD6.Entities;
using BD6.Services;
using Microsoft.AspNetCore.Mvc;

namespace BD6.Controllers
{
    public class AuthorController : Controller
    {
        private readonly DbService _dbService;

        public AuthorController(DbService dbService)
        {
            _dbService = dbService;
        }

        public async Task<IActionResult> Index()
        {
            var authors = await _dbService.GetAllAuthorsAsync();
            return View(authors);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Author author)
        {
            if (ModelState.IsValid)
            {
                await _dbService.AddAuthorAsync(author);
                return RedirectToAction(nameof(Index));
            }
            return View(author);
        }

        public async Task<IActionResult> Delete(int id)
        {
            await _dbService.DeleteAuthorAsync(id);
            return RedirectToAction(nameof(Index));
        }

    }
}
