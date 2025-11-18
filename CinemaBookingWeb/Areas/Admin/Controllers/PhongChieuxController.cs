using CinemaBookingWeb.Areas.Admin.Models;
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
    [Route("Admin/PhongChieu")]
    public class PhongChieuxController : Controller
    {
        private readonly CinemaBookingWebContext _context;

        public PhongChieuxController(CinemaBookingWebContext context)
        {
            _context = context;
        }

        // GET: Admin/PhongChieux
        [Route("DanhSach")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.PhongChieus.ToListAsync());
        }

        // GET: Admin/PhongChieux/Create
        [Route("TaoMoi")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/PhongChieux/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("TaoMoi")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaPhong,TenPhong,SoHangGhe,SoGheMoiHang")] PhongChieu phongChieu)
        {
            if (ModelState.IsValid)
            {
                // Tính tổng ghế
                phongChieu.SoLuongGhe = phongChieu.SoHangGhe * phongChieu.SoGheMoiHang;

                // Tạo ghế tự động
                for (int r = 1; r <= phongChieu.SoHangGhe; r++)
                {
                    string hang = ((char)('A' + r - 1)).ToString();
                    for (int c = 1; c <= phongChieu.SoGheMoiHang; c++)
                    {
                        _context.GheNgois.Add(new GheNgoi
                        {
                            MaGhe = $"{phongChieu.MaPhong}_{hang}{c}",
                            MaPhong = phongChieu.MaPhong,
                            HangGhe = hang,
                            CotGhe = c,
                            LoaiGhe = "Normal"
                        });
                    }
                }

                _context.Add(phongChieu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(phongChieu);
        }
        [HttpGet]
        [Route("GetSeatLayout/{roomId}")]
        public async Task<IActionResult> GetSeatLayout(string roomId)
        {
            var gheList = await _context.GheNgois
            .Where(g => g.MaPhong == roomId)
            .ToListAsync();

            var model = new SeatLayoutViewModel
            {
                MaPhong = roomId,
                GheNgois = gheList
            };
            return PartialView("_SeatLayoutPartial", model);
        }

        [HttpPost]
        [Route("UpdateSeatRow")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> UpdateSeatRow([FromBody] UpdateSeatRowRequest request)
        {
            try
            {
                foreach (var seat in request.Seats)
                {
                    var entity = await _context.GheNgois.FindAsync(seat.MaGhe);
                    if (entity != null)
                    {
                        entity.LoaiGhe = seat.LoaiGhe;
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new
                {
                    success = true,
                    message = "Cập nhật ghế thành công!",
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Lỗi server khi cập nhật ghế: " + ex.Message
                });
            }
        }


        // GET: Admin/PhongChieux/Edit/5
        [Route("ChinhSua/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phongChieu = await _context.PhongChieus.FindAsync(id);
            if (phongChieu == null)
            {
                return NotFound();
            }
            return View(phongChieu);
        }

        // POST: Admin/PhongChieux/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("ChinhSua/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaPhong,TenPhong,SoHangGhe,SoGheMoiHang,SoLuongGhe")] PhongChieu phongChieu)
        {
            if (id != phongChieu.MaPhong)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
                return View(phongChieu);
            var existingRoom = await _context.PhongChieus
        .Include(p => p.GheNgois)
        .FirstOrDefaultAsync(p => p.MaPhong == phongChieu.MaPhong);

            if (existingRoom == null)
                return NotFound();

            existingRoom.TenPhong = phongChieu.TenPhong;
            int newRows = phongChieu.SoHangGhe;
            int newCols = phongChieu.SoGheMoiHang;

            var seatsToRemove = existingRoom.GheNgois
                .Where(g => (g.HangGhe[0] - 'A' + 1) > newRows || g.CotGhe > newCols)
                .Where(g => !g.Ves.Any()) // chỉ xóa ghế chưa có vé
                .ToList();

            _context.GheNgois.RemoveRange(seatsToRemove);

            for (int r = 1; r <= newRows; r++)
            {
                string rowName = ((char)('A' + r - 1)).ToString();
                for (int c = 1; c <= newCols; c++)
                {
                    if (!existingRoom.GheNgois.Any(s => s.HangGhe == rowName && s.CotGhe == c))
                    {
                        _context.GheNgois.Add(new GheNgoi
                        {
                            MaGhe = $"{existingRoom.MaPhong}_{rowName}{c}",
                            MaPhong = existingRoom.MaPhong,
                            HangGhe = rowName,
                            CotGhe = c,
                            LoaiGhe = "Normal"
                        });
                    }
                }
            }

            existingRoom.SoHangGhe = newRows;
            existingRoom.SoGheMoiHang = newCols;
            existingRoom.SoLuongGhe = existingRoom.GheNgois.Count + (newRows * newCols - existingRoom.GheNgois.Count);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PhongChieuExists(phongChieu.MaPhong))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/PhongChieux/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("Xoa/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var phongChieu = await _context.PhongChieus
                .Include(p => p.GheNgois) // load ghế liên quan
                .FirstOrDefaultAsync(p => p.MaPhong == id);

            if (phongChieu == null)
                return RedirectToAction(nameof(Index));
            bool hasSchedules = await _context.LichChieus.AnyAsync(l => l.MaPhong == id);
            if (hasSchedules)
            {
                TempData["ErrorMessage"] = "Phòng chiếu này đang có lịch chiếu, không thể xóa!";
                return RedirectToAction(nameof(Index));
            }

            // Xóa ghế
            if (phongChieu.GheNgois != null && phongChieu.GheNgois.Any())
                _context.GheNgois.RemoveRange(phongChieu.GheNgois);

            _context.PhongChieus.Remove(phongChieu);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool PhongChieuExists(string id)
        {
            return _context.PhongChieus.Any(e => e.MaPhong == id);
        }
    }
}
