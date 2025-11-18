namespace CinemaBookingWeb.Areas.Admin.Models
{
    public class HoaDonChiTietVM
    {
        // Thông tin Hóa đơn chính
        public string MaHD { get; set; } = null!;
        public string TrangThai { get; set; } = null!;
        public string PhuongThucThanhToan { get; set; } = null!; // Thêm PT thanh toán
        public decimal TongTien { get; set; }
        public DateTime NgayLap { get; set; }
        public string? TenKhuyenMai { get; set; }
        public decimal GiamGia { get; set; } // Giả định tính được số tiền giảm
        public decimal ThanhTienTruocGiam { get; set; }

        // Thông tin Khách hàng
        public string TenKhachHang { get; set; } = null!;
        public string SoDienThoai { get; set; } = null!;
        public string Email { get; set; } = null!;

        // Thông tin Suất chiếu (Lấy từ vé đầu tiên)
        public string TenPhim { get; set; } = null!;
        public string PhongChieu { get; set; } = null!;
        public string SuatChieu { get; set; } = null!;

        public string GioBatDau { get; set; } = null!;
        public string GioKetThuc { get; set; } = null!;
        public DateOnly NgayChieu { get; set; }

        // Danh sách Chi tiết Vé (Ghế)
        public List<VeChiTiet> DanhSachGhe { get; set; } = new List<VeChiTiet>();

        // Danh sách Chi tiết Đồ ăn/Dịch vụ
        public List<DichVuChiTiet> DanhSachDichVu { get; set; } = new List<DichVuChiTiet>();
    }

    public class VeChiTiet
    {
        public string TenGhe { get; set; } = null!;
        public string LoaiGhe { get; set; } = null!;
        public decimal GiaTien { get; set; }
    }

    public class DichVuChiTiet
    {
        public string TenDichVu { get; set; } = null!;
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal TongTien { get; set; }
    }
}
