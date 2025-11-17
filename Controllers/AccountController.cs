using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Quanlinhahang.Data;
using System.Linq;

namespace Quanlinhahang.Controllers
{
    public class AccountController : Controller
    {
        private readonly QlnhContext _context;

        public AccountController(QlnhContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = _context.TaiKhoans
                .FirstOrDefault(t => t.TenDangNhap == username && t.MatKhauHash == password);

            if (user == null)
            {
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng!";
                return View();
            }

            // ✅ Tạo Claims cho Cookie Auth
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.TenDangNhap),
                new Claim(ClaimTypes.Role, user.VaiTro)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(30)
                });

            // ✅ Lưu thêm session (tuỳ chọn)
            HttpContext.Session.SetString("UserName", user.TenDangNhap);
            HttpContext.Session.SetString("Role", user.VaiTro);

            // ✅ Chuyển hướng phù hợp
            if (user.VaiTro == "Admin")
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            else
                return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}
