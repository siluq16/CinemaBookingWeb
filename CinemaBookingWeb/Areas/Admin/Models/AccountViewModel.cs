namespace CinemaBookingWeb.Areas.Admin.Models
{
    public class AccountViewModel
    {
        public string TenDangNhap { get; set; } = null!;
        public string VaiTro { get; set; } = null!;
        public string? MaDoiTuong { get; set; }
        public string? TrangThai { get; set; } // Thêm trường này vào ViewModel

        public string? MaKH { get; set; }
        public string? HoTen { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public DateTime? NgayTao { get; set; }
        public string? Avatar { get; set; }
    }
}
