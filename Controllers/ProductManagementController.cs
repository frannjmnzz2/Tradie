using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tradie.Data;
using Tradie.Models.Products;

namespace Tradie.Controllers
{
	[Authorize(Roles = "Admin, Seller")]
	public class ProductManagementController : Controller
	{
		private readonly ILogger<ProductManagementController> _logger;
		private readonly ApplicationDbContext _context;
		private readonly UserManager<User> _userMgr;

		public ProductManagementController(
			ILogger<ProductManagementController> logger,
			ApplicationDbContext context, UserManager<User> userMgr)
		{
			_logger = logger;
			_context = context;
			_userMgr = userMgr;
		}

		[HttpGet]
		public async Task<IActionResult> ProductRegistry(string? searchTerm, string? category)
		{
			// For logged in admin 
			var currentAdmin = await _userMgr.GetUserAsync(User);
			if (currentAdmin != null)
			{
				ViewData["AdminName"] = currentAdmin.Name;
				ViewData["AdminEmail"] = currentAdmin.Email;
				ViewData["AdminPhoto"] = currentAdmin.ProfilePhotoUrl;
			}
			else
			{
				// Optional fallback
				ViewData["AdminName"] = "Admin";
				ViewData["AdminEmail"] = "admin@example.com";
				ViewData["AdminPhoto"] = null;
			}

			var query = _context.Products.Include(p => p.Seller).AsQueryable();

			if (!string.IsNullOrEmpty(searchTerm))
			{
				query = query
					.Where(p => p.Name.Contains(searchTerm)
							 || p.Description.Contains(searchTerm));
			}

			if (!string.IsNullOrEmpty(category))
			{
				query = query.Where(p => p.Subcategory != null &&
										p.Subcategory.ToLower() == category.ToLower());
			}

			var vm = new ProductRegistryViewModel
			{
				Products = await query.ToListAsync(),
				SearchTerm = searchTerm,
				Categories = await _context.Categories.ToListAsync()
			};

			return View(vm);
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ProductRegistry(ProductRegistryViewModel vm)
		{
			var product = vm.NewProduct;
			var currentUser = await _userMgr.GetUserAsync(User);

			product.SellerId = currentUser!.Id;

			if (ModelState.IsValid)
			{
				_context.Products.Add(product);
				await _context.SaveChangesAsync();

				return RedirectToAction(nameof(ProductRegistry));
			}

			vm.Products = await _context.Products
				.Include(p => p.Seller)
				.ToListAsync();

			vm.Categories = await _context.Categories.ToListAsync();

			return View(vm);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditProduct(ProductRegistryViewModel vm)
		{
			var prod = vm.NewProduct;
			var user = await _userMgr.GetUserAsync(User);
			prod.SellerId = user!.Id;
			if (ModelState.IsValid)
			{
				_context.Products.Update(prod);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(ProductRegistry));
			}
			vm.Products = await _context.Products
				 .Include(p => p.Seller)
				 .ToListAsync();

			vm.Categories = await _context.Categories.ToListAsync();

			return View("ProductRegistry", vm);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteProduct(int id)
		{
			var prod = await _context.Products.FindAsync(id);
			if (prod != null)
			{
				_context.Products.Remove(prod);
				await _context.SaveChangesAsync();
			}
			return RedirectToAction(nameof(ProductRegistry));
		}


	}
}
