using System.ComponentModel.DataAnnotations;

namespace CinemaBookingWeb.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Mật khẩu cũ không được để trống.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu cũ")]
        public string OldPassword { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu mới không được để trống.")]
        [StringLength(100, ErrorMessage = "{0} phải có ít nhất {2} và nhiều nhất {1} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; } = null!;

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu mới")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu mới và xác nhận mật khẩu không khớp.")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
