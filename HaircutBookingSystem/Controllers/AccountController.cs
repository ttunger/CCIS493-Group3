using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HaircutBookingSystem.Models;
using HaircutBookingSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace HaircutBookingSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _db = db;
        }

        // ---------- REGISTER ----------
        [AllowAnonymous]
        public IActionResult Register() => View();

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(AccountRegisterViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = new ApplicationUser { UserName = vm.Email, Email = vm.Email };
            var result = await _userManager.CreateAsync(user, vm.Password);

            if (result.Succeeded)
            {
                // Optional: ensure basic role exists and assign (uncomment if needed)
                // if (!await _roleManager.RoleExistsAsync("Client"))
                //     await _roleManager.CreateAsync(new IdentityRole("Client"));
                // await _userManager.AddToRoleAsync(user, "Client");

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(vm);
        }

        // ---------- LOGIN ----------
        [AllowAnonymous]
        public IActionResult Login() => View();

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AccountLoginViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _signInManager.PasswordSignInAsync(
                userName: vm.Email,          // you register with UserName = Email
                password: vm.Password,
                isPersistent: vm.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded) return RedirectToAction("Index", "Home");

            ModelState.AddModelError(string.Empty, "Invalid login.");
            return View(vm);
        }

        // ---------- LOGOUT (POST) ----------
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync(); // use Identity sign-out
            return Redirect("~/");
        }

        // ---------- ADMIN: LIST USERS ----------
        [Authorize(Roles = "Admin")]
        public IActionResult AllUser()
        {
            var users = _db.Users.ToList();
            var userRoles = new Dictionary<string, List<string>>();
            foreach (var u in users)
                userRoles[u.Id] = _userManager.GetRolesAsync(u).Result.ToList();

            ViewBag.UserRoles = userRoles;
            return View(users);
        }

        // ---------- ROLE PICKER (when a user has multiple roles) ----------
        [Authorize]
        public IActionResult SelectRole()
        {
            var rolesJson = HttpContext.Session.GetString("UserRoles");
            var roleList = string.IsNullOrEmpty(rolesJson)
                ? new List<string>()
                : JsonConvert.DeserializeObject<List<string>>(rolesJson);
            return View(roleList);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetRole(string SelectedRole)
        {
            if (SelectedRole == "Admin") return RedirectToAction("Manage", "Admin");
            if (SelectedRole == "Stylist") return RedirectToAction("Index", "Stylist");
            // default
            return RedirectToAction("Index", "Home");
        }
    }
}
