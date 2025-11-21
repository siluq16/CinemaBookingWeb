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
            var rawData = await _context.HoaDons
                .OrderByDescending(hd => hd.NgayLap)
                .Select(hd => new
                {
                    hd.MaHd,
                    hd.TrangThai,
                    hd.TongTien,
                    hd.NgayLap,
                    ThongTinPhim = hd.Ves.Select(v => new
                    {
                        TenPhim = v.MaLichNavigation.MaPhimNavigation.TenPhim,
                        TenPhong = v.MaLichNavigation.MaPhongNavigation.TenPhong,
                        GioBatDau = v.MaLichNavigation.GioBatDau, 
                        GioKetThuc = v.MaLichNavigation.GioKetThuc
                    }).FirstOrDefault()
                })
                .ToListAsync();

            var hoaDonDetails = rawData.Select(item => new HoaDonChiTietVM
            {
                MaHD = item.MaHd,
                TrangThai = item.TrangThai,
                TongTien = item.TongTien,
                NgayLap = item.NgayLap,

                TenPhim = item.ThongTinPhim?.TenPhim ?? "Chưa xác định",
                PhongChieu = item.ThongTinPhim?.TenPhong ?? "",

                SuatChieu = item.ThongTinPhim != null
                    ? $"{item.ThongTinPhim.GioBatDau:HH:mm} - {item.ThongTinPhim.GioKetThuc:HH:mm}"
                    : ""
            }).ToList();
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
                GioBatDau = lichChieu?.GioBatDau.ToString(@"HH:mm") ?? "N/A",
                GioKetThuc = lichChieu?.GioKetThuc.ToString(@"HH:mm") ?? "N/A",
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

            decimal tongTienHang = model.DanhSachGhe.Sum(g => g.GiaTien) + model.DanhSachDichVu.Sum(dv => dv.TongTien);
            model.ThanhTienTruocGiam = tongTienHang;
            model.GiamGia = tongTienHang - hoaDon.TongTien;


            return View(model);
        }
    }
}
