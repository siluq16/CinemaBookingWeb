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
    [Route("Admin/Phim")]
    public class PhimsController : Controller
    {
        private readonly CinemaBookingWebContext _context;
        private readonly FileUploadService _uploader;

        public PhimsController(CinemaBookingWebContext context)
        {
            _context = context;
            _uploader = new FileUploadService();
        }

        // GET: Admin/Phims
        [Route("DanhSach")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Phims.ToListAsync());
        }

        // GET: Admin/Phims/Details/5
        [Route("ChiTiet/{id}")]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phim = await _context.Phims
                .FirstOrDefaultAsync(m => m.MaPhim == id);
            if (phim == null)
            {
                return NotFound();
            }

            return View(phim);
        }

        // GET: Admin/Phims/Create
        [Route("TaoMoi")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Phims/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("TaoMoi")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaPhim,TenPhim,TheLoai,DienVien,ThoiLuong,NgayKhoiChieu,NgayKetThuc,DaoDien,Trailer,Poster,MoTa,DoTuoi")] Phim phim, IFormFile PosterFile)
        {
            if (ModelState.IsValid)
            {
                if (PosterFile != null && PosterFile.Length > 0)
                    phim.Poster = await _uploader.SavePosterAsync(PosterFile);
                else
                    phim.Poster = "default.jpg";

                _context.Add(phim);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(phim);
        }

        // GET: Admin/Phims/Edit/5
        [Route("ChinhSua/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phim = await _context.Phims.FindAsync(id);
            if (phim == null)
            {
                return NotFound();
            }
            return View(phim);
        }

        // POST: Admin/Phims/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("ChinhSua/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaPhim,TenPhim,TheLoai,DienVien,ThoiLuong,NgayKhoiChieu,NgayKetThuc,DaoDien,Trailer,MoTa,DoTuoi")] Phim phim, IFormFile? NewPoster)
        {
            if (id != phim.MaPhim)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingPhim = await _context.Phims.FindAsync(id);

                if (existingPhim == null)
                    return NotFound();

                try
                {
                    string? oldPosterName = existingPhim.Poster;
                    string? newPosterName = oldPosterName;

                    if (NewPoster != null && NewPoster.Length > 0)
                    {
                        newPosterName = await _uploader.SavePosterAsync(NewPoster, oldPosterName);
                    }

                    _context.Entry(existingPhim).CurrentValues.SetValues(phim);
                    existingPhim.Poster = newPosterName;

                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi lưu dữ liệu hoặc upload file: " + ex.Message);
                }
            }
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine("❌ MODEL ERROR: " + error.ErrorMessage);
                }

                ModelState.AddModelError("", "Có lỗi xảy ra. Kiểm tra lại dữ liệu nhập vào.");
            }
            return View(phim);
        }

        // POST: Admin/Phims/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("Xoa/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var phim = await _context.Phims.FindAsync(id);
            if (phim != null)
            {
                if (!string.IsNullOrEmpty(phim.Poster))
                {
                    _uploader.DeletePoster(phim.Poster);
                }
                _context.Phims.Remove(phim);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PhimExists(string id)
        {
            return _context.Phims.Any(e => e.MaPhim == id);
        }
    }
}
