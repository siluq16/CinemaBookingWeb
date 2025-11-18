using CinemaBookingWeb.Areas.Admin.Data;
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
    [Route("Admin/ComBo")]
    public class DoAnsController : Controller
    {
        private readonly CinemaBookingWebContext _context;
        private readonly FileUploadService _uploader;

        public DoAnsController(CinemaBookingWebContext context)
        {
            _context = context;
            _uploader = new FileUploadService("combo");
        }

        // GET: Admin/DoAns
        [Route("DanhSach")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.DoAns.ToListAsync());
        }

        [HttpGet]
        [Route("GetDoAnById/{id}")]
        public async Task<IActionResult> GetDoAnById(string id)
        {
            var doAn = await _context.DoAns.FirstOrDefaultAsync(d => d.MaDoAn == id);
            if (doAn == null)
            {
                return NotFound();
            }
            return Json(doAn);
        }

        // POST: /Admin/DoAn/Create
        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] DoAn doAn, IFormFile ImageFile)
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(doAn.MaDoAn) || await _context.DoAns.AnyAsync(d => d.MaDoAn == doAn.MaDoAn))
            {
                return Json(new { success = false, message = "Mã Đồ ăn không hợp lệ hoặc đã tồn tại." });
            }

            string? savedFileName = await _uploader.SavePosterAsync(ImageFile);

            doAn.Anh = string.IsNullOrEmpty(savedFileName)
                       ? "/images/no-image.jpg" // Ảnh mặc định
                       : savedFileName;

            _context.Add(doAn);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Thêm Đồ ăn thành công." });
        }
        // POST: /Admin/DoAn/Update
        [HttpPost]
        [Route("Update")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update([FromForm] DoAn model, IFormFile? ImageFile)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var existing = await _context.DoAns.FirstOrDefaultAsync(d => d.MaDoAn == model.MaDoAn);
            if (existing == null)
                return Json(new { success = false, message = "Không tìm thấy Đồ ăn để cập nhật." });

            string? oldFileName = Path.GetFileName(existing.Anh);

            string? newFileName = oldFileName;
            bool imageUpdated = false;
            if (ImageFile != null && ImageFile.Length > 0)
            {
                string? tempNewFileName = await _uploader.SavePosterAsync(ImageFile);
                if (!string.IsNullOrEmpty(tempNewFileName))
                {
                    newFileName = tempNewFileName;
                    imageUpdated = true;
                    if (!string.IsNullOrEmpty(oldFileName) && existing.Anh != "/images/no-image.jpg")
                    {
                        _uploader.DeletePoster(oldFileName);
                    }
                }
            }

            if (imageUpdated)
            {
                existing.Anh = newFileName;
            }
            existing.TenDoAn = model.TenDoAn;
            existing.MoTa = model.MoTa;
            existing.DonGia = model.DonGia;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Cập nhật Đồ ăn thành công." });
        }

        // POST: /Admin/DoAn/Delete/{id}
        [HttpPost]
        [Route("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var doAn = await _context.DoAns.FindAsync(id);
            if (doAn == null)
                return Json(new { success = false, message = "Đồ ăn không tồn tại." });

            // Kiểm tra ràng buộc
            if (await _context.ChiTietDoAns.AnyAsync(ct => ct.MaDoAn == id))
            {
                return Json(new { success = false, message = "Không thể xóa vì Đồ ăn này đã được bán trong các Hóa đơn." });
            }

            // Trích xuất tên file và sử dụng dịch vụ để xóa
            string? fileNameToDelete = Path.GetFileName(doAn.Anh);
            _uploader.DeletePoster(fileNameToDelete);

            _context.DoAns.Remove(doAn);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Xóa Đồ ăn thành công." });
        }

        private bool DoAnExists(string id)
        {
            return _context.DoAns.Any(e => e.MaDoAn == id);
        }
    }
}
