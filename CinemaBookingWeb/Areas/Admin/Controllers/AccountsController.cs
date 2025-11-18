using CinemaBookingWeb.Areas.Admin.Data;
using CinemaBookingWeb.Areas.Admin.Models;
using CinemaBookingWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CinemaBookingWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    [Route("Admin/User")]
    public class AccountsController : Controller
    {
        private readonly CinemaBookingWebContext _context;
        private readonly FileUploadService _uploader;

        public AccountsController(CinemaBookingWebContext context)
        {
            _context = context;
            _uploader = new FileUploadService("user");
        }
        private string HashPassword(string password)
        {
            var passwordHasher = new PasswordHasher<object>();
            return passwordHasher.HashPassword(null, password);
        }

        // GET: Admin/Accounts
        [Route("DanhSach")]
        public async Task<IActionResult> Index()
        {
            ViewBag.CurrentUsername = User.Identity.Name; // Giả định Tên đăng nhập được lưu trong ClaimTypes.Name

            var allAccounts = await _context.Accounts
                .Include(a => a.MaDoiTuongNavigation) // Bao gồm thông tin Khách hàng
                .Select(a => new AccountViewModel
                {
                    TenDangNhap = a.TenDangNhap,
                    VaiTro = a.VaiTro,
                    TrangThai = a.TrangThai,
                    NgayTao = a.MaDoiTuongNavigation != null ? a.MaDoiTuongNavigation.NgayTao : a.MaDoiTuongNavigation.NgayTao, // Giữ nguyên logic cũ
                    HoTen = a.MaDoiTuongNavigation != null ? a.MaDoiTuongNavigation.HoTen : a.TenDangNhap, // Nếu không phải KH, dùng TenDangNhap làm Họ tên
                    Email = a.MaDoiTuongNavigation.Email,
                    SoDienThoai = a.MaDoiTuongNavigation.SoDienThoai,
                    Avatar = a.MaDoiTuongNavigation.Avatar,
                })
                .ToListAsync();

            return View(allAccounts);
        }
        [Route("TaoMoi")]
        public IActionResult Create()
        {
            ViewBag.VaiTroList = new List<string> { "KhachHang", "Admin" };
            return View();
        }

        // POST: Admin/Account/Create
        [Route("TaoMoi")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AccountCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.VaiTroList = new List<string> { "KhachHang", "Admin" };
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
                return View(model);
            }
            if (await _context.Accounts.AnyAsync(a => a.TenDangNhap == model.TenDangNhap))
            {
                TempData["ErrorMessage"] = "Tên đăng nhập đã tồn tại.";
                ViewBag.VaiTroList = new List<string> { "KhachHang", "Admin" };
                return View(model);
            }
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    string maDoiTuong = null;
                    string savedAvatar = null;
                    if (model.VaiTro == "KhachHang")
                    {
                        maDoiTuong = $"KH{DateTime.Now.Ticks.ToString().Substring(0, 8)}";
                        if (model.AvatarFile != null)
                        {
                            savedAvatar = await _uploader.SavePosterAsync(model.AvatarFile);
                        }
                        var kh = new KhachHang
                        {
                            MaKh = maDoiTuong,
                            HoTen = model.HoTen,
                            SoDienThoai = model.SoDienThoai,
                            Email = model.Email,
                            NgaySinh = model.NgaySinh,
                            Avatar = savedAvatar 
                        };
                        _context.KhachHangs.Add(kh);
                    }
                    var newAccount = new Account
                    {
                        TenDangNhap = model.TenDangNhap,
                        MatKhau = HashPassword(model.MatKhau),
                        MaDoiTuong = maDoiTuong,
                        VaiTro = model.VaiTro,
                        TrangThai = "Active"
                    };
                    _context.Accounts.Add(newAccount);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    TempData["SuccessMessage"] = $"Tạo tài khoản '{model.TenDangNhap}' ({model.VaiTro}) thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = $"Lỗi hệ thống khi tạo tài khoản: {ex.Message}";
                    ViewBag.VaiTroList = new List<string> { "KhachHang", "Admin" };
                    return View(model);
                }
            }
        }

        // GET: Admin/Account/Edit/{TenDangNhap}
        [Route("ChinhSua/{id}")]
        public async Task<IActionResult> Edit(string id) // id là TenDangNhap
        {
            if (id == null)
            {
                return NotFound();
            }

            // 1. Tìm Account bằng TenDangNhap
            var account = await _context.Accounts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.TenDangNhap == id);

            if (account == null)
            {
                return NotFound();
            }

            KhachHang khachHang = null;

            // 2. Nếu có MaDoiTuong, tìm hồ sơ Khách hàng
            if (!string.IsNullOrEmpty(account.MaDoiTuong))
            {
                khachHang = await _context.KhachHangs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(k => k.MaKh == account.MaDoiTuong);
            }

            var model = new AccountCreateViewModel
            {
                TenDangNhap = account.TenDangNhap,
                HoTen = khachHang?.HoTen,
                Email = khachHang?.Email,
                SoDienThoai = khachHang?.SoDienThoai,
                NgaySinh = khachHang?.NgaySinh,
                VaiTro = account.VaiTro
            };

            ViewData["MaKH"] = account.MaDoiTuong; // Có thể null cho Admin
            ViewData["CurrentAvatar"] = khachHang?.Avatar ?? "/images/no-avatar.jpg";

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, AccountCreateViewModel model) // id là TenDangNhap
        {
            if (id != model.TenDangNhap)
            {
                return NotFound();
            }

            string maKh = HttpContext.Request.Form["MaKH"]!;
            string currentAvatarPath = HttpContext.Request.Form["CurrentAvatar"]!;

            if (!ModelState.IsValid)
            {
                ViewData["MaKH"] = maKh;
                ViewData["CurrentAvatar"] = currentAvatarPath;
                return View(model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var account = await _context.Accounts.FindAsync(id); // id = TenDangNhap
                var khachHang = await _context.KhachHangs.FindAsync(maKh); // MaKH chỉ tồn tại nếu là KhachHang

                if (account == null) throw new Exception("Không tìm thấy tài khoản.");

                string? oldAvatarFileName = Path.GetFileName(currentAvatarPath);
                string newAvatarNameInDb = currentAvatarPath; // Giữ nguyên path DB nếu không upload

                if (model.AvatarFile != null)
                {
                    string? savedFileName = await _uploader.SavePosterAsync(model.AvatarFile);

                    if (!string.IsNullOrEmpty(savedFileName))
                    {
                        newAvatarNameInDb = savedFileName;

                        if (khachHang != null && !string.IsNullOrEmpty(oldAvatarFileName) && currentAvatarPath != "/images/no-avatar.jpg")
                        {
                            _uploader.DeletePoster(oldAvatarFileName);
                        }
                    }
                }

                if (khachHang != null)
                {
                    khachHang.HoTen = model.HoTen;
                    khachHang.Email = model.Email;
                    khachHang.SoDienThoai = model.SoDienThoai;
                    khachHang.NgaySinh = model.NgaySinh; // DateOnly?

                    khachHang.Avatar = newAvatarNameInDb;

                    _context.Update(khachHang);
                }

                await _context.SaveChangesAsync();
                transaction.Commit();

                TempData["SuccessMessage"] = "Cập nhật tài khoản thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                ModelState.AddModelError("", $"Lỗi hệ thống khi cập nhật hồ sơ: {ex.Message}");

                ViewData["MaKH"] = maKh;
                ViewData["CurrentAvatar"] = currentAvatarPath;
                return View(model);
            }
        }

        // --------------------------------------------------------------------------------------

        [HttpPost("ResetPassword/{id}")]
        public async Task<IActionResult> ResetPassword(string id) // id là TenDangNhap
        {
            var account = await _context.Accounts.FindAsync(id);

            if (account == null)
            {
                return Json(new { success = false, message = "Không tìm thấy tài khoản để reset." });
            }

            try
            {
                const string defaultPassword = "123456";

                account.MatKhau = HashPassword(defaultPassword); // <--- ĐÃ SỬA VÀ BẢO MẬT

                _context.Update(account);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Đặt lại mật khẩu thành công. Mật khẩu mới đã được băm an toàn." });
            }
            catch (Exception)
            {
                // Ghi log lỗi
                return Json(new { success = false, message = "Lỗi hệ thống khi reset mật khẩu." });
            }
        }
        [HttpPost("LockUser/{id}")]
        public async Task<IActionResult> LockUser(string id) 
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.TenDangNhap == id);
            if (account == null)
                return Json(new { success = false, message = "Không tìm thấy tài khoản." });
            if (account.TrangThai == "Locked")
                return Json(new { success = false, message = "Tài khoản này đã bị khóa." });

            if (account.VaiTro == "Admin" && User.Identity.Name == id)
                return Json(new { success = false, message = "Không thể tự khóa tài khoản Admin đang hoạt động." });

            account.TrangThai = "Locked";
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"Khóa tài khoản '{id}' thành công." });
        }

        [HttpPost("UnLockUser/{id}")]
        public async Task<IActionResult> UnlockUser(string id) 
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.TenDangNhap == id);
            if (account == null)
                return Json(new { success = false, message = "Không tìm thấy tài khoản." });

            account.TrangThai = "Active";
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"Mở khóa tài khoản '{id}' thành công." });
        }

        private bool AccountExists(string id)
        {
            return _context.Accounts.Any(e => e.TenDangNhap == id);
        }
    }
}
