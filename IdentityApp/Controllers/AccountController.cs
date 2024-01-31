using IdentityApp.Models;
using IdentityApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApp.Controllers
{
	public class AccountController : Controller
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly RoleManager<AppRole> _roleManager;
		private readonly SignInManager<AppUser> _signInManager;
		private readonly IEMailSender _emailSender;

		public AccountController(
			UserManager<AppUser> userManager, 
			RoleManager<AppRole> roleManager,
			SignInManager<AppUser>signInManager,
			IEMailSender emailSender)
		{
			_userManager = userManager;
			_roleManager = roleManager;
			_signInManager = signInManager;
			_emailSender = emailSender;
		}
		public IActionResult Login()
		{
			return View();
		}

		[HttpPost]
		public async Task <IActionResult> Login(LoginViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user=await _userManager.FindByEmailAsync(model.Email);
			
				if (user != null)
				{
					await _signInManager.SignOutAsync();

					if (!await _userManager.IsEmailConfirmedAsync(user))
					{
						ModelState.AddModelError("", "Hesabinizi onaylayiniz.");
						return View(model);
					}
					

					var result=await _signInManager.PasswordSignInAsync(user, model.Password,model.RememberMe,true);

					if(result.Succeeded)
					{
						await _userManager.ResetAccessFailedCountAsync(user);
						await _userManager.SetLockoutEndDateAsync(user, null);

						return RedirectToAction(nameof(Index), "Home");
					}
					else if(result.IsLockedOut)
					{
						var lockoutDate = await _userManager.GetLockoutEndDateAsync(user);
						var timeLeft = lockoutDate.Value - DateTimeOffset.UtcNow;

						ModelState.AddModelError("", $"Hesabiniz kitlendi, Lutfen {timeLeft.Minutes+1} dakika sonra deneyiniz");

					}
					else
					{
						ModelState.AddModelError("", "Parolaniz Hatali");
					}
				}
                else
                {
					ModelState.AddModelError("", "Email hesabiyla kayitli bir hesap bulunamadi");
                }
            }
			return View(model);
		}
		public IActionResult Create()
		{
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> Create(CreateViewModel model)
		{


			if (ModelState.IsValid)

			{
				var user = new AppUser
				{
					UserName = model.UserName,
					Email = model.Email,
					FullName = model.FullName

				};
				IdentityResult result = await _userManager.CreateAsync(user, model.Password);
				if (result.Succeeded)
					
				{
					var token=await _userManager.GenerateEmailConfirmationTokenAsync(user);
					var url = Url.Action("ConfirmEmail", "Account", new { user.Id, token });
					//email

					await _emailSender.SendEmailAsync(user.Email, "Hesap Onayi", $"Lutfen email hesabinizi onaylamak icin linke <a href='https://localhost:7155{url}'>tiklayiniz.</a>");
					
                    TempData["message"] = "Email hesabinizdaki onay mailini tiklayiniz";
                    return RedirectToAction("Login","Account");
				}
				foreach (IdentityError error in result.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}

				return View(model);
			}
			return View();
		}
		public async Task <IActionResult>ConfirmEmail(string Id, string token)
		{
			if (Id == null || token == null)
			{
				TempData["message"] = "Gecersiz token bilgisi";
				return View();
			}
			var user=await _userManager.FindByIdAsync(Id);

			if (user != null)
			{
				var result=await _userManager.ConfirmEmailAsync(user, token);
				if (result.Succeeded)
				{
                    TempData["message"] = "Hesabiniz onaylandi";
					return RedirectToAction("Login", "Account");
                }
			}
            TempData["message"] = "Kullanici bulunamadi";
            return View();
        }
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			return RedirectToAction(nameof(Login));
		}
		public IActionResult ForgotPassword()
		{
			return	View();
		}
		[HttpPost]
		public async Task<IActionResult> ForgotPassword(string Email)
		{
			if(string.IsNullOrEmpty(Email))
			{
				TempData["message"] = "Eposta adresinizi giriniz.";
				return View();
			}
			var user=await _userManager.FindByEmailAsync(Email);
			if (user == null)
			{
				TempData["message"] = "Eposta adresiyle eslesen bir kayit yok";
				return	View();
			}
			var token = await _userManager.GeneratePasswordResetTokenAsync(user);
			var url = Url.Action("ResetPassword", "Account", new { user.Id, token });
			await _emailSender.SendEmailAsync(Email, "Parola Sifirlama", $"Parolanizi yenilemek icin linke <a href='https://localhost:7155{url}'>tiklayiniz.</a> tiklayiniz.");
			TempData["message"] = "Eposta adresinize gonderilen link ile sifrenizi sifirlayabilirsiniz.";
			return View();
		}
		public IActionResult ResetPassword(string Id, string token)
		{
			if(Id==null || token == null)
			{
				return RedirectToAction("Login");
			}
			var model = new ResetPasswordModel
			{
				Token = token

			};
			return View(model);
		}
		[HttpPost]
		public async Task<IActionResult>ResetPassword(ResetPasswordModel model)
		{
            if (ModelState.IsValid)
			{
				var user = await _userManager.FindByEmailAsync(model.Email);
				if(user == null)
				{
					TempData["message"] = "Bu mail adresiyle eslesen kullanici yok";
					return RedirectToAction("Login");
				}
				var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
				if(result.Succeeded)
				{
					TempData["message"] = "Sifreniz degistirildi";
					return RedirectToAction("Login");
				}
				foreach (IdentityError error in result.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}
			}
			return View(model);
            
        }
	}
}
