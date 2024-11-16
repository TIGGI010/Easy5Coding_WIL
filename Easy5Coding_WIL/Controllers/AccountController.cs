using Easy5Coding_WIL.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using FirebaseAdmin.Auth;
using FirebaseAdmin;

namespace Easy5Coding_WIL.Controllers
{
    public class AccountController : Controller
    {
        private readonly FirebaseService _firebaseService;

        public AccountController(FirebaseService firebaseService)
        {
            _firebaseService = firebaseService;
        }

        [HttpGet]
        public IActionResult Login() => View(new LoginViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Attempt to authenticate the user using Firebase
                    bool isValidUser = await _firebaseService.ValidateUserCredentialsAsync(model.Email, model.Password);

                    if (isValidUser)
                    {
                        // Retrieve the user details after successful authentication
                        var userRecord = await _firebaseService.GetUserByEmailAsync(model.Email);
                        if (userRecord != null)
                        {
                            // Log the UID for debugging purposes
                            Console.WriteLine("Authenticated User UID: " + userRecord.Uid);

                            // Retrieve user role from the Firebase database
                            var role = await _firebaseService.GetUserRoleAsync(userRecord.Uid);
                            if (role == null)
                            {
                                ModelState.AddModelError(string.Empty, "Role not found for the user.");
                                return View(model);
                            }

                            // Sign in the user
                            await SignInUserAsync(userRecord, role, model.Email, model.RememberMe);
                            return RedirectToRoleHome(role, returnUrl ); // Redirect based on role
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "User record not found. Try registering first.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid email or password.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                    Console.WriteLine("Error in Login: " + ex.Message);
                }
            }
            return View(model);
        }


        [HttpGet]
        public IActionResult Register() => View(new RegisterModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    bool registrationSuccess = await _firebaseService.RegisterUserAsync(model.Email, model.Password, model.Role);
                    if (registrationSuccess)
                    {
                        var userRecord = await _firebaseService.GetUserByEmailAsync(model.Email);
                        if (userRecord != null)
                        {
                            // Log the UID for debugging purposes
                            Console.WriteLine("Newly Registered User UID: " + userRecord.Uid);

                            await SignInUserAsync(userRecord, model.Role, model.Email);
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Registration succeeded, but user record not found.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Registration failed. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                    Console.WriteLine("Error in Register: " + ex.Message);
                }
            }
            return View("~/Views/Shared/Register.cshtml"); // Explicitly point to the Shared folder
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        private async Task SignInUserAsync(UserRecord userRecord, string role, string email, bool isPersistent = false)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userRecord.Uid),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Role, role ?? "Manager") // Default role as "User" if none provided
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = isPersistent
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
        }

        private IActionResult RedirectToRoleHome(string role, string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return role switch
            {
                "Manager" => RedirectToAction("Index", "Home"),
                "HRManager" => RedirectToAction("Index", "Home"),
                "Employee" => RedirectToAction("Dashboard", "Dashboard"),
                _ => RedirectToAction("Index", "Home")
            };
        }

    }
}
