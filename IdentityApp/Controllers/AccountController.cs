using IdentityApp.Models;
using IdentityApp.ViewModels;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly RoleManager<AppRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailSender _emailSender;
        public AccountController(RoleManager<AppRole> roleManager, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailSender emailSender)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    await _signInManager.SignOutAsync();    //Closes the current session

                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        ModelState.AddModelError("", "Confirm Your Account");
                        return View(model);
                    }

                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, true);
                    //checks the user's password and attempts to log in

                    if (result.Succeeded)
                    {
                        await _userManager.ResetAccessFailedCountAsync(user);   //Resets the user's number of failed login attempts
                        await _userManager.SetLockoutEndDateAsync(user, null);  //Resets the user's lockout date

                        return RedirectToAction("Index", "Home");

                    }
                    else if (result.IsLockedOut)
                    {
                        var lockoutDate = await _userManager.GetLockoutEndDateAsync(user);  //Gets the lock date
                        var timeLeft = lockoutDate.Value - DateTime.UtcNow;
                        ModelState.AddModelError("", $"Your account is locked. Please try after {timeLeft.Minutes} minutes");
                        //Calculates the time remaining until the crash expires and adds an error message
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid password");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "No account found with this mail adresses");
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

                IdentityResult result = await _userManager.CreateAsync(user, model.Password); //create the user with the specified password
                if (result.Succeeded)
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);   //If user creation is successful, generates an email confirmation token
                    var url = Url.Action("ConfirmEmail", "Account", new { user.Id, token });
                    //Constructs a URL for email confirmation using Url.Action with the ConfirmEmail action, user ID, and token.
                    //1.Parameter->ConfirmEmail=>Method name. Direct to method
                    //2.Parameter->Account=>Controller name. Direct to controller
                    //3.Parameter->Query string parameters. Matches the parameters in the ConfirmEmail method

                    await _emailSender.SendEmailAsync(user.Email, "Account Confirmation", $"Please <a href = 'https://localhost:44341{url}'>click </a> on the link to confirm your email account.");

                    TempData["message"] = "Please verify your account with the email sent";
                    return RedirectToAction("Login", "Account");
                }
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        public async Task<IActionResult> ConfirmEmail(string Id, string token)
        {
            if (Id == null || token == null)
            {
                TempData["message"] = "Invalid token information";
                return View();
            }
            var user = await _userManager.FindByIdAsync(Id);
            if (user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    TempData["message"] = "Account Confirmed";
                    return RedirectToAction("Login", "Account");
                }
            }
            TempData["message"] = "User not found";
            return View();

        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        public IActionResult ForgotPassword()
        {

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string Email)
        {
            if (string.IsNullOrEmpty(Email))
            {
                TempData["message"] = "Please enter your email address";
                return View();
            }
            var user = await _userManager.FindByNameAsync(Email); //The user with the e-mail address is searched
            if (user == null)
            {
                TempData["message"] = "There is no account matching your email address";

                return View();
            }
            var token=await _userManager.GeneratePasswordResetTokenAsync(user); //A password reset token is generated.
            var url = Url.Action("ResetPassword", "Account", new { user.Id, token });   //A password reset link is created.
            await _emailSender.SendEmailAsync(Email, "Reset Password", $"<a href = 'https://localhost:44341{url}'>Click </a> on the link to reset your password.");
            TempData["message"] = "You can reset your password with the link in your e-mail address.";

            return View();
        }

        public IActionResult ResetPassword(string Id, string token)
        {
            if (Id == null || token == null)
            {
                return RedirectToAction("Login");
            }
            var model = new ResetPasswordViewModel { Token = token };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);    //The user with the e-mail address is searched
                if (user == null)
                {
                    TempData["message"] = "There is no account matching your email address";
                    return RedirectToAction("Login");
                }
                var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);  //Password reset is performed
                if (result.Succeeded)
                {
                    TempData["message"] = "Changed your password";
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
