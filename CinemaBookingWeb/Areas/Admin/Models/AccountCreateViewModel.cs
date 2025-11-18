namespace CinemaBookingWeb.Areas.Admin.Models
{
    public class AccountCreateViewModel
    {
        public string? TenDangNhap { get; set; }
        public string? MatKhau { get; set; }
        public string? VaiTro { get; set; } // 'Admin' hoặc 'KhachHang'
        public string? TrangThai { get; set; } // 'Active' hoặc 'Locked'

        // Thông tin KhachHang (Nếu VaiTro là KhachHang)
        public string? HoTen { get; set; }
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public DateOnly? NgaySinh { get; set; }
        public IFormFile? AvatarFile { get; set; }
    }
}
