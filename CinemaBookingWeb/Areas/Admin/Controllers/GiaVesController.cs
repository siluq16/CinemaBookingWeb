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
    [Route("Admin/GiaVe")]
    public class GiaVesController : Controller
    {
        private readonly CinemaBookingWebContext _context;

        public GiaVesController(CinemaBookingWebContext context)
        {
            _context = context;
        }

        // GET: Admin/GiaVes
        [Route("DanhSach")]
        public async Task<IActionResult> Index()
        {
            var giaVes = await _context.GiaVes.ToListAsync();
            return View(giaVes);
        }

        // POST: /Admin/GiaVes/SaveAjax
        [HttpPost]
        [Route("SaveAjax")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAjax([FromBody] GiaVe model)
        {
            // Kiểm tra tính hợp lệ của Model (tên trường phải khớp với tên trong JS)
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            try
            {
                var existingPrice = await _context.GiaVes
                    .FirstOrDefaultAsync(g => g.MaGia == model.MaGia);

                if (existingPrice == null)
                {
                    // --- Xử lý TẠO MỚI ---
                    // Kiểm tra trùng lặp theo cặp LoaiGhe và LoaiNgay (phòng trường hợp client bỏ qua check)
                    var duplicateCheck = await _context.GiaVes
                        .AnyAsync(g => g.LoaiGhe == model.LoaiGhe && g.LoaiNgay == model.LoaiNgay);

                    if (duplicateCheck)
                    {
                        return Json(new { success = false, message = "Đã tồn tại giá vé cho loại ghế và loại ngày này." });
                    }

                    // Gán giá trị mặc định cho MaGia (đã được tạo ở client, chỉ cần đảm bảo)
                    // Lưu ý: Nếu MaGia là Primary Key TỰ ĐỘNG TĂNG, bạn phải bỏ logic này ở client và server phải tự tạo
                    // Hiện tại, MaGia được tạo từ cặp:
                    // model.MaGia = $"{model.LoaiGhe}_{model.LoaiNgay}"; 

                    _context.Add(model);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Tạo giá vé thành công." });
                }
                else
                {
                    // --- Xử lý CẬP NHẬT (SỬA) ---
                    // KHÔNG cho phép sửa LoaiGhe và LoaiNgay (theo logic JS)
                    existingPrice.Gia = model.Gia;

                    _context.Update(existingPrice);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Cập nhật giá vé thành công." });
                }
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết (trong môi trường thực tế)
                return Json(new { success = false, message = $"Lỗi khi lưu: {ex.Message}" });
            }
        }

        // POST: /Admin/GiaVes/DeleteAjax
        [HttpPost]
        [Route("DeleteAjax")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAjax([FromBody] string id) // id là MaGia
        {
            if (string.IsNullOrEmpty(id))
            {
                return Json(new { success = false, message = "Mã giá vé không được để trống." });
            }

            try
            {
                var giaVe = await _context.GiaVes.FindAsync(id);

                if (giaVe == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy giá vé cần xóa." });
                }

                // Kiểm tra ràng buộc (nếu có, ví dụ: giá vé này đang được sử dụng trong lịch chiếu)
                // if (_context.LichChieu.Any(lc => lc.MaGia == id)) { ... return Json(new { ... }); }

                _context.GiaVes.Remove(giaVe);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Xóa giá vé '{id}' thành công." });
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết
                return Json(new { success = false, message = $"Lỗi khi xóa: {ex.Message}" });
            }
        }

        private bool GiaVeExists(string id)
        {
            return _context.GiaVes.Any(e => e.MaGia == id);
        }
    }
}
