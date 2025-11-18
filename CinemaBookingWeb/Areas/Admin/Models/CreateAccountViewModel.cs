using System.ComponentModel.DataAnnotations;

namespace CinemaBookingWeb.Areas.Admin.Models
{
    public class CreateAccountViewModel
    {
        // --- Thông tin Account ---

        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "Tên đăng nhập phải từ 4 đến 50 ký tự.")]
        public string TenDangNhap { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 ký tự trở lên.")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; } = null!;

        [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống.")]
        [DataType(DataType.Password)]
        [Compare("MatKhau", ErrorMessage = "Xác nhận mật khẩu không khớp.")]
        public string XacNhanMatKhau { get; set; } = null!;

        // --- Thông tin Khách hàng ---

        // Mã khách hàng sẽ được tạo tự động trong Controller

        [Required(ErrorMessage = "Họ tên không được để trống.")]
        public string HoTen { get; set; } = null!;

        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Số điện thoại không được để trống.")]
        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
        [StringLength(15)]
        public string SoDienThoai { get; set; } = null!;

        [DataType(DataType.Date)]
        public DateOnly? NgaySinh { get; set; }

        // Thêm trường cho việc upload Avatar (nếu bạn muốn)
        public IFormFile? AvatarFile { get; set; }
    }
}
