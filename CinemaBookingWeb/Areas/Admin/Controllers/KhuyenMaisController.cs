using CinemaBookingWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CinemaBookingWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    [Route("Admin/KhuyenMai")]
    public class KhuyenMaisController : Controller
    {
        private readonly CinemaBookingWebContext _context;

        public KhuyenMaisController(CinemaBookingWebContext context)
        {
            _context = context;
        }

        // GET: Admin/KhuyenMais
        [Route("DanhSach")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.KhuyenMais.ToListAsync());
        }

        [HttpPost]
        [Route("SaveAjax")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAjax([FromBody] KhuyenMai model)
        {
            if (ModelState.IsValid)
            {
                // Chuyển đổi NgayBatDau/NgayKetThuc từ string sang DateOnly (nếu model binding không tự động làm)
                // Lưu ý: Do DateOnly là kiểu dữ liệu mới, việc binding có thể cần tùy chỉnh.
                // Trong trường hợp này, giả định model.NgayBatDau và model.NgayKetThuc đã được bind đúng từ chuỗi YYYY-MM-DD.

                var existingKm = await _context.KhuyenMais.FindAsync(model.MaKm);

                if (existingKm == null)
                {
                    // Tạo mới
                    if (await _context.KhuyenMais.AnyAsync(k => k.MaKm == model.MaKm))
                    {
                        return Json(new { success = false, message = "Mã khuyến mãi này đã tồn tại." });
                    }

                    _context.Add(model);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Tạo khuyến mãi thành công." });
                }
                else
                {
                    // Cập nhật
                    existingKm.TenKm = model.TenKm;
                    existingKm.MoTa = model.MoTa;
                    existingKm.PhanTramGiam = model.PhanTramGiam;
                    existingKm.SoTienGiam = model.SoTienGiam;
                    existingKm.NgayBatDau = model.NgayBatDau;
                    existingKm.NgayKetThuc = model.NgayKetThuc;

                    _context.Update(existingKm);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Cập nhật khuyến mãi thành công." });
                }
            }
            return Json(new { success = false, message = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại các trường bắt buộc và quy tắc giảm giá (chỉ được nhập 1 loại)." });
        }

        // POST: /Admin/KhuyenMais/DeleteAjax
        [HttpPost]
        [Route("DeleteAjax")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAjax([FromBody] string maKm)
        {
            if (string.IsNullOrEmpty(maKm))
            {
                return Json(new { success = false, message = "Mã khuyến mãi không hợp lệ." });
            }

            var khuyenMai = await _context.KhuyenMais.FindAsync(maKm);
            if (khuyenMai == null)
            {
                return Json(new { success = false, message = "Khuyến mãi không tồn tại." });
            }

            // Cần kiểm tra ràng buộc: Nếu khuyến mãi đã được sử dụng trong hóa đơn thì không cho xóa
            if (await _context.HoaDons.AnyAsync(h => h.MaKm == maKm))
            {
                return Json(new { success = false, message = "Không thể xóa khuyến mãi này vì đã có hóa đơn sử dụng." });
            }

            _context.KhuyenMais.Remove(khuyenMai);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Xóa khuyến mãi thành công." });
        }

        private bool KhuyenMaiExists(string id)
        {
            return _context.KhuyenMais.Any(e => e.MaKm == id);
        }
    }
}
