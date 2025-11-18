using CinemaBookingWeb.Models;
using CinemaBookingWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CinemaBookingWeb.Controllers
{
    public class DatVeController : Controller
    {
        private readonly CinemaBookingWebContext _context;
        public DatVeController(CinemaBookingWebContext context)
        {
            _context = context;
        }
        [HttpGet]
            public async Task<IActionResult> Index(string maLichChieu)
            {
                if (string.IsNullOrEmpty(maLichChieu))
                    return RedirectToAction("Index", "Home");

                var lichChieu = await _context.LichChieus
                    .Include(l => l.MaPhimNavigation)
                    .Include(l => l.MaPhongNavigation)
                    .FirstOrDefaultAsync(l => l.MaLich == maLichChieu);

            if (lichChieu == null)
                    return NotFound();

            var gheNgois = await _context.GheNgois
                    .Where(g => g.MaPhong == lichChieu.MaPhong)
                    .OrderBy(g => g.HangGhe)
                    .ThenBy(g => g.CotGhe)
                    .ToListAsync();

                var gheDaDat = await _context.Ves
                    .Where(v => v.MaLich == maLichChieu)
                    .Select(v => v.MaGhe)
                    .ToListAsync();

                ViewBag.LichChieu = lichChieu;
                ViewBag.GheDaDat = gheDaDat;

                return View(gheNgois);
            }

        [HttpGet]
        public async Task<IActionResult> LoadGhePartial(string maLichChieu)
        {
            var lichChieu = await _context.LichChieus.AsNoTracking().FirstOrDefaultAsync(l => l.MaLich == maLichChieu);

            if (lichChieu == null)
                return NotFound(); // Xử lý nếu lịch chiếu không tìm thấy

            var gheList = await _context.GheNgois
                .Where(g => g.MaPhong == lichChieu.MaPhong) // Chỉ lấy ghế của phòng này
                .OrderBy(g => g.HangGhe)
                .ThenBy(g => g.CotGhe)
                .ToListAsync();

            var gheDaDat = await _context.Ves
                .Where(v => v.MaLich == maLichChieu)
                .Select(v => v.MaGhe)
                .ToListAsync();

            ViewBag.GheDaDat = gheDaDat;

            return PartialView("_GhePartial", gheList);
        }

        [HttpPost]
        public async Task<IActionResult> ChonCombo(string maLichChieu, List<string> maGhe)
        {
            if (maGhe == null || maGhe.Count == 0)
                return Json(new { success = false, message = "Vui lòng chọn ghế." });

            var maKH = User.FindFirstValue("MaKh");
            if (string.IsNullOrEmpty(maKH))
                return Json(new { success = false, message = "Bạn cần đăng nhập để đặt vé." });

            var gheDangGiuHoacDat = await _context.Ves
                .Where(v => v.MaLich == maLichChieu)
                .Include(v => v.MaHdNavigation)
                .Where(v => v.MaHdNavigation.TrangThai == "Paid" ||
                            (v.MaHdNavigation.TrangThai == "Holding" && v.MaHdNavigation.ThoiGianHetHan > DateTime.Now))
                .Select(v => v.MaGhe)
                .ToListAsync();

            var gheBiTrung = maGhe.Intersect(gheDangGiuHoacDat).ToList();
            if (gheBiTrung.Any())
            {
                return Json(new { success = false, message = $"Ghế {string.Join(", ", gheBiTrung)} vừa được người khác chọn hoặc đặt. Vui lòng chọn lại." });
            }
            var lichChieu = await _context.LichChieus
                .FirstOrDefaultAsync(l => l.MaLich == maLichChieu);
            if (lichChieu == null)
                return Json(new { success = false, message = "Không tìm thấy lịch chiếu." });

            string loaiNgay = (lichChieu.NgayChieu.DayOfWeek == DayOfWeek.Saturday || lichChieu.NgayChieu.DayOfWeek == DayOfWeek.Sunday)
                ? "CuoiTuan" : "Thuong";

            var chiTietGhe = await _context.GheNgois
                .Where(g => maGhe.Contains(g.MaGhe))
                .ToDictionaryAsync(g => g.MaGhe, g => g.LoaiGhe);

            var giaVeList = await _context.GiaVes
                .Where(g => g.LoaiNgay == loaiNgay && chiTietGhe.Values.Contains(g.LoaiGhe))
                .ToDictionaryAsync(g => g.LoaiGhe, g => g.Gia);

            string maHD = "HD" + DateTime.Now.ToString("yyyyMMddHHmmss");
            var thoiGianHetHan = DateTime.Now.AddMinutes(5);
            decimal tongTienVe = 0; // Biến tính tổng giá vé
            var hoaDon = new HoaDon
            {
                MaHd = maHD,
                MaKh = maKH,
                NgayLap = DateTime.Now,
                TongTien = 0,
                TrangThai = "Holding", 
                ThoiGianHetHan = thoiGianHetHan
            };
            _context.HoaDons.Add(hoaDon);
            foreach (var ghe in maGhe)
            {
                var loaiGhe = chiTietGhe.GetValueOrDefault(ghe);
                decimal giaVe = 0;
                if (loaiGhe != null && giaVeList.ContainsKey(loaiGhe))
                {
                    giaVe = giaVeList[loaiGhe];
                }

                tongTienVe += giaVe;
                string? tenGhe = ghe.Contains("_") ? ghe.Split('_').LastOrDefault() : ghe;
                string maVeMoi = $"{maHD}_{tenGhe}"; 

                _context.Ves.Add(new Ve
                {
                    MaVe = maVeMoi,
                    MaHd = maHD,
                    MaGhe = ghe,
                    MaLich = maLichChieu,
                    GiaVe = giaVe 
                });
            }
            hoaDon.TongTien = tongTienVe;
            await _context.SaveChangesAsync();
            TempData["MaHoaDonTam"] = maHD;

            var doAnList = await _context.DoAns.ToListAsync();
            ViewBag.MaLich = maLichChieu;
            return PartialView("_ComboPartial", doAnList);
        }
        // Trong DatVeController.cs (Cuối cùng)

        [HttpPost]
        public async Task<JsonResult> TinhGiaVe([FromBody] TinhGiaVeRequestViewModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.MaLichChieu))
                return Json(new { tongGia = 0, error = "Dữ liệu không hợp lệ." });

            var lichChieu = await _context.LichChieus
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.MaLich == model.MaLichChieu);

            if (lichChieu == null)
                return Json(new { tongGia = 0, error = "Không tìm thấy lịch chiếu." });

            var ngayChieu = lichChieu.NgayChieu;
            string loaiNgay = (ngayChieu.DayOfWeek == DayOfWeek.Saturday || ngayChieu.DayOfWeek == DayOfWeek.Sunday)
                ? "CuoiTuan" : "Thuong";

            var giaVeList = _context.GiaVes
                .Where(g => g.LoaiNgay == loaiNgay)
                .ToDictionary(g => g.LoaiGhe, g => g.Gia);

            decimal tongGia = 0;

            foreach (var loaiGhe in model.DanhSachLoaiGhe)
            {
                if (giaVeList.TryGetValue(loaiGhe, out decimal gia))
                {
                    tongGia += gia;
                }
            }

            return Json(new { tongGia });
        }
        // Trong DatVeController.cs

        [HttpGet]
        public async Task<IActionResult> ThanhToan()
        {
            var maHD = TempData["MaHoaDonTam"]?.ToString();
            if (string.IsNullOrEmpty(maHD))
            {
                return RedirectToAction("Index", "Home");
            }

            var hoaDon = await _context.HoaDons
                .FirstOrDefaultAsync(h => h.MaHd == maHD && h.TrangThai == "Holding");

            if (hoaDon == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var thoiGianHetHanMoi = DateTime.Now.AddMinutes(5);
            hoaDon.ThoiGianHetHan = thoiGianHetHanMoi;

            await _context.SaveChangesAsync(); // Lưu thay đổi vào DB

            ViewBag.ThoiGianHetHan = thoiGianHetHanMoi;
            ViewBag.MaHoaDon = maHD;

            TempData["MaHoaDonTam"] = maHD;

            return PartialView("_ThanhToanPartial");
        }

        [HttpPost]
        public async Task<IActionResult> ThanhToan([FromBody] ThanhToanViewModel model)
        {
            var maHD = TempData["MaHoaDonTam"]?.ToString();
            var hoaDon = await _context.HoaDons.FirstOrDefaultAsync(h => h.MaHd == maHD);

            if (hoaDon == null || hoaDon.TrangThai != "Holding")
            {
                return Json(new { success = false, message = "Phiên đặt vé đã hết hạn hoặc không hợp lệ. Vui lòng thử lại." });
            }
            hoaDon.PhuongThucThanhToan = model.PhuongThucThanhToan;
            if (!string.IsNullOrEmpty(model.MaKM))
            {
                hoaDon.MaKm = model.MaKM;
            }
            hoaDon.TongTien = model.TongTien;
            hoaDon.TrangThai = "Paid"; 
            hoaDon.NgayLap = DateTime.Now;
            hoaDon.ThoiGianHetHan = null; 
            foreach (var combo in model.ComboList)
            {
                var ct = new ChiTietDoAn
                {
                    MaHd = maHD,
                    MaDoAn = combo.MaDoAn,
                    SoLuong = combo.SoLuong,
                    ThanhTien = combo.ThanhTien
                };
                _context.ChiTietDoAns.Add(ct);
            }

            await _context.SaveChangesAsync();
            TempData.Remove("MaHoaDonTam");

            return Json(new { success = true, maHD = hoaDon.MaHd, message = "Thanh toán thành công!" });
        }

        [HttpPost]
        public async Task<IActionResult> HuyHold(string maLichChieu)
        {
            var maHD = TempData["MaHoaDonTam"]?.ToString();

            if (!string.IsNullOrEmpty(maHD))
            {
                var hoaDon = await _context.HoaDons
                    .Include(h => h.Ves)
                    .Include(h => h.ChiTietDoAns) // Nếu có
                    .FirstOrDefaultAsync(h => h.MaHd == maHD && h.TrangThai == "Holding");

                if (hoaDon != null)
                {
                    _context.Ves.RemoveRange(hoaDon.Ves);
                    _context.HoaDons.Remove(hoaDon);
                    await _context.SaveChangesAsync();
                    TempData.Remove("MaHoaDonTam");
                    return Json(new { success = true });
                }
            }
            return Json(new { success = true, message = "Không tìm thấy giao dịch tạm thời để hủy, tiếp tục." });
        }

        [HttpPost]
        public async Task<IActionResult> ApDungKhuyenMai([FromBody] ApDungKhuyenMaiViewModel model)
        {
            if (string.IsNullOrEmpty(model.MaKhuyenMai))
                return Json(new { success = false, message = "Vui lòng nhập mã." });

            var khuyenMai = await _context.KhuyenMais
                .FirstOrDefaultAsync(km => km.MaKm == model.MaKhuyenMai);

            if (khuyenMai == null)
                return Json(new { success = false, message = "Mã khuyến mãi không hợp lệ." });
            DateTime ngayKetThuc = khuyenMai.NgayKetThuc.ToDateTime(TimeOnly.MaxValue);

            if (ngayKetThuc < DateTime.Now)
                return Json(new { success = false, message = "Mã khuyến mãi đã hết hạn sử dụng." });

            decimal giamGia = 0;

            if (khuyenMai.PhanTramGiam.HasValue)
            {
                if (khuyenMai.PhanTramGiam.Value > 0)
                {
                    giamGia = model.TongTienBanDau * (khuyenMai.PhanTramGiam.Value / 100m);
                }
            }
            else if (khuyenMai.SoTienGiam.HasValue)
            {
                giamGia = khuyenMai.SoTienGiam.Value;
            }

            if (giamGia <= 0)
            {
                return Json(new { success = false, message = "Mã khuyến mãi không có giá trị giảm giá." });
            }

            return Json(new { success = true, giamGia = Math.Round(giamGia) });
        }
    }
}
