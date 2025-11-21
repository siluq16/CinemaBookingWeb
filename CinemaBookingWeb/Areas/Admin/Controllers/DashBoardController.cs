using CinemaBookingWeb.Areas.Admin.Models;
using CinemaBookingWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaBookingWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    [Route("Admin/DashBoard")]
    public class DashBoardController : Controller
    {
        private readonly CinemaBookingWebContext _context;
        public DashBoardController(CinemaBookingWebContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            // === Các chỉ số tổng quan ===
            var now = DateTime.Now;
            var today = now.Date;
            int thisday = DateTime.Now.Day;
            var thisMonth = DateTime.Now.Month;
            var thisYear = DateTime.Now.Year;
            var firstDayOfMonth = new DateTime(thisYear, thisMonth, 1);
            var firstDayOfNextMonth = firstDayOfMonth.AddMonths(1);
            var doanhThuPhimThang = _context.Ves
                .Include(v => v.MaLichNavigation)
                    .ThenInclude(lc => lc.MaPhimNavigation)
                .Include(v => v.MaHdNavigation)
                .Where(v => v.MaHdNavigation != null
                         && v.MaHdNavigation.TongTien > 0
                         && v.MaHdNavigation.NgayLap >= firstDayOfMonth
                         && v.MaHdNavigation.NgayLap <= firstDayOfNextMonth)
                .GroupBy(v => v.MaLichNavigation!.MaPhimNavigation!.TenPhim)
                .Select(g => new DoanhThuPhimVM
                {
                    TenPhim = g.Key ?? "Không xác định",
                    TongDoanhThu = g.Sum(v => (decimal?)v.GiaVe) ?? 0
                })
                .OrderByDescending(x => x.TongDoanhThu)
                .ToList();

            var doanhThuTheoThang = _context.HoaDons
                .Where(h => h.TongTien > 0 && h.NgayLap.Year == now.Year)
                .GroupBy(h => h.NgayLap.Month)
                .Select(g => new
                {
                    ThangSo = g.Key,
                    TongTien = g.Sum(h => (decimal?)h.TongTien) ?? 0
                })
                .OrderBy(g => g.ThangSo) // Sắp xếp theo số (1, 2... 10) trước khi đổi thành chữ
                .ToList() // Tải về RAM rồi mới map sang ViewModel
                .Select(x => new DoanhThuThangVM
                {
                    Thang = "Tháng " + x.ThangSo,
                    TongDoanhThu = x.TongTien
                })
                .ToList();

            // === Doanh thu trong ngày ===
            var endOfDay = today.AddDays(1);

            var doanhThuNgay = _context.HoaDons
                .Where(hd => hd.NgayLap >= today && hd.NgayLap < endOfDay)
                .Sum(hd => (decimal?)hd.TongTien) ?? 0;


            // === Tổng doanh thu toàn thời gian ===
            var tongDoanhThu = _context.HoaDons
                .Where(hd => hd.NgayLap >= firstDayOfMonth && hd.NgayLap < firstDayOfNextMonth) 
                .Sum(hd => (decimal?)hd.TongTien) ?? 0;

            // === Tổng khách hàng mới trong tháng ===
            var khachHangMoi = _context.KhachHangs
                .Count(kh => kh.NgayTao >= firstDayOfMonth && kh.NgayTao < firstDayOfNextMonth);


            // === Tổng vé bán ra trong tháng ===
            var veBanRa = _context.Ves
                .Include(v => v.MaHdNavigation)
                .Count(v => v.MaHdNavigation != null
                         && v.MaHdNavigation.NgayLap >= firstDayOfMonth
                         && v.MaHdNavigation.NgayLap < firstDayOfNextMonth);
            // === Gộp tất cả vào ViewModel ===
            var model = new DashBoardViewModel
            {
                Ngay = thisday,
                Thang = thisMonth,
                Nam = thisYear,
                DoanhThuNgay = doanhThuNgay,
                TongDoanhThu = tongDoanhThu,
                KhachHangMoi = khachHangMoi,
                VeBanRa = veBanRa,
                DoanhThuPhim = doanhThuPhimThang,
                DoanhThuPhong = doanhThuTheoThang
            };

            return View(model);
        }
        [Route("DoanhThuTheoPhim")]
        public IActionResult RevenueByMovie(DateTime? startDate, DateTime? endDate)
        {
            var today = DateTime.Now;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            var firstDayOfNextMonth = firstDayOfMonth.AddMonths(1);

            var start = startDate ?? firstDayOfMonth;
            var end = endDate ?? firstDayOfNextMonth;
            ViewBag.StartDate = start.ToString("yyyy-MM-dd");
            ViewBag.EndDate = end.ToString("yyyy-MM-dd");

            var doanhThuPhim = _context.Ves
                .Include(v => v.MaLichNavigation)
                    .ThenInclude(lc => lc.MaPhimNavigation)
                .Include(v => v.MaHdNavigation)
                .Where(v => v.MaHdNavigation != null
                         && v.MaHdNavigation.TongTien > 0
                         && v.MaHdNavigation.NgayLap >= start
                         && v.MaHdNavigation.NgayLap < end)
                .GroupBy(v => v.MaLichNavigation!.MaPhimNavigation!.TenPhim)
                .Select(g => new DoanhThuChiTietPhim
                {
                    TenPhim = g.Key ?? "Không xác định",
                    TongDoanhThu = g.Sum(v => (decimal?)v.GiaVe) ?? 0,
                    TongVeBanRa = g.Count()
                })
                .OrderByDescending(x => x.TongDoanhThu)
                .ToList();

            var model = new DashBoardViewModel
            {
                DoanhThuChiTietPhim = doanhThuPhim
            };

            return View(model);
        }

    }
}
