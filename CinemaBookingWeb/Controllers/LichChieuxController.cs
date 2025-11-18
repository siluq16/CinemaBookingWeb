using CinemaBookingWeb.Models;
using CinemaBookingWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaBookingWeb.Controllers
{
    public class LichChieuxController : Controller
    {
        private readonly CinemaBookingWebContext _context;
        public LichChieuxController(CinemaBookingWebContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> XemLichChieu(string maPhim)
        {
            var lichTheoNgay = await _context.LichChieus
                .Include(l => l.MaPhimNavigation)
                .Include(l => l.MaPhongNavigation)
                .Where(l => l.MaPhim == maPhim)
                .ToListAsync();

            var today = DateOnly.FromDateTime(DateTime.Today);

            // 7 ngày tiếp theo (kể cả hôm nay)
            var next7Days = Enumerable.Range(0, 7)
                .Select(i => today.AddDays(i))
                .ToList();

            var vm = new LichChieuViewModel
            {
                MaPhim = maPhim,
                TenPhim = lichTheoNgay.FirstOrDefault()?.MaPhimNavigation.TenPhim,
                NgayChieuKhac = next7Days
            };
            return PartialView("_PopupLichChieu", vm);
        }

        // Gọi khi chọn ngày
        public async Task<IActionResult> LichTheoNgay(string maPhim, DateOnly ngay)
        {
            var lich = await _context.LichChieus
                .Include(l => l.MaPhongNavigation)
                .Where(l => l.MaPhim == maPhim && l.NgayChieu == ngay)
                .ToListAsync();

            var lichItems = lich.Select(l => new LichChieuItem
            {
                MaLich = l.MaLich,
                TenPhong = l.MaPhong,
                NgayChieu = l.NgayChieu,
                GioBatDau = l.GioBatDau,
                GioKetThuc = l.GioKetThuc,
                DinhDang = "2D"
            }).ToList();

            return PartialView("_LichTheoNgayPartial", lichItems);
        }
    }

}
    
