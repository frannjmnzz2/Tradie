﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tradie.Data;
using Tradie.Models.Users;

namespace Tradie.Controllers
{
	public class AccountController : BaseController

	{
		private readonly UserManager<User> _userMgr;
		private readonly SignInManager<User> _signInMgr;

		public AccountController(UserManager<User> userMgr, SignInManager<User> signInMgr, ApplicationDbContext context)
		: base(userMgr, context)
		{
			_userMgr = userMgr;
			_signInMgr = signInMgr;
		}

		[HttpGet]
		public IActionResult Login()
		{
			return View("~/Views/Login/Login.cshtml");
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		[AllowAnonymous]
		public async Task<IActionResult> VerifyLogin(LoginModel model)
		{
			if (ModelState.IsValid)
			{
				var user = await _userMgr.FindByEmailAsync(model.UserName);

				if (user == null)
				{
					ModelState.AddModelError("", "El correo o la contraseña son incorrectos.");
					return View("~/Views/Login/Login.cshtml", model);
				}

				var result = await _signInMgr.PasswordSignInAsync(
					model.UserName,
					model.Password,
					isPersistent: false,
					lockoutOnFailure: false);

				if (result.Succeeded)
				{
                    return RedirectToAction("Index", "Home");
                }
				else
				{
					ModelState.AddModelError("", "El correo o la contraseña son incorrectos.");
					return View("~/Views/Login/Login.cshtml", model);
				}
			}
			return View("~/Views/Login/Login.cshtml", model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Logout()
		{
			await _signInMgr.SignOutAsync();
			return RedirectToAction("Login", "Account");
		}
	}
}
