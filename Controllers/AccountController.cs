using Food_Market.Data.Entity;
using Food_Market.Data.Services;
using Food_Market.ViewModel.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Food_Market.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<AppUser> _userManager;
        public readonly RoleManager<AppRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IStorageService _storageService;

        public AccountController(IStorageService storageService, RoleManager<AppRole> roleManager, IConfiguration configuration, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _storageService = storageService;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _userManager = userManager;
        }


        [HttpGet("account/logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet()]
        public async Task<IActionResult> Login(string returnUrl)
        {
            await _signInManager.SignOutAsync();
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Login(UserLogin request)
        {
            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user == null) return View();
            var result = await _signInManager.PasswordSignInAsync(user, request.Password, true, lockoutOnFailure: false);
            if (!result.Succeeded) {
                TempData["message"] = "Password Wrong";
                return View();
            }
            
            var roles = await _userManager.GetRolesAsync(user);
            SessionHelper.SetObjectAsJson(HttpContext.Session, "isAuthenticated", user);
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            var url = Request.Form["returnUrl"];

            if (roles.Count > 0) {
                await _userManager.AddClaimAsync(user, new Claim("UserRole", "Admin"));
                if (!url.Equals(""))
                {
                    return Redirect(url);
                }
                return Redirect("/admin");
            }

            if (!url.Equals(""))
            {
                return Redirect(url);
            }
            return Redirect("/");
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AccessDenied()
        {
            return View();
        }  
        public IActionResult Register()
        {
            return View();
        }
        private async Task<string> SaveFile(IFormFile file)
        {
            var originalFileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";
            await _storageService.SaveFileAsync(file.OpenReadStream(), fileName);
            return fileName;
        }
        public async Task<IActionResult> RegisterAccount(UserCreate user)
        {
            var check = await _userManager.FindByNameAsync(user.UserName);
            if (check != null)
            {
                TempData["error"] = "Username existed !!";
                return View();
            }
            var appuser = new AppUser()
            {
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                FullName = user.FullName,
                Address = user.Address,
                CreatedAt = DateTime.Now,
                UserName = user.UserName,
            };
            if (user.Image != null)
            {
                appuser.Image = await SaveFile(user.Image);
            }
            var result = await _userManager.CreateAsync(appuser, user.Password);
            if (result.Succeeded)
            {
                return Redirect("/account/login");
            }
            TempData["error"] = result.Errors;
            return RedirectToAction(nameof(Register));
        }


    }
}