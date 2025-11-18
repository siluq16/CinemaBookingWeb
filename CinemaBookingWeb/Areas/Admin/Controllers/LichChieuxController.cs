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
    [Route("Admin/LichChieu")]
    public class LichChieuxController : Controller
    {
        private readonly CinemaBookingWebContext _context;

        public LichChieuxController(CinemaBookingWebContext context)
        {
            _context = context;
        }

        // GET: Admin/LichChieux
        [Route("DanhSach")]
        public async Task<IActionResult> Index()
        {
            var lichChieux = await _context.LichChieus
                .Include(l => l.MaPhimNavigation)
                .Include(l => l.MaPhongNavigation)
                .OrderByDescending(l => l.NgayChieu)
                .ThenBy(l => l.GioBatDau)
                .ToListAsync();

            ViewBag.PhimList = await _context.Phims.ToListAsync();
            ViewBag.PhongList = await _context.PhongChieus.ToListAsync();

            return View(lichChieux);
        }
        [HttpPost]
        [Route("DeleteAjax")]
        public async Task<JsonResult> DeleteAjax([FromBody] string id)
        {
            var lichChieu = await _context.LichChieus.FindAsync(id);
            if (lichChieu != null)
            {
                _context.LichChieus.Remove(lichChieu);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Đã xóa lịch chiếu." });
            }
            return Json(new { success = false, message = "Không tìm thấy lịch chiếu." });
        }

        [HttpPost]
        [Route("SaveAjax")]
        public async Task<JsonResult> SaveAjax([FromBody] LichChieuDto model)
        {
            try
            {
                var lich = new LichChieu
                {
                    MaLich = string.IsNullOrEmpty(model.MaLich) ? Guid.NewGuid().ToString() : model.MaLich,
                    MaPhim = model.MaPhim,
                    MaPhong = model.MaPhong,
                    NgayChieu = DateOnly.Parse(model.NgayChieu),
                    GioBatDau = TimeOnly.Parse(model.GioBatDau),
                    GioKetThuc = TimeOnly.Parse(model.GioKetThuc)
                };
                var newStart = lich.NgayChieu.ToDateTime(lich.GioBatDau);
                var newEnd = (lich.GioKetThuc >= lich.GioBatDau)
                    ? lich.NgayChieu.ToDateTime(lich.GioKetThuc)
                    : lich.NgayChieu.AddDays(1).ToDateTime(lich.GioKetThuc);
                var conflict = await _context.LichChieus
                    .Where(x => x.MaPhong == lich.MaPhong && x.MaLich != lich.MaLich)
                    .ToListAsync();

                foreach (var c in conflict)
                {
                    var oldStart = c.NgayChieu.ToDateTime(c.GioBatDau);
                    var oldEnd = (c.GioKetThuc >= c.GioBatDau)
                        ? c.NgayChieu.ToDateTime(c.GioKetThuc)
                        : c.NgayChieu.AddDays(1).ToDateTime(c.GioKetThuc);

                    if (newStart < oldEnd && newEnd > oldStart)
                    {
                        return Json(new
                        {
                            success = false,
                            message = $"⚠ Trùng giờ với suất: {c.GioBatDau} - {c.GioKetThuc} (Ngày {c.NgayChieu})"
                        });
                    }
                }
                var allShows = await _context.LichChieus
                    .Where(x => x.MaPhong == lich.MaPhong && x.MaLich != lich.MaLich)
                    .ToListAsync();

                var timeline = allShows
                    .Select(x => new
                    {
                        Start = x.NgayChieu.ToDateTime(x.GioBatDau),
                        End = (x.GioKetThuc >= x.GioBatDau)
                                ? x.NgayChieu.ToDateTime(x.GioKetThuc)
                                : x.NgayChieu.AddDays(1).ToDateTime(x.GioKetThuc)
                    })
                    .OrderBy(x => x.Start)
                    .ToList();
                var previous = timeline.LastOrDefault(x => x.End <= newStart);
                if (previous != null)
                {
                    if (newStart - previous.End < TimeSpan.FromMinutes(15))
                    {
                        return Json(new
                        {
                            success = false,
                            message = "⚠ Suất mới phải cách suất trước ít nhất 15 phút!"
                        });
                    }
                }
                var next = timeline.FirstOrDefault(x => x.Start >= newEnd);

                if (next != null)
                {
                    if (next.Start - newEnd < TimeSpan.FromMinutes(15))
                    {
                        return Json(new
                        {
                            success = false,
                            message = "⚠ Suất mới phải cách suất sau ít nhất 15 phút!"
                        });
                    }
                }
                var existing = await _context.LichChieus.FindAsync(lich.MaLich);
                if (existing == null)
                {
                    _context.LichChieus.Add(lich);
                }
                else
                {
                    _context.Entry(existing).CurrentValues.SetValues(lich);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Lưu lịch chiếu thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        private bool LichChieuExists(string id)
        {
            return _context.LichChieus.Any(e => e.MaLich == id);
        }
    }
}
