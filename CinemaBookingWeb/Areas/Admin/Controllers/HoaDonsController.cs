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
    [Route("Admin/DonHang")]
    public class HoaDonsController : Controller
    {
        private readonly CinemaBookingWebContext _context;

        public HoaDonsController(CinemaBookingWebContext context)
        {
            _context = context;
        }

        // GET: Admin/HoaDons
        [Route("DanhSach")]
        public async Task<IActionResult> Index()
        {
            var hoaDonDetails = await _context.HoaDons
                .SelectMany(hd => _context.Ves
                    .Where(v => v.MaHd == hd.MaHd)
                    .Take(1), // Chỉ lấy 1 vé/suất chiếu đại diện cho mỗi hóa đơn
                    (hd, v) => new { HoaDon = hd, Ve = v }
                )
                .GroupJoin(
                    _context.LichChieus.Include(lc => lc.MaPhimNavigation).Include(lc => lc.MaPhongNavigation),
                    combined => combined.Ve.MaLich,
                    lc => lc.MaLich,
                    (combined, lichChieuGroup) => new { combined, LichChieu = lichChieuGroup.FirstOrDefault() }
                )
                .Select(result => new HoaDonChiTietVM
                {
                    MaHD = result.combined.HoaDon.MaHd,
                    TrangThai = result.combined.HoaDon.TrangThai,
                    TongTien = result.combined.HoaDon.TongTien,
                    NgayLap = result.combined.HoaDon.NgayLap,

                    // Lấy thông tin từ LichChieu (có thể null nếu Vé không hợp lệ)
                    TenPhim = result.LichChieu!.MaPhimNavigation!.TenPhim,
                    PhongChieu = result.LichChieu.MaPhongNavigation!.TenPhong,
                    SuatChieu = $"{result.LichChieu.GioBatDau.ToString(@"hh\:mm")} - {result.LichChieu.GioKetThuc.ToString(@"hh\:mm")}"
                })
                .ToListAsync();

            return View(hoaDonDetails);
        }

        // GET: Admin/HoaDons/Details/5
        [Route("ChiTiet/{id}")]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var hoaDon = await _context.HoaDons
                .Include(hd => hd.MaKhNavigation)      // Khách hàng
                .Include(hd => hd.MaKmNavigation)      // Khuyến mãi
                .Include(hd => hd.Ves)                 // Vé
                .ThenInclude(v => v.MaLichNavigation)  // Lịch chiếu
                .ThenInclude(lc => lc.MaPhimNavigation) // Phim
                .Include(hd => hd.Ves)
                .ThenInclude(v => v.MaLichNavigation)
                .ThenInclude(lc => lc.MaPhongNavigation) // Phòng chiếu
                .Include(hd => hd.Ves)
                .ThenInclude(v => v.MaGheNavigation)     // Ghế
                .Include(hd => hd.ChiTietDoAns)          // Chi tiết đồ ăn
                .ThenInclude(ctda => ctda.MaDoAnNavigation) // Đồ ăn
                .FirstOrDefaultAsync(hd => hd.MaHd == id);

            if (hoaDon == null) return NotFound();

            var firstVe = hoaDon.Ves.FirstOrDefault();
            var lichChieu = firstVe?.MaLichNavigation;

            var model = new HoaDonChiTietVM
            {
                MaHD = hoaDon.MaHd,
                TrangThai = hoaDon.TrangThai,
                PhuongThucThanhToan = hoaDon.PhuongThucThanhToan ?? "N/A",
                TongTien = hoaDon.TongTien,
                NgayLap = hoaDon.NgayLap,

                TenKhachHang = hoaDon.MaKhNavigation?.HoTen ?? "Khách vãng lai",
                Email = hoaDon.MaKhNavigation?.Email ?? "N/A",
                SoDienThoai = hoaDon.MaKhNavigation?.SoDienThoai ?? "N/A",

                TenPhim = lichChieu?.MaPhimNavigation?.TenPhim ?? "Không xác định",
                PhongChieu = lichChieu?.MaPhongNavigation?.TenPhong ?? "N/A",
                GioBatDau = lichChieu?.GioBatDau.ToString(@"hh\:mm") ?? "N/A",
                GioKetThuc = lichChieu?.GioKetThuc.ToString(@"hh\:mm") ?? "N/A",
                NgayChieu = lichChieu?.NgayChieu ?? new DateOnly(),

                DanhSachGhe = hoaDon.Ves.Select(v => new VeChiTiet
                {
                    TenGhe = $"{v.MaGheNavigation.HangGhe}{v.MaGheNavigation.CotGhe}",
                    LoaiGhe = v.MaGheNavigation.LoaiGhe,
                    GiaTien = v.GiaVe
                }).ToList(),

                DanhSachDichVu = hoaDon.ChiTietDoAns.Select(ctda => new DichVuChiTiet
                {
                    TenDichVu = ctda.MaDoAnNavigation.TenDoAn,
                    SoLuong = ctda.SoLuong,
                    DonGia = ctda.MaDoAnNavigation.DonGia,
                    TongTien = ctda.ThanhTien
                }).ToList(),

                ThanhTienTruocGiam = hoaDon.TongTien, 
                GiamGia = hoaDon.MaKmNavigation?.PhanTramGiam ?? 0
            };

            // Tính ThanhTienTruocGiam (Tổng tiền vé + đồ ăn)
            decimal tongTienHang = model.DanhSachGhe.Sum(g => g.GiaTien) + model.DanhSachDichVu.Sum(dv => dv.TongTien);
            model.ThanhTienTruocGiam = tongTienHang;
            model.GiamGia = tongTienHang - hoaDon.TongTien;


            return View(model);
        }
        private bool HoaDonExists(string id)
        {
            return _context.HoaDons.Any(e => e.MaHd == id);
        }
    }
}
