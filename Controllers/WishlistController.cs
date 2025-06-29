﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tradie.Data;
using Tradie.Models.ShoppingCart;
using Tradie.Models.Wishlist;

namespace Tradie.Controllers
{
	public class WishlistController : BaseController
	{
		private readonly ApplicationDbContext _context;

		public WishlistController(ApplicationDbContext context, UserManager<User> userManager)
			: base(userManager, context)
		{
			_context = context;
		}

		// GET: Wishlist
		public async Task<IActionResult> Index()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
				return RedirectToAction("Login", "Account");

			var wishlist = await _context.Wishlists
				.Include(w => w.Items)
				.ThenInclude(i => i.Product)
				.FirstOrDefaultAsync(w => w.UserId == user.Id.ToString());

			if (wishlist == null)
			{
				wishlist = new Wishlist
				{
					UserId = user.Id.ToString(),
					Items = new List<WishlistItem>()
				};
				_context.Wishlists.Add(wishlist);
				await _context.SaveChangesAsync();
			}

			return View("~/Views/Wishlist/Wishlist.cshtml", wishlist);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> RemoveItem(int itemId)
		{
			var item = await _context.WishlistItems.FindAsync(itemId);
			if (item != null)
			{
				_context.WishlistItems.Remove(item);
				await _context.SaveChangesAsync();
			}

			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddToCart(int itemId)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null) return RedirectToAction("Login", "Account");

			var item = await _context.WishlistItems
				.Include(i => i.Product)
				.FirstOrDefaultAsync(i => i.Id == itemId);

			if (item == null) return NotFound();

			var cart = await _context.ShoppingCarts
				.Include(c => c.Items)
				.FirstOrDefaultAsync(c => c.UserId == user.Id.ToString());

			if (cart == null)
			{
				cart = new ShoppingCart
				{
					UserId = user.Id.ToString(),
					Items = new List<CartItem>()
				};
				_context.ShoppingCarts.Add(cart);
				await _context.SaveChangesAsync();
			}

			cart.AddItem(item.Product, item.Quantity, item.PriceAtAddition);

			_context.WishlistItems.Remove(item);

			await _context.SaveChangesAsync();

			TempData["ToastMessage"] = $"<strong>{item.ProductName}</strong> se añade a tu <a href='/ShoppingCart'>carrito</a>.";
			TempData["ToastType"] = "success";

			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddToWishList(int productId, string returnUrl)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
				return RedirectToAction("Login", "Account");

			var product = await _context.Products.FindAsync(productId);
			if (product == null)
				return NotFound();

			// Use the discounted price if available, else fallback to regular price
			var priceAtAddition = product.DiscountedPrice.HasValue ? product.DiscountedPrice.Value : product.Price;

			var wishlist = await _context.Wishlists
				.Include(w => w.Items)
				.FirstOrDefaultAsync(w => w.UserId == user.Id.ToString());

			if (wishlist == null)
			{
				wishlist = new Wishlist
				{
					UserId = user.Id.ToString(),
					Items = new List<WishlistItem>()
				};
				_context.Wishlists.Add(wishlist);
			}

			// Check for duplicates
			var existingItem = wishlist.Items.FirstOrDefault(i => i.ProductId == product.Id);
			if (existingItem != null)
			{
				existingItem.Quantity += 1;
			}
			else
			{
				wishlist.Items.Add(new WishlistItem
				{
					ProductId = product.Id,
					ProductName = product.Name,
					Quantity = 1,
					PriceAtAddition = product.Price,
					ImageUrl = product.ImageUrl,
					Size = "Default",
					Color = "Default"
				});
			}

			await _context.SaveChangesAsync();

			TempData["ToastMessage"] = $"<strong>{product.Name}</strong> se añade a la  <a href='/Wishlist'>lista de deseos</a>.";
			TempData["ToastType"] = "success";

			return LocalRedirect(returnUrl ?? "/");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ToggleWishlistItem(int productId, string returnUrl)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
				return RedirectToAction("Login", "Account");

			var wishlist = await _context.Wishlists
				.Include(w => w.Items)
				.FirstOrDefaultAsync(w => w.UserId == user.Id.ToString());

			if (wishlist == null)
			{
				wishlist = new Wishlist
				{
					UserId = user.Id.ToString(),
					Items = new List<WishlistItem>()
				};
				_context.Wishlists.Add(wishlist);
				await _context.SaveChangesAsync();
			}

			var existingItem = wishlist.Items.FirstOrDefault(i => i.ProductId == productId);
			if (existingItem != null)
			{
				_context.WishlistItems.Remove(existingItem);
				TempData["ToastMessage"] = $"Artículo eliminado de la <a href='/Wishlist'>lista de deseos</a>.";
				TempData["ToastType"] = "info";
			}
			else
			{
				var product = await _context.Products.FindAsync(productId);
				if (product == null) return NotFound();

				wishlist.Items.Add(new WishlistItem
				{
					ProductId = product.Id,
					ProductName = product.Name,
					Quantity = 1,
					PriceAtAddition = product.Price,
					ImageUrl = product.ImageUrl,
					Size = "Default",
					Color = "Default"
				});

				TempData["ToastMessage"] = $"<strong>{product.Name}</strong> se añade a la <a href='/Wishlist'>lista de deseos</a>.";
				TempData["ToastType"] = "success";
			}

			await _context.SaveChangesAsync();
			return LocalRedirect(returnUrl ?? "/");
		}


	}
}
