using CinemaBookingWeb.Models;
using CinemaBookingWeb.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System;
using System.Security.Claims;

namespace CinemaBookingWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly CinemaBookingWebContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public AccountController(CinemaBookingWebContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }
        private string HashPassword(string password)
        {
            var passwordHasher = new PasswordHasher<object>();
            return passwordHasher.HashPassword(null, password);
        }

        private async Task<KhachHang?> GetCurrentKhachHangDataAsync()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return null;
            }
            var maKh = User.FindFirstValue("MaKh");
            if (string.IsNullOrEmpty(maKh))
            {
                return null;
            }
            var kh = await _context.KhachHangs
                .Include(k => k.HoaDons) // Kéo theo danh sách hóa đơn
                .FirstOrDefaultAsync(k => k.MaKh == maKh);

            return kh;
        }
        private bool VerifyPassword(string inputPassword, string hashedPassword)
        {
            var passwordHasher = new PasswordHasher<object>();

            var result = passwordHasher.VerifyHashedPassword(null, hashedPassword, inputPassword);

            return result == PasswordVerificationResult.Success;
        }
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl; // <-- Thêm dòng này
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string tenDangNhap, string matKhau, string? returnUrl = null)
        {
            var account = await _context.Accounts
                .Include(a => a.MaDoiTuongNavigation) // Include KhachHang
                .FirstOrDefaultAsync(a => a.TenDangNhap == tenDangNhap);

            if (account == null || !VerifyPassword(matKhau, account.MatKhau))
            {
                ViewBag.Error = "Sai tên đăng nhập hoặc mật khẩu!";
                return View();
            }
            string hoTen = account.MaDoiTuongNavigation?.HoTen ?? account.TenDangNhap;

            // Tạo claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, hoTen),
                new Claim(ClaimTypes.Role, account.VaiTro),
                new Claim("MaKh", account.MaDoiTuong ?? ""),
                new Claim("TenDangNhap", account.TenDangNhap),
                new Claim("Avatar", account.MaDoiTuongNavigation?.Avatar ?? "")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = false 
                });
            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            // Điều hướng
            if (account.VaiTro == "Admin")
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            else
                return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public async Task<IActionResult> Register(string hoTen, string soDienThoai, string email, string matKhau, DateOnly? ngaySinh)
        {
            bool exists = await _context.Accounts
                .AnyAsync(a => a.TenDangNhap == email || a.TenDangNhap == soDienThoai);
            if (exists)
            {
                ViewBag.RegError = "Email hoặc số điện thoại đã tồn tại.";
                return View("Login");
            }

            string maKH = "KH" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            while (await _context.KhachHangs.AnyAsync(kh => kh.MaKh == maKH))
            {
                maKH = "KH" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            }

            var kh = new KhachHang
            {
                MaKh = maKH,
                HoTen = hoTen,
                SoDienThoai = soDienThoai,
                Email = email,
                NgaySinh = ngaySinh,
                NgayTao = DateTime.Now
            };

            var account = new Account
            {
                TenDangNhap = email,
                MatKhau = HashPassword(matKhau), 
                MaDoiTuong = maKH,
                VaiTro = "KhachHang",
                TrangThai = "Active"
            };

            _context.KhachHangs.Add(kh);
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            ViewBag.RegSuccess = "Đăng ký thành công! Vui lòng đăng nhập.";
            return View("Login");
        }

        // -------------------- [ Logout ] --------------------
        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var kh = await GetCurrentKhachHangDataAsync();
            if (kh == null)
                return RedirectToAction("Login");

            return View(kh);
        }

        [HttpGet]
        [Authorize(Roles = "KhachHang")]
        public async Task<IActionResult> LoadPartial(string actionName)
        {
            var khachHang = await GetCurrentKhachHangDataAsync();

            if (khachHang == null)
            {
                return Unauthorized(); // Hoặc chuyển hướng về trang Login
            }

            return actionName switch
            {
                "Dashboard" => PartialView("_ThongTinChung", khachHang),
                "EditProfile" => PartialView("_ChiTietTaiKhoan", khachHang),
                "ChangePassword" => PartialView("_ChangePassword", new ChangePasswordViewModel()),
                "OrderHistory" => PartialView("_LichSuGiaoDich", khachHang), 
                _ => NotFound(),
            };
        }

        [HttpPost]
        [Authorize(Roles = "KhachHang")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(KhachHang model, IFormFile? AvatarFile)
        {
            var currentMaKh = User.FindFirstValue("MaKh");
            if (model.MaKh != currentMaKh)
            {
                return Unauthorized();
            }
            ModelState.Remove("Email"); 
            ModelState.Remove("NgayTao");

            if (ModelState.IsValid)
            {
                var kh = await _context.KhachHangs.FirstOrDefaultAsync(k => k.MaKh == model.MaKh);
                if (kh == null)
                {
                    ModelState.AddModelError("", "Không tìm thấy thông tin khách hàng.");
                    return PartialView("_ChiTietTaiKhoan", model);
                }

                if (AvatarFile != null)
                {
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images/user");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(AvatarFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await AvatarFile.CopyToAsync(fileStream);
                    }

                    if (!string.IsNullOrEmpty(kh.Avatar) && kh.Avatar != "/images/no-image.jpg")
                    {
                        var oldFilePath = Path.Combine(_hostEnvironment.WebRootPath, kh.Avatar.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }
                    kh.Avatar =  uniqueFileName;
                }

                kh.HoTen = model.HoTen;
                kh.SoDienThoai = model.SoDienThoai;
                kh.NgaySinh = model.NgaySinh;

                await _context.SaveChangesAsync();

                var newClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, kh.HoTen),
                    new Claim(ClaimTypes.Role, User.FindFirstValue(ClaimTypes.Role)!),
                    new Claim("MaKh", kh.MaKh),
                    new Claim("TenDangNhap", kh.Email),
                };
                var newIdentity = new ClaimsIdentity(newClaims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(newIdentity));

                ViewBag.UpdateSuccess = "Cập nhật thông tin thành công!";
                return PartialView("_ThongTinChung", kh);
            }

            return PartialView("_ChiTietTaiKhoan", model);
        }

        [HttpPost]
        [Authorize(Roles = "KhachHang")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var tenDangNhap = User.FindFirstValue("TenDangNhap");
                var account = await _context.Accounts.FirstOrDefaultAsync(a => a.TenDangNhap == tenDangNhap);

                if (account == null)
                {
                    ViewBag.ErrorMessage = "Không tìm thấy tài khoản.";
                    return PartialView("_ChangePassword", model);
                }
                if (!VerifyPassword(model.OldPassword, account.MatKhau))
                {
                    ModelState.AddModelError("OldPassword", "Mật khẩu cũ không chính xác.");
                    return PartialView("_ChangePassword", model);
                }
                account.MatKhau = HashPassword(model.NewPassword);
                _context.Accounts.Update(account);
                await _context.SaveChangesAsync();

                ViewBag.SuccessMessage = "Đổi mật khẩu thành công!";
                ModelState.Clear();
                return PartialView("_ChangePassword", new ChangePasswordViewModel { NewPassword = "", ConfirmPassword = "", OldPassword = "" });
            }

            return PartialView("_ChangePassword", model);
        }

        [HttpGet]
        public IActionResult CheckLoginStatus()
        {
            bool isLoggedIn = User.Identity != null && User.Identity.IsAuthenticated;
            return Json(isLoggedIn);
        }

    }
}
