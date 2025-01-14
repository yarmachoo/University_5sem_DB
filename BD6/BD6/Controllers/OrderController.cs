using BD6.Services;
using Microsoft.AspNetCore.Mvc;

namespace BD6.Controllers
{
    public class OrderController : Controller
    {
        private readonly DbService _dbService;

        public OrderController(DbService dbService)
        {
            _dbService = dbService;
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int bookId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            await _dbService.AddBookToOrderAsync(userId.Value, bookId);
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> ViewOrders()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = await _dbService.GetUserOrdersAsync(userId.Value);
            return View(orders);
        }
    }

}
