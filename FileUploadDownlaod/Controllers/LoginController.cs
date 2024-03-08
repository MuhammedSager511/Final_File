using FileUploadDownlaod.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FileUploadDownlaod.Controllers
{
    public class LoginController : Controller
    {
        [Route("/login")]
        public IActionResult Index()
        {
            return View();
        }
        [Route("/login")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(Login login)
        {

            if (login.Username=="mvc@mvc.com"&&login.Password=="123456789")
            {
                var claims = new List<Claim>{
                new Claim(ClaimTypes.NameIdentifier, login.Username),
                new Claim(ClaimTypes.Role,"Administrator")
               
                };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
           

                return RedirectToAction("Index","Home");
            }
            else
            {
                TempData["ErrorMessage"] = "خطأ في اسم المستخدم أو كلمة المرور";
                return View();
            }
        }

        [Route("/logout")]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");

        }
    }
}
