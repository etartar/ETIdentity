using ETIdentity.Helpers;
using ETIdentity.Models;
using ETIdentity.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ETIdentity.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public HomeController(ILogger<HomeController> logger, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login(string returnUrl)
        {
            TempData["ReturnUrl"] = returnUrl;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                AppUser user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    if (await _userManager.IsLockedOutAsync(user))
                    {
                        ModelState.AddModelError("", "Hesabınız bir süreliğine kitlenmiştir. Lütfen daha sonra tekrar deneyiniz.");
                    }
                    else
                    {
                        await _signInManager.SignOutAsync();

                        Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

                        if (result.Succeeded)
                        {
                            await _userManager.ResetAccessFailedCountAsync(user);

                            if (TempData["ReturnUrl"] != null)
                            {
                                return Redirect(TempData["ReturnUrl"].ToString());
                            }

                            return RedirectToAction("Index", "Member");
                        }
                        else
                        {
                            await _userManager.AccessFailedAsync(user);

                            int fail = await _userManager.GetAccessFailedCountAsync(user);

                            if (fail == 3)
                            {
                                await _userManager.SetLockoutEndDateAsync(user, new DateTimeOffset(DateTime.Now.AddMinutes(20)));

                                ModelState.AddModelError("", "Hesabınız 3 başarısız girişten dolayı 20 dakika süre ile kitlenmiştir. Lütfen daha sonra tekrar deneyiniz.");
                            }
                            else
                            {
                                ModelState.AddModelError("", "Kullanıcı bilgileri hatalı.");
                            }
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Kullanıcı bilgileri hatalı.");
                }
            }

            return View(model);
        }

        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                AppUser user = new AppUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber
                };

                IdentityResult signUpResult = await _userManager.CreateAsync(user, model.Password);
                
                if (signUpResult.Succeeded)
                {
                    return RedirectToAction("Login");
                }
                else
                {
                    foreach (IdentityError error in signUpResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            return View(model);
        }
    
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(PasswordResetViewModel model)
        {
            if (ModelState.IsValid)
            {
                AppUser user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    string passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

                    string passwordResetLink = Url.Action("ResetPasswordConfirm", "Home", new
                    {
                        userId = user.Id,
                        token = passwordResetToken
                    }, HttpContext.Request.Scheme);

                    PasswordReset.PasswordResetSendEmail(passwordResetLink);

                    ViewBag.Status = "success";
                }
                else
                {
                    ModelState.AddModelError("", "Kullanıcı bilgisi bulunamadı.");
                }
            }

            return View(model);
        }
        
        public IActionResult ResetPasswordConfirm(string userId, string token)
        {
            TempData["userId"] = userId;
            TempData["token"] = token;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPasswordConfirm([Bind("Password")] PasswordResetViewModel model)
        {
            if (ModelState.IsValid)
            {
                string userId = TempData["userId"].ToString();
                string token = TempData["token"].ToString();

                AppUser user = await _userManager.FindByIdAsync(userId);

                if (user != null)
                {
                    IdentityResult result = await _userManager.ResetPasswordAsync(user, token, model.Password);

                    if (result.Succeeded)
                    {
                        await _userManager.UpdateSecurityStampAsync(user);

                        ViewBag.status = "success";
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Bir meydana gelmiştir. Daha sonra tekrar deneyiniz.");
                }
            }

            return View(model);
        }
    }
}
