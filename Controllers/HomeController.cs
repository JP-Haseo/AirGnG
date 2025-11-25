using AirGnG.Models;
using AirGnG.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text.Json; // Used for serializing/deserializing the cart from session

namespace AirGnG.Controllers
{
    public class HomeController : Controller
    {
        private readonly AirGnGContext _context;
        private const string CartSessionKey = "ShoppingCart";

        public HomeController(AirGnGContext context)
        {
            _context = context;
        }

        // GET: Home/Index - Displays the menu and allows item selection
        public async Task<IActionResult> Index()
        {
            var menuItems = await _context.MenuItems.ToListAsync();
            return View(menuItems);
        }

        // POST: Home/AddToCart - Adds selected items to the session cart
        [HttpPost]
        public IActionResult AddToCart(int[] selectedItemIds)
        {
            if (selectedItemIds == null || selectedItemIds.Length == 0)
            {
                return RedirectToAction(nameof(Index));
            }

            // Get existing cart or create a new one
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            var cartItems = new Dictionary<int, int>(); // ItemId, Quantity

            if (!string.IsNullOrEmpty(cartJson))
            {
                cartItems = JsonSerializer.Deserialize<Dictionary<int, int>>(cartJson) ?? new Dictionary<int, int>();
            }

            // Add/Update the selected items in the cart
            foreach (var itemId in selectedItemIds.Distinct())
            {
                if (cartItems.ContainsKey(itemId))
                {
                    cartItems[itemId] += 1;
                }
                else
                {
                    cartItems.Add(itemId, 1);
                }
            }

            // Store the updated cart back in session
            HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(cartItems));

            // Proceed to the ShoppingCart View
            return RedirectToAction(nameof(ShoppingCart));
        }

        // GET: Home/ShoppingCart - Displays the selected items, total, and confirmation button
        public async Task<IActionResult> ShoppingCart()
        {
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            var cartItemsDictionary = new Dictionary<int, int>();

            if (!string.IsNullOrEmpty(cartJson))
            {
                cartItemsDictionary = JsonSerializer.Deserialize<Dictionary<int, int>>(cartJson) ?? new Dictionary<int, int>();
            }

            if (!cartItemsDictionary.Any())
            {
                ViewBag.Total = 0.00f;
                ViewBag.CartDetails = new List<(MenuItem Item, int Quantity)>();
                return View();
            }

            // Fetch the MenuItems corresponding to the IDs in the cart
            var itemIds = cartItemsDictionary.Keys.ToList();
            var menuItems = await _context.MenuItems.Where(m => itemIds.Contains(m.Id)).ToListAsync();

            var cartDetails = new List<(MenuItem Item, int Quantity)>();
            float total = 0.00f;

            foreach (var item in menuItems)
            {
                if (cartItemsDictionary.TryGetValue(item.Id, out int quantity))
                {
                    cartDetails.Add((item, quantity));
                    total += item.Price * quantity;
                }
            }

            ViewBag.Total = total;
            ViewBag.CartDetails = cartDetails;
            return View();
        }

        // POST: Home/ConfirmOrder - Creates the order and clears the cart
        [HttpPost]
        public async Task<IActionResult> ConfirmOrder()
        {
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            var cartItemsDictionary = new Dictionary<int, int>();

            if (!string.IsNullOrEmpty(cartJson))
            {
                cartItemsDictionary = JsonSerializer.Deserialize<Dictionary<int, int>>(cartJson) ?? new Dictionary<int, int>();
            }

            if (!cartItemsDictionary.Any())
            {
                return RedirectToAction(nameof(Index)); // Nothing to order
            }

            // 1. Create the Order entity
            var order = new Order();
            _context.Orders.Add(order);
            await _context.SaveChangesAsync(); // Save to get the Order ID

            // 2. Create OrderItem entities
            var itemIds = cartItemsDictionary.Keys.ToList();
            var menuItems = await _context.MenuItems.Where(m => itemIds.Contains(m.Id)).ToListAsync();

            foreach (var item in menuItems)
            {
                if (cartItemsDictionary.TryGetValue(item.Id, out int quantity))
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        MenuItemId = item.Id,
                        Quantity = quantity
                    };
                    order.OrderItems.Add(orderItem);
                }
            }

            // 3. Save the OrderItems and update the Order
            await _context.SaveChangesAsync();

            // 4. Clear the cart from session
            HttpContext.Session.Remove(CartSessionKey);

            // 5. Return JSON result for the modal
            return Json(new
            {
                success = true,
                orderId = order.Id,
                orderTime = order.OrderTime.ToString("g"), // Use standard format
                completionTime = order.CompletionTime.ToString("g")
            });
        }
    }
}