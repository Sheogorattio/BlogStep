using Blog.Helpers;
using Blog.Interfaces;
using Blog.Models;
using Blog.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Blog.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;


        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }


        [Route("login")]
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }


        [Route("login")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
            
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Неправильный логин и (или) пароль");
                }
            }

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }


        [Route("register")]
        [HttpGet]   
        public async Task<IActionResult> Register([FromServices] IMembership membership, string? code)
        {
            if (!User.Identity.IsAuthenticated || code != null)
            {
                if (await membership.ExistsMembershipByCodeAsync(code))
                {
                    if (await membership.EnableCodeMembershipByCodeAsync(code))
                    {
                        return View(new RegisterUserViewModel { Code = code });
                    }
                }
            }
            return RedirectToAction("Index", "Home");
        }


        [Route("register")]
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Register([FromServices] IMembership membership, RegisterUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userCheck = await _userManager.FindByEmailAsync(model.Email);
                if (userCheck != null)
                {
                    ModelState.AddModelError("Email", "Такой email адрес уже есть");
                    return View(model);
                }
                User user = new User { Email = model.Email, UserName = model.Email, Name = model.Name, PhoneNumber = model.Phone };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Editor");
                    await membership.DisableMembershipCodeAsync(model.Code);
                    await _signInManager.SignInAsync(user, true);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
        }


        [Route("settings")]
        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.FindByIdAsync(userId);
            return View(currentUser);
        }


        [Route("change-password")]
        [HttpGet]
        public async Task<IActionResult> ChangePassword(string id)
        {
            User user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            ChangePasswordViewModel model = new ChangePasswordViewModel { Id = user.Id, Email = user.Email };
            return View(model);
        }


        [Route("change-password")]
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userManager.FindByIdAsync(model.Id);
                if (user != null)
                {
                    IdentityResult result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword); 
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Settings");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            if (error.Description.Equals("Incorrect password."))
                            {
                                ModelState.AddModelError(string.Empty, "Введён неверный текущий пароль");
                                continue;
                            }
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Пользователь не найден");
                }
            }
            return View(model);
        }


        [Route("edit-info")]
        [HttpGet]
        public async Task<IActionResult> EditInfo(string id)
        {
            User user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            EditUserViewModel model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Phone = user.PhoneNumber
            };

            return View(model);
        }


        [Route("edit-info")]
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> EditInfo(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userManager.FindByIdAsync(model.Id);
                if (user != null)
                {
                    user.Email = model.Email;
                    user.Name = model.Name;
                    user.PhoneNumber = model.Phone;

                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Settings");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
            }

            return View(model);
        }


        [Route("forgot-password")]
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [Route("forgot-password")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword([Required] string email)
        {
            if (!ModelState.IsValid)
            {
                return View(model: email);
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction(nameof(Register));
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var link = Url.Action("ResetPassword", "Account", new { token, email = user.Email }, Request.Scheme);

            EmailHelper emailHelper = new EmailHelper();
            bool emailResponse = emailHelper.SendEmailPasswordReset(user.Email, link);

            if (emailResponse)
            {
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }
            else
            {
                ModelState.AddModelError("", "Произошла ошибка, попробуйте позже.");
            }
            return View(model: email);
        }


        [Route("forgot-password-confirmation")]

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }


        [Route("reset-password")]

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            var model = new ResetPassword { Token = token, Email = email };
            return View(model);
        }

        [Route("reset-password")]

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPassword resetPassword)
        {
            if (!ModelState.IsValid)
            {
                return View(resetPassword);
            }

            var user = await _userManager.FindByEmailAsync(resetPassword.Email);
            if (user == null)
            {
                RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            var resetPassResult = await _userManager.ResetPasswordAsync(user, resetPassword.Token, resetPassword.Password);
            if (!resetPassResult.Succeeded)
            {
                foreach (var error in resetPassResult.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return View(resetPassword);
            }
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }


        [Route("reset-password-confirmation")]

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

    }
}
